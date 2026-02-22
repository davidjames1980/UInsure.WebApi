using AutoMapper;
using UInsure.WebApi.DavidJames.Data;
using UInsure.WebApi.DavidJames.DataModels;
using UInsure.WebApi.DavidJames.Models;
using UInsure.WebApi.DavidJames.Models.Responses;
using UInsure.WebApi.DavidJames.Services.Exceptions;

namespace UInsure.WebApi.DavidJames.Services
{
    public partial class PolicyService(IPolicyRepository policyRepository, IMapper autoMapper) : IPolicyService
    {
        private readonly IPolicyRepository _policyRepository = policyRepository;
        private readonly IMapper _autoMapper = autoMapper;

        public async Task<Policy> SellPolicy(PolicyModel policy)
        {
            await ValidateSaleOfPolicyRequest(policy);

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
            Policy policy = await ValidateCancellationOfPolicyRequest(uniqueReference, cancellationDate);

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
            Policy policy = await ValidateCalculateRefundRequest(uniqueReference, cancellationDate);

            return CalculateRefund(cancellationDate, policy);
        }

        public async Task<CanRenewResponse> CanRenewPolicy(string uniqueReference)
        {
            var policy = await _policyRepository.GetByReferenceWithPaymentAsync(uniqueReference)
                ?? throw new PolicyNotFoundException();

            try
            {
                ValidatePolicyCanBeRenewed(policy);
            }
            catch (GeneralApiException ex)
            {
                return new CanRenewResponse { CanRenew = false, Reason = ex.Message };
            }

            return new CanRenewResponse { CanRenew = true };
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