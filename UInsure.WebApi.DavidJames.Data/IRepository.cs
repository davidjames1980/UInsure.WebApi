namespace UInsure.WebApi.DavidJames.Data
{
    public interface IRepository<TEntity>
    {
        Task AddAsync(TEntity policy);
        Task UpdateAsync(TEntity policy);
        Task SaveChangesAsync();
    }
}
