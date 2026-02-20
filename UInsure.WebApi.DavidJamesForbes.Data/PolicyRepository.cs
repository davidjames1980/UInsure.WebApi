using Microsoft.EntityFrameworkCore;
using UInsure.WebApi.DavidJamesForbes.DataModels;

namespace UInsure.WebApi.DavidJamesForbes.Data.Repositories
{
    public class PolicyRepository : IPolicyRepository
    {
        private readonly UinsureDbContext _context;

        public PolicyRepository(UinsureDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Returns a policy by unique reference — no related entities included.
        /// Use for operations that only need the core policy (e.g. renew, can-renew).
        /// </summary>
        public async Task<Policy?> GetByReferenceAsync(string uniqueReference)
        {
            return await _context.Policies
                .FirstOrDefaultAsync(p => p.UniqueReference == uniqueReference);
        }

        /// <summary>
        /// Returns a policy with Payment included.
        /// Use for renew/can-renew operations, which need PaymentType for validation.
        /// </summary>
        public async Task<Policy?> GetByReferenceWithPaymentAsync(string uniqueReference)
        {
            return await _context.Policies
                .Include(p => p.Payment)
                .FirstOrDefaultAsync(p => p.UniqueReference == uniqueReference);
        }

        /// <summary>
        /// Returns a policy with all related entities included.
        /// Use for operations that need policyholders, payment, and property (e.g. get, cancel).
        /// </summary>
        public async Task<Policy?> GetByReferenceWithDetailsAsync(string uniqueReference)
        {
            return await _context.Policies
                .Include(p => p.Policyholders)
                .Include(p => p.Payment)
                .Include(p => p.Property)
                .FirstOrDefaultAsync(p => p.UniqueReference == uniqueReference);
        }

        /// <summary>
        /// Checks whether a policy with the given unique reference already exists.
        /// </summary>
        public async Task<bool> ReferenceExistsAsync(string uniqueReference)
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
