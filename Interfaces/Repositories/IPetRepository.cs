using MiProyectoOnion.Domain.Entities;

namespace MiProyectoOnion.Domain.Interfaces.Repositories;

public interface IPetRepository : IGenericRepository<Pet>
{
    Task<IEnumerable<Pet>> GetAllAsync();
    Task<Pet?> GetByIdAsync(int id);
    Task<IEnumerable<Pet>> GetByOwnerIdAsync(int ownerId);
}