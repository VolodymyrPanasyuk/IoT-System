using IoT_System.Application.Interfaces.Repositories;
using IoT_System.Domain.Entities.Auth;
using IoT_System.Infrastructure.Contexts;

namespace IoT_System.Infrastructure.Repositories;

public class GroupRepository(AuthDbContext context) : RepositoryBase<Group, AuthDbContext>(context), IGroupRepository;