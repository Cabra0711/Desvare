using MiProyectoOnion.Domain.Entities;
using MiProyectoOnion.Domain.Enums;

namespace MiProyectoOnion.Domain.Interfaces.Repositories;

public interface IVeterinarianRepository : IGenericRepository<Veterinarian>
{
    Task<Veterinarian> GetByDocumentAsync(string document, Speciality speciality);
    Task<bool> IsAvalableAsync(int id, ScheduleTime schedule);
}