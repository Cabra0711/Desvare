namespace MiProyectoOnion.Application.DTOs;

public record OwnerResponseDto()
{
    public int Id { get; init; }
    public string Name { get; init; }
    public string Email  { get; init; }
    public string Phone { get; init; }
    public int TotalPets { get; init; }
}