using UInsure.WebApi.DavidJamesForbes.DataModels;
using UInsure.WebApi.DavidJamesForbes.Models;

namespace UInsure.WebApi.DavidJamesForbes.Services
{
    public interface IPolicyService
    {
        Task<decimal> CalculateCancellationRefund(string uniqueReference, DateTime cancellationDate);
        Task<decimal> CancelPolicy(string uniqueReference, DateTime cancellationDate);
        Task<CanRenewResponseModel> CanRenewPolicy(string uniqueReference);
        Task<PolicyModel> GetPolicy(string uniqueReference);
        Task<Policy> RenewPolicy(string uniqueReference);
        Task<Policy> SellPolicy(PolicyModel policy);
    }
}