using MiProyectoOnion.Domain.Entities;

namespace MiProyectoOnion.Domain.Interfaces.Repositories;

// We define a Dynamic class which means that it needs to come with an specific object
// to Enter to the DB for working with that specific object and in a future return Types NULL
public interface IGenericRepository<T> where T : BaseEntity
{
    Task<IEnumerable<T>> GetAll();
    Task<T?> GetById(int id);
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
}