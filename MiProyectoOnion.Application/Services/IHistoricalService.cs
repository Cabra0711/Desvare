using MiProyectoOnion.Application.DTOs;

namespace MiProyectoOnion.Application.Services;

public interface IHistoricalService
{
    Task<HistoricalResponseDto?> GetHistoryByPetIdAsync(int petId);
}
