using UInsure.WebApi.DavidJames.DataModels;
using UInsure.WebApi.DavidJames.Models;
using UInsure.WebApi.DavidJames.Services.Exceptions;
using UInsure.WebApi.DavidJames.Services.Extensions;

namespace UInsure.WebApi.DavidJames.Services
{
    public partial class PolicyService
    {
        private async Task ValidateSaleOfPolicyRequest(PolicyModel policy)
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

            if (!policy.Property.Postcode.IsValidUkPostcode())
                throw new GeneralApiException("The provided property postcode is not valid.");
        }

        private async Task<Policy> GetValidatedPolicyBeforeCancellation(string uniqueReference, DateTime cancellationDate)
        {
            if (cancellationDate.Date < DateTime.UtcNow.Date)
                throw new GeneralApiException("Cancellation date is in the past.");

            var policy = await _policyRepository.GetByReferenceWithDetailsAsync(uniqueReference)
                ?? throw new PolicyNotFoundException();

            if (policy.HasClaims)
                throw new GeneralApiException("A policy with an active claim cannot be cancelled using this endpoint.");
            return policy;
        }

        private static void ValidatePolicyCanBeRenewed(Policy policy)
        {
            var today = DateTime.UtcNow.Date;

            if (policy.EndDate <= today)
                throw new GeneralApiException("Cannot renew a policy after the end date.");

            if (policy.EndDate.AddDays(-30) > today)
                throw new GeneralApiException("Policy can only be renewed 30 days before the end date.");

            if (policy.Payment.PaymentType == PaymentType.Cheque)
                throw new GeneralApiException("Only direct debit and card payment policies can be renewed using this method.");
        }

        private async Task<Policy> GetValidatedPolicyBeforeRefundCalculation(string uniqueReference, DateTime cancellationDate)
        {
            if (cancellationDate.Date < DateTime.UtcNow.Date)
                throw new GeneralApiException("Cancellation date is in the past.");

            var policy = await _policyRepository.GetByReferenceReadonlyAsync(uniqueReference)
                ?? throw new PolicyNotFoundException();

            if (policy.HasClaims)
                throw new GeneralApiException("A policy with an active claim cannot be cancelled so no refund is available.");
            return policy;
        }
    }
}
