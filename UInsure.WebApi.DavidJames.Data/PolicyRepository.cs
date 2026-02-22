using Microsoft.EntityFrameworkCore;
using UInsure.WebApi.DavidJames.Entities;

namespace UInsure.WebApi.DavidJames.Data
{
    /// <summary>
    /// We are using a Repo pattern because we need to unit test this bad-boy.  Impossible without DI.
    /// </summary>
    public class PolicyRepository : IPolicyRepository
    {
        private readonly UinsureDbContext _context;

        public PolicyRepository(UinsureDbContext context)
        {
            _context = context;
        }

        public async Task<Policy?> GetByReferenceAsync(string uniqueReference)
        {
            return await _context.Policies
                .FirstOrDefaultAsync(p => p.UniqueReference == uniqueReference);
        }

        // A case could be made for splitting a high use repository
        // into an IReadOnlyPolicyRepository and IPolicyRepository. AsNoTracking can make a huge difference under high read loads.
        // If this was for an enterprise solution I would head in that direction.
        public async Task<Policy?> GetByReferenceReadonlyAsync(string uniqueReference)
        {
            return await _context.Policies
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.UniqueReference == uniqueReference);
        }

        public async Task<Policy?> GetByReferenceWithPaymentAsync(string uniqueReference)
        {
            return await _context.Policies
                .Include(p => p.Payment)
                .FirstOrDefaultAsync(p => p.UniqueReference == uniqueReference);
        }

        public async Task<Policy?> GetByReferenceWithDetailsAsync(string uniqueReference)
        {
            return await _context.Policies
                .Include(p => p.Policyholders)
                .Include(p => p.Payment)
                .Include(p => p.Property)
                .FirstOrDefaultAsync(p => p.UniqueReference == uniqueReference);
        }

        public async Task<bool> ExistsAsync(string uniqueReference)
        {
            return await _context.Policies
                .AnyAsync(p => p.UniqueReference == uniqueReference);
        }

        public async Task AddAsync(Policy policy)
        {
            await _context.Policies.AddAsync(policy);
        }

        public Task UpdateAsync(Policy policy)
        {
            _context.Policies.Update(policy);
            return Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
