namespace MiProyectoOnion.Application.DTOs;

public record TreatmentCreateDto()
{
    public int AppointmentId { get; init; }
    public int MedicineId { get; init; }
    public string Diagnosis { get; init; }
    public string Observations { get; init; }
}