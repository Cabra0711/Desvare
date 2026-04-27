using MiProyectoOnion.Application.DTOs;

namespace MiProyectoOnion.Application.Services;

public interface IOwnerService
{
    Task<bool> CreateOwnerAsync(OwnerCreateDto ownerDto);
    Task<IEnumerable<OwnerResponseDto>> GetAllOwnersAsync();
    Task<OwnerResponseDto?> GetByDocumentAsync(string document);
}