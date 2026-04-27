
using MiProyectoOnion.Domain.Enums;

namespace MiProyectoOnion.Domain.Entities;

public class Pet : BaseEntity
{
    public string Name { get; set; }
    public PetRace Race { get; set; }
    public int Age { get; set; }
    public decimal Weight { get; set; }
    public int OwnerId { get; set; }
    public Owner Owner { get; set; }
    public ICollection<Appointment> Appointments { get; set; }
    public ICollection<Historical> Historicals { get; set; }
}