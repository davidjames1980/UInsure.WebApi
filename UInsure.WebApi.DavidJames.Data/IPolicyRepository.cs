using UInsure.WebApi.DavidJames.DataModels;

namespace UInsure.WebApi.DavidJames.Data
{
    public interface IPolicyRepository : IRepository<Policy>
    {
        Task<Policy?> GetByReferenceReadonlyAsync(string uniqueReference);
        Task<Policy?> GetByReferenceAsync(string uniqueReference);
        Task<Policy?> GetByReferenceWithPaymentAsync(string uniqueReference);
        Task<Policy?> GetByReferenceWithDetailsAsync(string uniqueReference);
        Task<bool> ExistsAsync(string uniqueReference);
    }
}