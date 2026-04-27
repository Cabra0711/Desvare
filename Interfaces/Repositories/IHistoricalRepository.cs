using MiProyectoOnion.Domain.Entities;

namespace MiProyectoOnion.Domain.Interfaces.Repositories;

public interface IHistoricalRepository : IGenericRepository<Historical>
{
    Task<Historical> GetHistoryByPetIdAsync(int petId);    
}