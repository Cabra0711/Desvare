using MiProyectoOnion.Application.DTOs;

namespace MiProyectoOnion.Application.Services;

public interface ITreatmentService
{
    Task<bool> CreateTreatmentAsync(TreatmentCreateDto treatmentDto);
    Task<IEnumerable<TreatmentResponseDto>> GetByAppointmentAsync(int appointmentId);
}
