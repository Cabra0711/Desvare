using MiProyectoOnion.Application.DTOs;
using MiProyectoOnion.Domain.Entities;
using MiProyectoOnion.Domain.Enums;
using MiProyectoOnion.Domain.Interfaces.Repositories;

namespace MiProyectoOnion.Application.Services;

public class TreatmentService : ITreatmentService
{
    private readonly ITreatmentRepository _treatmentRepository;
    private readonly IAppoinmentRepository _appointmentRepository;
    private readonly IMedicineRepository _medicineRepository;

    public TreatmentService(
        ITreatmentRepository treatmentRepository,
        IAppoinmentRepository appointmentRepository,
        IMedicineRepository medicineRepository)
    {
        _treatmentRepository = treatmentRepository;
        _appointmentRepository = appointmentRepository;
        _medicineRepository = medicineRepository;
    }

    public async Task<bool> CreateTreatmentAsync(TreatmentCreateDto treatmentDto)
    {
        // Regla: Solo se puede registrar tratamiento en citas con estado "Atendida"
        var appointment = await _appointmentRepository.GetById(treatmentDto.AppointmentId);
        if (appointment == null) return false;
        if (appointment.Status != AppoimentStatus.Attended) return false;

        // Regla: El medicamento debe existir y estar disponible
        var medicineAvailable = await _treatmentRepository.IsAvailableAsync(treatmentDto.MedicineId);
        if (!medicineAvailable) return false;

        var newTreatment = new Treatment
        {
            AppointmentId = treatmentDto.AppointmentId,
            MedicineId = treatmentDto.MedicineId,
            Diagnosis = treatmentDto.Diagnosis,
            Observations = treatmentDto.Observations
        };

        await _treatmentRepository.AddAsync(newTreatment);
        return true;
    }

    public async Task<IEnumerable<TreatmentResponseDto>> GetByAppointmentAsync(int appointmentId)
    {
        var treatments = await _treatmentRepository.GetAll();
        return treatments
            .Where(t => t.AppointmentId == appointmentId)
            .Select(t => new TreatmentResponseDto
            {
                Id = t.Id,
                MedicineName = t.Medicine?.Name ?? "N/A",
                Diagnosis = t.Diagnosis,
                Observations = t.Observations
            });
    }
}
