using MiProyectoOnion.Application.DTOs;
using MiProyectoOnion.Domain.Entities;
using MiProyectoOnion.Domain.Interfaces.Repositories;

namespace MiProyectoOnion.Application.Services;

public class PetService : IPetService
{
    private readonly IPetRepository _petRepository;
    private readonly IOwnerRepository _ownerRepository;

    public PetService(IPetRepository petRepository, IOwnerRepository ownerRepository)
    {
        _petRepository = petRepository;
        _ownerRepository = ownerRepository;
    }

    public async Task<bool> CreatePetAsync(PetCreateDto petDto)
    {
        var owner = await _ownerRepository.GetById(petDto.OwnerId);
        if (owner == null) return false;

        var newPet = new Pet()
        {
            Name = petDto.Name,
            Age = petDto.Age,
            Race = petDto.Race,
            Weight = petDto.Weight,
            OwnerId = petDto.OwnerId
        };

        await _petRepository.AddAsync(newPet);
        return true;
    }

    public async Task<IEnumerable<PetResponseDto>> GetAllPetsAsync()
    {
        var pets = await _petRepository.GetAll();
        return pets.Select(p => new PetResponseDto
        {
            Id = p.Id,
            Name = p.Name,
            Age = p.Age,
            Weight = p.Weight,
            OwnerName = p.Owner?.Name ?? "N/A"
        });
    }

    public async Task<PetResponseDto?> GetByIdAsync(int id)
    {
        var pet = await _petRepository.GetById(id);
        if (pet == null) return null;

        return new PetResponseDto
        {
            Id = pet.Id,
            Name = pet.Name,
            Age = pet.Age,
            Weight = pet.Weight,
            OwnerName = pet.Owner?.Name ?? "N/A"
        };
    }
}
