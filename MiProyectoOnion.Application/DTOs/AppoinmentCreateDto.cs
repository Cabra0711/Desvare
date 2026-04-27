using MiProyectoOnion.Domain.Enums;

namespace MiProyectoOnion.Application.DTOs;

public record AppoinmentCreateDto()
{
    public int PetId { get; init; }
    public int VeterinarianId { get; init; }
    public DateTime AppoimentDate { get; init; }
    public TimeOnly StartTime { get; init; }
    public TimeOnly EndTime { get; init; }
}