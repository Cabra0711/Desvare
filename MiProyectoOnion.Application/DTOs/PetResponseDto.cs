namespace MiProyectoOnion.Application.DTOs;

public record PetResponseDto()
{
    public int Id { get; init; }
    public string Name { get; init; }
    public int Age { get; init; }
    public decimal Weight { get; init; }
    public string OwnerName { get; init; }
}