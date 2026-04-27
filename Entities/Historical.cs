namespace MiProyectoOnion.Domain.Entities;

public class Historical : BaseEntity
{
    public int PetId { get; set; }
    public Pet Pet { get; set; }
    public int AppointmentId { get; set; }
    public Appointment Appointment { get; set; }
    public int MedicineId { get; set; }
    public Medicine Medicine { get; set; }
}