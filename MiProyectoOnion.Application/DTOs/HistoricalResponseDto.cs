using MiProyectoOnion.Domain.Entities;

namespace MiProyectoOnion.Application.DTOs;

public record HistoricalResponseDto()
{
    public string PetName { get; init; }
    public string OwnerName { get; init; } 
    public string Species { get; init; } 
    public List<AppointmentDetailDto> Appointments { get; init; } = new();
}