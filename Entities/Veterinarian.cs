using MiProyectoOnion.Domain.Enums;

namespace MiProyectoOnion.Domain.Entities;

public class Veterinarian : BaseEntity
{
    public string Name { get; set; }
    public string Document { get; set; }
    public Speciality Speciality { get; set; }
    public ScheduleTime Schedule { get; set; }
    public ICollection<Appointment> Appointments { get; set; }
}