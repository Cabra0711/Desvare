using Microsoft.EntityFrameworkCore;
using MiProyectoOnion.Domain.Entities;
using MiProyectoOnion.Domain.Interfaces;
using MiProyectoOnion.Infrastructure.Persistence;

namespace MiProyectoOnion.Infrastructure.Repositories;

// ═══════════════════════════════════════════════════════════════
// REPOSITORIO GENÉRICO BASE
// ═══════════════════════════════════════════════════════════════

/// <summary>
/// Implementación genérica del repositorio.
/// Proporciona las operaciones CRUD básicas para cualquier entidad.
/// Todos los repositorios específicos heredan de esta clase.
/// </summary>
public class Repository<T> : IRepository<T> where T : class
{
    protected readonly VeterinaryDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(VeterinaryDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    /// <summary>Obtiene todos los registros de la entidad.</summary>
    public async Task<IEnumerable<T>> GetAllAsync()
        => await _dbSet.ToListAsync();

    /// <summary>Busca un registro por su Id (clave primaria).</summary>
    public async Task<T?> GetByIdAsync(int id)
        => await _dbSet.FindAsync(id);

    /// <summary>Agrega un nuevo registro al contexto (sin guardar aún).</summary>
    public async Task AddAsync(T entity)
        => await _dbSet.AddAsync(entity);

    /// <summary>Marca el registro para actualización.</summary>
    public void Update(T entity)
        => _dbSet.Update(entity);

    /// <summary>Marca el registro para eliminación.</summary>
    public void Delete(T entity)
        => _dbSet.Remove(entity);

    /// <summary>Persiste todos los cambios pendientes en la base de datos.</summary>
    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();
}

// ═══════════════════════════════════════════════════════════════
// REPOSITORIO DE PROPIETARIOS
// ═══════════════════════════════════════════════════════════════

/// <summary>
/// Repositorio de propietarios con consultas específicas de negocio.
/// </summary>
public class OwnerRepository : Repository<Owner>, IOwnerRepository
{
    public OwnerRepository(VeterinaryDbContext context) : base(context) { }

    /// <summary>Busca un propietario por documento de identidad.</summary>
    public async Task<Owner?> GetByDocumentAsync(string document)
        => await _context.Owners
            .FirstOrDefaultAsync(o => o.Document == document);

    /// <summary>Busca un propietario por email.</summary>
    public async Task<Owner?> GetByEmailAsync(string email)
        => await _context.Owners
            .FirstOrDefaultAsync(o => o.Email == email.ToLower());

    /// <summary>Obtiene un propietario con su lista de mascotas cargadas.</summary>
    public async Task<Owner?> GetWithPetsAsync(int ownerId)
        => await _context.Owners
            .Include(o => o.Pets)
            .FirstOrDefaultAsync(o => o.Id == ownerId);
}

// ═══════════════════════════════════════════════════════════════
// REPOSITORIO DE MASCOTAS
// ═══════════════════════════════════════════════════════════════

/// <summary>
/// Repositorio de mascotas.
/// Incluye consultas para historial clínico completo (citas + tratamientos + medicamentos).
/// </summary>
public class PetRepository : Repository<Pet>, IPetRepository
{
    public PetRepository(VeterinaryDbContext context) : base(context) { }

    /// <summary>Obtiene todas las mascotas de un propietario.</summary>
    public async Task<IEnumerable<Pet>> GetByOwnerAsync(int ownerId)
        => await _context.Pets
            .Include(p => p.Owner)
            .Where(p => p.OwnerId == ownerId)
            .ToListAsync();

    /// <summary>
    /// Carga la mascota con todo su historial clínico:
    /// citas → tratamientos → medicamentos → veterinario.
    /// </summary>
    public async Task<Pet?> GetWithHistoryAsync(int petId)
        => await _context.Pets
            .Include(p => p.Owner)
            .Include(p => p.Appointments)
                .ThenInclude(a => a.Veterinarian)
            .Include(p => p.Appointments)
                .ThenInclude(a => a.Treatment)
                    .ThenInclude(t => t!.Medicine)
            .FirstOrDefaultAsync(p => p.Id == petId);
}

// ═══════════════════════════════════════════════════════════════
// REPOSITORIO DE VETERINARIOS
// ═══════════════════════════════════════════════════════════════

/// <summary>
/// Repositorio de veterinarios con búsqueda por nombre y especialidad.
/// </summary>
public class VeterinarianRepository : Repository<Veterinarian>, IVeterinarianRepository
{
    public VeterinarianRepository(VeterinaryDbContext context) : base(context) { }

