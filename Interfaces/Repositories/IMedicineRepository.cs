using MiProyectoOnion.Domain.Entities;

namespace MiProyectoOnion.Domain.Interfaces.Repositories;

public interface IMedicineRepository
{
    Task<IEnumerable<Medicine>> GetAllAsync();
    Task<Medicine?> GetByIdAsync(int id);
}