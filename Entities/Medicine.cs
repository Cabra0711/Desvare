namespace MiProyectoOnion.Domain.Entities;

public class Medicine : BaseEntity
{
    public string Name { get; set; }
    public string Dosis { get; set; }
    public string Frecuency { get; set; }
}