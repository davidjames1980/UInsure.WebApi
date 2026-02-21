using AutoMapper;
using UInsure.WebApi.DavidJames.DataModels;
using UInsure.WebApi.DavidJames.Models;
using UInsure.WebApi.DavidJames.Services.Exceptions;

namespace UInsure.WebApi.DavidJames.Services
{
    public class PolicyService : IPolicyService
    {
        private readonly IPolicyRepository _policyRepository;
        private readonly IMapper _autoMapper;

        public PolicyService(IPolicyRepository policyRepository, IMapper autoMapper)
        {
            _policyRepository = policyRepository;
            _autoMapper = autoMapper;
        }

        public async Task<Policy> SellPolicy(PolicyModel policy)
        {
            // Policy reference must be unique
            if (await _policyRepository.ExistsAsync(policy.UniqueReference))
                throw new GeneralApiException("The provided policy UniqueReference already exists. Please provide a unique reference");

            // A policy can only be sold up to 60 days in advance
            if (policy.StartDate.Date > DateTime.UtcNow.Date.AddDays(60) || policy.StartDate.Date < DateTime.UtcNow.Date)
                throw new GeneralApiException("The policy StartDate must be within the next 60 days");

            // The policy must always be one year in length
            if (policy.EndDate.Date != policy.StartDate.AddYears(1))
                throw new GeneralApiException("The policy must last one annual year.");

            // At least 1 but no more than 3 policy holders
            if (policy.Policyholders.Count < 1 || policy.Policyholders.Count > 3)
                throw new GeneralApiException("The policy must have between 1 and 3 policy holders.");

            // Each policyholder must be OVER 16 on the policy start date
            foreach (var policyHolder in policy.Policyholders)
            {
                if (policyHolder.DateOfBirth.Date >= policy.StartDate.Date.AddYears(-16))
                    throw new GeneralApiException("All policy holders must be over the age of 16 on the date the policy starts.");
            }

            var dataModel = _autoMapper.Map<Policy>(policy);

            dataModel.Payment = new Payment
            {
                PaymentReference = Guid.NewGuid().ToString("N"),
                Amount = dataModel.Amount,
                PaymentType = _autoMapper.Map<PaymentType>(policy.PaymentType)
            };

            await _policyRepository.AddAsync(dataModel);
            await _policyRepository.SaveChangesAsync();

            return dataModel;
        }

        public async Task<PolicyModel> GetPolicy(string uniqueReference)
        {
            var policy = await _policyRepository.GetByReferenceWithDetailsAsync(uniqueReference)
                ?? throw new PolicyNotFoundException();

            var model = _autoMapper.Map<PolicyModel>(policy);
            model.PaymentType = _autoMapper.Map<PaymentTypeModel>(policy.Payment.PaymentType);

            return model;
        }

        public async Task<decimal> CancelPolicy(string uniqueReference, DateTime cancellationDate)
        {
            if (cancellationDate.Date < DateTime.UtcNow.Date)
                throw new GeneralApiException("Cancellation date is in the past.");

            var policy = await _policyRepository.GetByReferenceWithDetailsAsync(uniqueReference)
                ?? throw new PolicyNotFoundException();

            if (policy.HasClaims)
                throw new GeneralApiException("A policy with an active claim cannot be cancelled using this method.");

            // Full refund if before start date or within the 14-day cool-off period
            if (cancellationDate < policy.StartDate ||
                cancellationDate >= policy.StartDate && cancellationDate <= policy.StartDate.AddDays(14))
            {
                return policy.Amount;
            }

            var refund = CalculateRefund(cancellationDate, policy);

            // At this point we would process the refund via a payment merchant

            // I'd imagine we would have a PolicyCancelled bool at least on the Policy but as it isn't in the spec
            // I shall leave it for now.

            return refund;
        }

        public async Task<Policy> RenewPolicy(string uniqueReference)
        {
            var policy = await _policyRepository.GetByReferenceWithPaymentAsync(uniqueReference)
                ?? throw new PolicyNotFoundException();

            ValidatePolicyCanBeRenewed(policy);

            policy.StartDate = policy.EndDate;
            policy.EndDate = policy.EndDate.AddYears(1);

            if (policy.AutoRenew)
            {
                // Call a payment merchant based on existing payment data
            }

            await _policyRepository.UpdateAsync(policy);
            await _policyRepository.SaveChangesAsync();

            return policy;
        }

        public async Task<decimal> CalculateCancellationRefund(string uniqueReference, DateTime cancellationDate)
        {
            if (cancellationDate.Date < DateTime.UtcNow.Date)
                throw new GeneralApiException("Cancellation date is in the past.");

            var policy = await _policyRepository.GetByReferenceReadonlyAsync(uniqueReference)
                ?? throw new PolicyNotFoundException();

            if (policy.HasClaims)
                throw new GeneralApiException("A policy with an active claim cannot be cancelled so no refund is available.");

            return CalculateRefund(cancellationDate, policy);
        }

        public async Task<CanRenewResponseModel> CanRenewPolicy(string uniqueReference)
        {
            var policy = await _policyRepository.GetByReferenceWithPaymentAsync(uniqueReference)
                ?? throw new PolicyNotFoundException();

            try
            {
                ValidatePolicyCanBeRenewed(policy);
            }
            catch (GeneralApiException ex)
            {
                return new CanRenewResponseModel { CanRenew = false, Reason = ex.Message };
            }

            return new CanRenewResponseModel { CanRenew = true };
        }

        private static void ValidatePolicyCanBeRenewed(Policy policy)
        {
            var today = DateTime.Now;

            if (policy.EndDate <= today)
                throw new GeneralApiException("Cannot renew a policy after the end date.");

            if (policy.EndDate.AddDays(-30) > today)
                throw new GeneralApiException("Policy can only be renewed 30 days before the end date.");

            if (policy.Payment.PaymentType == PaymentType.Cheque)
                throw new GeneralApiException("Only direct debit and card payment policies can be renewed using this method.");
        }

        private static decimal CalculateRefund(DateTime cancellationDate, Policy policy)
        {
            var totalPolicyDays = (policy.EndDate.Date - policy.StartDate.Date).Days;

            // Using the assumption that the date of cancellation is insured hence the + 1
            var usedDays = (cancellationDate.Date - policy.StartDate.Date).Days + 1;
            var unusedDays = totalPolicyDays - usedDays;

            var refund = policy.Amount * unusedDays / totalPolicyDays;

            return Math.Round(refund, 2, MidpointRounding.AwayFromZero);
        }
    }
}