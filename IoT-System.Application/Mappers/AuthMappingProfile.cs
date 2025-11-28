using AutoMapper;
using IoT_System.Application.Common;
using IoT_System.Application.DTOs.Request;
using IoT_System.Application.DTOs.Response.Groups;
using IoT_System.Application.DTOs.Response.Roles;
using IoT_System.Application.DTOs.Response.Users;
using IoT_System.Domain.Entities.Auth;

namespace IoT_System.Application.Mappers;

public class AuthMappingProfile : Profile
{
    public AuthMappingProfile()
    {
        CreateUserMappings();
        CreateRoleMappings();
        CreateGroupMappings();
    }

    private void CreateUserMappings()
    {
        CreateMap<User, UserResponse>()
            .ForMember(dest => dest.Groups, opt => opt.MapFrom(src =>
                src.UserGroups.Select(ug => ug.Group)))
            .ForMember(dest => dest.Roles, opt => opt.MapFrom(src =>
                src.UserRoles
                    .Select(ur => new RoleWithInheritance
                    {
                        Role = ur.Role,
                        IsInherited = false
                    })
                    .Concat(src.UserGroups
                        .SelectMany(ug => ug.Group.GroupRoles
                            .Select(gr => new RoleWithInheritance
                            {
                                Role = gr.Role,
                                IsInherited = true
                            })))
                    .GroupBy(x => x.Role.Id)
                    .Select(g => g.First())));

        CreateMap<User, UserShortResponse>();
        CreateMap<CreateUserRequest, User>();
        CreateMap<UpdateUserRequest, User>();
        CreateMap<UserResponse, UserShortResponse>().ReverseMap();
    }

    private void CreateRoleMappings()
    {
        CreateMap<Role, RoleResponse>()
            .ForMember(dest => dest.IsSystemDefault, opt => opt.MapFrom(src =>
                Constants.Roles.SystemDefault.Contains(src.Name)))
            .ForMember(dest => dest.Users, opt => opt.MapFrom(src =>
                src.UserRoles.Select(ur => ur.User)
                    .Concat(src.GroupRoles
                        .SelectMany(gr => gr.Group.UserGroups
                            .Select(ug => ug.User)))
                    .DistinctBy(u => u.Id)))
            .ForMember(dest => dest.Groups, opt => opt.MapFrom(src =>
                src.GroupRoles.Select(gr => gr.Group)));

        CreateMap<Role, RoleShortResponse>()
            .ForMember(dest => dest.IsSystemDefault, opt => opt.MapFrom(src =>
                Constants.Roles.SystemDefault.Contains(src.Name)))
            .ForMember(dest => dest.IsInherited, opt => opt.MapFrom(_ => false));

        CreateMap<RoleWithInheritance, RoleShortResponse>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Role.Id))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Role.Name))
            .ForMember(dest => dest.IsSystemDefault, opt => opt.MapFrom(src =>
                Constants.Roles.SystemDefault.Contains(src.Role.Name)))
            .ForMember(dest => dest.IsInherited, opt => opt.MapFrom(src => src.IsInherited));

        CreateMap<CreateRoleRequest, Role>();
        CreateMap<UpdateRoleRequest, Role>();
        CreateMap<RoleResponse, RoleShortResponse>().ReverseMap();
    }

    private void CreateGroupMappings()
    {
        CreateMap<Group, GroupResponse>()
            .ForMember(dest => dest.Users, opt => opt.MapFrom(src =>
                src.UserGroups.Select(ug => ug.User)))
            .ForMember(dest => dest.Roles, opt => opt.MapFrom(src =>
                src.GroupRoles.Select(gr => gr.Role)));

        CreateMap<Group, GroupShortResponse>();
        CreateMap<CreateGroupRequest, Group>();
        CreateMap<UpdateGroupRequest, Group>();
        CreateMap<GroupResponse, GroupShortResponse>().ReverseMap();
    }
}

/// <summary>
/// Helper class for mapping roles with inheritance information.
/// Used internally by AutoMapper to track whether a role is directly assigned or inherited from a group.
/// </summary>
public class RoleWithInheritance
{
    public Role Role { get; set; } = null!;
    public bool IsInherited { get; set; }
}