namespace MiProyectoOnion.Domain.Entities;

public class Owner : BaseEntity
{
    public string Name { get; set; } 
    public string Document { get; set; } 
    public string Email { get; set; } 
    public string Phone { get; set; } 
    public ICollection<Pet> Pets { get; set; }
    public ICollection<Appointment> Appointments { get; set; }
}