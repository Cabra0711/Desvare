using MiProyectoOnion.Application.DTOs;
using MiProyectoOnion.Domain.Entities;
using MiProyectoOnion.Domain.Interfaces.Repositories;
using MiProyectoOnion.Domain.Interfaces.Services;

namespace MiProyectoOnion.Application.Services;

public class OwnerService : IOwnerService
{
    private readonly IOwnerRepository _ownerRepository;
    public  OwnerService(IOwnerRepository ownerRepository)
    {
        _ownerRepository = ownerRepository;
    }

    public async Task<bool> CreateOwnerAsync(OwnerCreateDto ownerDto)
    {
        var response = await _ownerRepository.GetByDocumentAsync(ownerDto.Document);
        if (response != null)
        {
            return false;
        }
        else
        {
            var newOwner = new Owner{Name = ownerDto.Name, Document = ownerDto.Document, Email = ownerDto.Email, Phone = ownerDto.Phone};
            await _ownerRepository.AddAsync(newOwner);
            return true;
        }
    }

    public async Task<IEnumerable<OwnerResponseDto>> GetAllOwnersAsync()
    {
        var response = await _ownerRepository.GetAll();
        return response.Select(o => new OwnerResponseDto
        {
            Name = o.Name,
            Email = o.Email,
            Phone = o.Phone,
            TotalPets = o.Pets.Count()
        });
    }

    public async Task<OwnerResponseDto?> GetByDocumentAsync(string document)
    {
        var owner = await _ownerRepository.GetByDocumentAsync(document);
        if (owner == null) return null;
        {
            return new OwnerResponseDto
            {
                Name = owner.Name,
                Email = owner.Email,
                Phone = owner.Phone,
                TotalPets = owner.Pets?.Count() ?? 0
            };
        }
    }
    
}