using MiProyectoOnion.Domain.Entities;

namespace MiProyectoOnion.Domain.Interfaces.Repositories;

public interface IAppoinmentRepository : IGenericRepository<Appointment>
{
    Task<Appointment> GetByAppointmentAsync(int id, Veterinarian veterinarian);
    Task<Appointment> GetByPetAsync(int petId);
    Task<bool> HasOverlapAsync(int veterinarianId, DateTime start, DateTime end);
    Task<int> CountActiveByPetAsync(int petId); 
    Task<int> CountAbsencesByPetAsync(int petId);
}