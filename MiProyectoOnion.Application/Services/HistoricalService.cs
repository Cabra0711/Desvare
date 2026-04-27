using MiProyectoOnion.Domain.Entities;
using MiProyectoOnion.Domain.Interfaces.Repositories;

namespace MiProyectoOnion.Infrastructure.Repositories;

public abstract class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
{
    protected readonly List<T> _data = new();
    private int _nextId = 1;

    public Task<IEnumerable<T>> GetAll() => Task.FromResult<IEnumerable<T>>(_data.ToList());
    public Task<T?> GetById(int id) => Task.FromResult(_data.FirstOrDefault(e => e.Id == id));

    public Task AddAsync(T entity)
    {
        entity.Id = _nextId++;
        entity.CreatedAt = DateTime.Now;
        _data.Add(entity);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(T entity) { entity.UpdatedAt = DateTime.Now; return Task.CompletedTask; }
    public Task DeleteAsync(T entity) { _data.Remove(entity); return Task.CompletedTask; }
}