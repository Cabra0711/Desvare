namespace MiProyectoOnion.Application.DTOs;

public record OwnerCreateDto()
{
    public string Name { get; init; }
    public string Document { get; init; }
    public string Email { get; init; }
    public string Phone { get; init; }
}