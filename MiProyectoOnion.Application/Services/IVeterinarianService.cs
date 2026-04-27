using MiProyectoOnion.Application.DTOs;
using MiProyectoOnion.Domain.Enums;

namespace MiProyectoOnion.Application.Services;

public interface IVeterinarianService
{
    Task<bool> CreateAsync(VeterinarianCreateDto veterinarianCreateDto);
    Task<VeterinarianResponseDto?> GetByDocumentAsync(string document, Speciality speciality);
    Task<IEnumerable<VeterinarianResponseDto>> GetAllVeterinarianAsync();
    Task<bool> IsAvalableAsync(int id, ScheduleTime schedule);
}