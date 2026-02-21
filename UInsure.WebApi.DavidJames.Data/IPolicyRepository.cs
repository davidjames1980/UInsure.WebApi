using UInsure.WebApi.DavidJames.DataModels;

public interface IPolicyRepository
{
    Task<Policy?> GetByReferenceReadonlyAsync(string uniqueReference);
    Task<Policy?> GetByReferenceAsync(string uniqueReference);
    Task<Policy?> GetByReferenceWithPaymentAsync(string uniqueReference);
    Task<Policy?> GetByReferenceWithDetailsAsync(string uniqueReference);
    Task<bool> ExistsAsync(string uniqueReference);
    Task AddAsync(Policy policy);
    Task UpdateAsync(Policy policy);
    Task SaveChangesAsync();
}