    /// <summary>
    /// Busca un veterinario por nombre y especialidad.
    /// Se usa para evitar duplicados en el registro.
    /// </summary>
    public async Task<Veterinarian?> GetByNameAndSpecialityAsync(string name, string speciality)
        => await _context.Veterinarians
            .FirstOrDefaultAsync(v =>
                v.Name.ToLower() == name.ToLower() &&
                v.Speciality.ToString() == speciality);
}

// ═══════════════════════════════════════════════════════════════
// REPOSITORIO DE CITAS
// ═══════════════════════════════════════════════════════════════

/// <summary>
/// Repositorio de citas médicas.
/// Contiene las consultas críticas para validar solapamientos e inasistencias.
/// </summary>
public class AppointmentRepository : Repository<Appointment>, IAppointmentRepository
{
    public AppointmentRepository(VeterinaryDbContext context) : base(context) { }

    /// <summary>
    /// Carga todas las citas de un veterinario en una fecha específica.
    /// Se usa para detectar solapamiento de horarios del veterinario.
    /// </summary>
    public async Task<IEnumerable<Appointment>> GetByVeterinarianAndDateAsync(int veterinarianId, DateTime date)
        => await _context.Appointments
            .Where(a => a.VeterinarianId == veterinarianId &&
                        a.AppointmentDate.Date == date.Date)
            .ToListAsync();

    /// <summary>
    /// Carga todas las citas de una mascota en una fecha específica.
    /// Se usa para detectar solapamiento de horarios de la mascota.
    /// </summary>
    public async Task<IEnumerable<Appointment>> GetByPetAndDateAsync(int petId, DateTime date)
        => await _context.Appointments
            .Where(a => a.PetId == petId &&
                        a.AppointmentDate.Date == date.Date)
            .ToListAsync();

    /// <summary>
    /// Cuenta las citas activas (estado Programada) de una mascota.
    /// Regla: máximo 2 citas activas por mascota.
    /// </summary>
    public async Task<int> CountActiveAppointmentsByPetAsync(int petId)
        => await _context.Appointments
            .CountAsync(a => a.PetId == petId &&
                             a.Status == Domain.Enums.AppointmentStatus.Programada);

    /// <summary>
    /// Cuenta las inasistencias acumuladas de una mascota.
    /// Al llegar a 3, se bloquea al propietario.
    /// </summary>
    public async Task<int> CountNoShowByPetAsync(int petId)
        => await _context.Appointments
            .CountAsync(a => a.PetId == petId &&
                             a.Status == Domain.Enums.AppointmentStatus.NoAsistio);

    /// <summary>
    /// Carga una cita con todos sus datos relacionados:
    /// mascota → propietario, veterinario, tratamiento → medicamento.
    /// </summary>
    public async Task<Appointment?> GetWithDetailsAsync(int appointmentId)
        => await _context.Appointments
            .Include(a => a.Pet)
                .ThenInclude(p => p.Owner)
            .Include(a => a.Veterinarian)
            .Include(a => a.Treatment)
                .ThenInclude(t => t!.Medicine)
            .FirstOrDefaultAsync(a => a.Id == appointmentId);

    /// <summary>Sobreescribe GetAllAsync para incluir navegaciones.</summary>
    public new async Task<IEnumerable<Appointment>> GetAllAsync()
        => await _context.Appointments
            .Include(a => a.Pet)
                .ThenInclude(p => p.Owner)
            .Include(a => a.Veterinarian)
            .Include(a => a.Treatment)
                .ThenInclude(t => t!.Medicine)
            .ToListAsync();
}

// ═══════════════════════════════════════════════════════════════
// REPOSITORIO DE TRATAMIENTOS
// ═══════════════════════════════════════════════════════════════

/// <summary>
/// Repositorio de tratamientos médicos.
/// </summary>
public class TreatmentRepository : Repository<Treatment>, ITreatmentRepository
{
    public TreatmentRepository(VeterinaryDbContext context) : base(context) { }

    /// <summary>Obtiene el tratamiento asociado a una cita específica.</summary>
    public async Task<Treatment?> GetByAppointmentAsync(int appointmentId)
        => await _context.Treatments
            .Include(t => t.Medicine)
            .Include(t => t.Appointment)
                .ThenInclude(a => a.Pet)
                    .ThenInclude(p => p.Owner)
            .FirstOrDefaultAsync(t => t.AppointmentId == appointmentId);

    /// <summary>Sobreescribe GetAllAsync para incluir navegaciones.</summary>
    public new async Task<IEnumerable<Treatment>> GetAllAsync()
        => await _context.Treatments
            .Include(t => t.Medicine)
            .Include(t => t.Appointment)
            .ToListAsync();
}

// ═══════════════════════════════════════════════════════════════
// REPOSITORIO DE MEDICAMENTOS
// ═══════════════════════════════════════════════════════════════

/// <summary>
/// Repositorio de medicamentos.
/// </summary>
public class MedicineRepository : Repository<Medicine>, IMedicineRepository
{
    public MedicineRepository(VeterinaryDbContext context) : base(context) { }

    /// <summary>Retorna únicamente los medicamentos con stock disponible.</summary>
    public async Task<IEnumerable<Medicine>> GetAvailableAsync()
        => await _context.Medicines
            .Where(m => m.IsAvailable && m.Stock > 0)
            .ToListAsync();
}
