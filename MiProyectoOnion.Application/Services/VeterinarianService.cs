using MiProyectoOnion.Application.DTOs;
using MiProyectoOnion.Domain.Entities;
using MiProyectoOnion.Domain.Enums;
using MiProyectoOnion.Domain.Interfaces.Repositories;

namespace MiProyectoOnion.Application.Services;

public class VeterinarianService : IVeterinarianService
{
    private readonly IVeterinarianRepository _veterinarianRepository;

    public VeterinarianService(IVeterinarianRepository veterinarianRepository)
    {
        _veterinarianRepository = veterinarianRepository;
    }

    public async Task<bool> CreateAsync(VeterinarianCreateDto veterinarianCreateDto)
    {
        var veterinarian = await _veterinarianRepository.GetByDocumentAsync(veterinarianCreateDto.Document, veterinarianCreateDto.Speciality);
        if (veterinarian != null)
        {
            return false;
            
        }
        else
        {
            var newVeterinarian = new Veterinarian{
                Name = veterinarianCreateDto.Name, 
                Document = veterinarianCreateDto.Document, 
                Schedule = veterinarianCreateDto.Schedule, 
                Speciality = veterinarianCreateDto.Speciality};
            await _veterinarianRepository.AddAsync(newVeterinarian);
            return true;
        }
    }

    public async Task<VeterinarianResponseDto?> GetByDocumentAsync(string document, Speciality speciality)
    {
        var veterinarian = await _veterinarianRepository.GetByDocumentAsync(document, speciality);
        if (veterinarian == null) return null;
        {
            return new VeterinarianResponseDto()
            {
                Id = veterinarian.Id,
                Name = veterinarian.Name,
                Document = veterinarian.Document,
                Speciality = speciality,
                IsActive = veterinarian.Schedule == ScheduleTime.Active
            };
        }
    }

    public async Task<IEnumerable<VeterinarianResponseDto>> GetAllVeterinarianAsync()
    {
        var veterinarian = await _veterinarianRepository.GetAll();
        return veterinarian.Select(v => new VeterinarianResponseDto()
        {
            Name = v.Name,
            Document = v.Document,
            IsActive = v.Schedule == ScheduleTime.Active,
            Speciality = v.Speciality
        });
    }

    public async Task<bool> IsAvalableAsync(int id, ScheduleTime schedule)
    {
        var v = await _veterinarianRepository.GetById(id);
        return v != null && v.Schedule == schedule;
    }
}