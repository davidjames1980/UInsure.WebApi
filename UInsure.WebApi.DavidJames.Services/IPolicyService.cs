using UInsure.WebApi.DavidJames.Entities;
using UInsure.WebApi.DavidJames.Models;
using UInsure.WebApi.DavidJames.Models.Responses;

namespace UInsure.WebApi.DavidJames.Services
{
    public interface IPolicyService
    {
        Task<decimal> CalculateCancellationRefund(string uniqueReference, DateTime cancellationDate);
        Task<decimal> CancelPolicy(string uniqueReference, DateTime cancellationDate);
        Task<CanRenewResponse> CanRenewPolicy(string uniqueReference);
        Task<PolicyModel> GetPolicy(string uniqueReference);
        Task<Policy> RenewPolicy(string uniqueReference);
        Task<Policy> SellPolicy(PolicyModel policy);
    }
}