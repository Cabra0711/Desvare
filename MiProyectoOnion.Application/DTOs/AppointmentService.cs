using MiProyectoOnion.Application.DTOs;
using MiProyectoOnion.Domain.Entities;
using MiProyectoOnion.Domain.Enums;
using MiProyectoOnion.Domain.Interfaces.Repositories;
using MiProyectoOnion.Domain.Interfaces.Services;

namespace MiProyectoOnion.Application.Services;

public class AppointmentService : IAppointmentService
{
    private readonly IAppoinmentRepository _appointmentRepository;
    private readonly IVeterinarianRepository _veterinarianRepository;
    private readonly IPetRepository _petRepository;
    private readonly IEmailService _emailService;

    public AppointmentService(
        IAppoinmentRepository appointmentRepository,
        IVeterinarianRepository veterinarianRepository,
        IPetRepository petRepository,
        IEmailService emailService)
    {
        _appointmentRepository = appointmentRepository;
        _veterinarianRepository = veterinarianRepository;
        _petRepository = petRepository;
        _emailService = emailService;
    }

    public async Task<bool> CreateAppointmentAsync(AppoinmentCreateDto appointmentDto)
    {
        // Regla 1: Maximo 2 citas activas por mascota
        var activeCitas = await _appointmentRepository.CountActiveByPetAsync(appointmentDto.PetId);
        if (activeCitas >= 2) return false;

        // Regla 2: 3 inasistencias = bloqueo 7 dias
        var absences = await _appointmentRepository.CountAbsencesByPetAsync(appointmentDto.PetId);
        if (absences >= 3) return false;

        // Regla 3: No solapamiento de horarios del veterinario
        var start = appointmentDto.AppoimentDate
            .Date
            .Add(appointmentDto.StartTime.ToTimeSpan());
        var end = appointmentDto.AppoimentDate
            .Date
            .Add(appointmentDto.EndTime.ToTimeSpan());

        var hasOverlap = await _appointmentRepository.HasOverlapAsync(
            appointmentDto.VeterinarianId, start, end);
        if (hasOverlap) return false;

        // Regla 4: El veterinario debe existir y estar activo
        var vet = await _veterinarianRepository.GetById(appointmentDto.VeterinarianId);
        if (vet == null || vet.Schedule != ScheduleTime.Active) return false;

        // Regla 5: La mascota debe existir
        var pet = await _petRepository.GetById(appointmentDto.PetId);
        if (pet == null) return false;

        var newAppointment = new Appointment
        {
            PetId = appointmentDto.PetId,
            VeterinarianId = appointmentDto.VeterinarianId,
            AppointmentDate = appointmentDto.AppoimentDate,
            StartTime = appointmentDto.StartTime,
            EndTime = appointmentDto.EndTime,
            Status = AppoimentStatus.Schechuled
        };

        await _appointmentRepository.AddAsync(newAppointment);

        // Notificacion al dueno por email
        if (pet.Owner?.Email != null)
        {
            _emailService.SendEmail(
                pet.Owner.Email,
                "Cita agendada",
                $"Se ha agendado una cita para {pet.Name} el {appointmentDto.AppoimentDate:dd/MM/yyyy} de {appointmentDto.StartTime} a {appointmentDto.EndTime}."
            );
        }

        return true;
    }

    public async Task<IEnumerable<AppoinmentResponseDto>> GetAllAppointmentsAsync()
    {
        var appointments = await _appointmentRepository.GetAll();
        return appointments.Select(a => new AppoinmentResponseDto
        {
            Id = a.Id,
            VeterinarianName = a.Veterinarian?.Name ?? "N/A",
            PetName = a.Pets?.Name ?? "N/A",
            AppoimentDate = a.AppointmentDate,
            StartTime = a.StartTime,
            EndTime = a.EndTime,
            IsActive = a.Status == AppoimentStatus.Schechuled
        });
    }

    public async Task<bool> CancelAppointmentAsync(int appointmentId)
    {
        var appointment = await _appointmentRepository.GetById(appointmentId);
        if (appointment == null) return false;
        if (appointment.Status != AppoimentStatus.Schechuled) return false;

        appointment.Status = AppoimentStatus.Cancelled;
        await _appointmentRepository.UpdateAsync(appointment);
        return true;
    }

    public async Task<IEnumerable<AppoinmentResponseDto>> GetActiveByPetAsync(int petId)
    {
        var appointments = await _appointmentRepository.GetAll();
        return appointments
            .Where(a => a.PetId == petId && a.Status == AppoimentStatus.Schechuled)
            .Select(a => new AppoinmentResponseDto
            {
                Id = a.Id,
                VeterinarianName = a.Veterinarian?.Name ?? "N/A",
                PetName = a.Pets?.Name ?? "N/A",
                AppoimentDate = a.AppointmentDate,
                StartTime = a.StartTime,
                EndTime = a.EndTime,
                IsActive = true
            });
    }
}
