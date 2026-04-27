using MiProyectoOnion.Domain.Enums;

namespace MiProyectoOnion.Application.DTOs;

public record VeterinarianCreateDto()
{
    public string Name { get; init; }
    public string Document { get; init; }
    public Speciality Speciality { get; init; }
    public ScheduleTime Schedule { get; init; }
    
}