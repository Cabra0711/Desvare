namespace MiProyectoOnion.Application.DTOs;

public record AppoinmentResponseDto()
{
    public int Id { get; init; }
    public string VeterinarianName { get; init; }
    public string PetName { get; init; }
    public DateTime AppoimentDate { get; init; }
    public TimeOnly StartTime { get; init; }
    public TimeOnly EndTime { get; init; }
    public bool IsActive { get; init; }
}