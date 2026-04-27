using MiProyectoOnion.Application.DTOs;

namespace MiProyectoOnion.Application.Services;

public interface IAppointmentService
{
    Task<bool> CreateAppointmentAsync(AppoinmentCreateDto appointmentDto);
    Task<IEnumerable<AppoinmentResponseDto>> GetAllAppointmentsAsync();
    Task<bool> CancelAppointmentAsync(int appointmentId);
    Task<IEnumerable<AppoinmentResponseDto>> GetActiveByPetAsync(int petId);
}
