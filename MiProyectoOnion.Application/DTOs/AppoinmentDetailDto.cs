namespace MiProyectoOnion.Application.DTOs;

public record AppointmentDetailDto()
{
    public DateTime Date { get; init; }
    public string VeterinarianName { get; init; } 
    public string Diagnosis { get; init; } 
    public string TreatmentObservations { get; init; } 
}