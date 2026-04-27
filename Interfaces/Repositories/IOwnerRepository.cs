using MiProyectoOnion.Domain.Entities;

namespace MiProyectoOnion.Domain.Interfaces.Repositories;

// We do a contract which it is going to extend all the stuff we assing into the GenericRepository
// at here we are just going to define the signing of the system Owner can not Get a Document Duplied
//and the Email has to be Unique
public interface IOwnerRepository : IGenericRepository<Owner>
{
    // with this we ensure that the system just sign the contract in a future
    Task<Owner> GetByDocumentAsync(string document);
    Task<Owner> FindByEmailAsync(string email);
}