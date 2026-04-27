using MiProyectoOnion.Domain.Enums;

namespace MiProyectoOnion.Application.DTOs;

public record VeterinarianResponseDto()
{
    public int Id { get; init; }
    public string Name { get; init; }
    public string Document { get; init; }
    public Speciality Speciality { get; init; }
    public bool IsActive { get; init; }
}