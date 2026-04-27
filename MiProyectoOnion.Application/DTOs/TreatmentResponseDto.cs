namespace MiProyectoOnion.Application.DTOs;

public record TreatmentResponseDto()
{
    public int Id { get; init; }
    public string MedicineName { get; init; }
    public string Diagnosis { get; init; }
    public string Observations { get; init; }
}