using MiProyectoOnion.Domain.Enums;

namespace MiProyectoOnion.Domain.Entities;

public class Appointment : BaseEntity
{
    public Owner Owner { get; set; }
    public int OwnerId { get; set; }
    public Pet Pets { get; set; }
    public int PetId { get; set; }
    public Veterinarian Veterinarian { get; set; }
    public int VeterinarianId { get; set; }
    public DateTime AppointmentDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public AppoimentStatus Status { get; set; }

    
    public Treatment? Treatment { get; set; }
}