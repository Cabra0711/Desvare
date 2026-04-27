using MiProyectoOnion.Application.DTOs;

namespace MiProyectoOnion.Application.Services;

public interface IPetService 
{
    Task<bool> CreatePetAsync(PetCreateDto petDto);
    Task<IEnumerable<PetResponseDto>> GetAllPetsAsync();
    Task<PetResponseDto?> GetByIdAsync(int id);
}