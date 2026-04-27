namespace MiProyectoOnion.Domain.Entities;
 
public class Treatment : BaseEntity
{
    public int AppointmentId { get; set; }
    public Appointment Appointment { get; set; }
    public int MedicineId { get; set; }
    public Medicine Medicine { get; set; }
    public string Diagnosis { get; set; }
    public string Observations { get; set; }
}