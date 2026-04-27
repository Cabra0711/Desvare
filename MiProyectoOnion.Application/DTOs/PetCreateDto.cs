using MiProyectoOnion.Domain.Enums;

namespace MiProyectoOnion.Application.DTOs;

public record PetCreateDto()
{
    public string Name { get; init; }
    public PetRace Race { get; init; }
    public int Age { get; init; }
    public decimal Weight { get; init; }
    public int OwnerId { get; init; }
}