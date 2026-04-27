using MiProyectoOnion.Domain.Entities;

namespace MiProyectoOnion.Domain.Interfaces.Repositories;

public interface ITreatmentRepository : IGenericRepository<Treatment>
{
    Task<bool> HasAttendedAsync(int veterinarianId, int appoinmentId);
    Task<bool> IsAvailableAsync(int medicineId);
}