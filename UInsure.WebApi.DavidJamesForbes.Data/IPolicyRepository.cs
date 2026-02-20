using UInsure.WebApi.DavidJamesForbes.DataModels;

public interface IPolicyRepository
{
    Task<Policy?> GetByReferenceAsync(string uniqueReference);
    Task<Policy?> GetByReferenceWithPaymentAsync(string uniqueReference);
    Task<Policy?> GetByReferenceWithDetailsAsync(string uniqueReference);
    Task<bool> ReferenceExistsAsync(string uniqueReference);
    Task AddAsync(Policy policy);
    Task UpdateAsync(Policy policy);
    Task SaveChangesAsync();
}