using IoT_System.Application.Models;
using IoT_System.Domain.Entities.Auth;

namespace IoT_System.Application.Interfaces.Repositories;

public interface IUserRepository : IRepositoryBase<User>
{
    Task<OperationResult<User?>> GetByUsernameAsync(string username);
}