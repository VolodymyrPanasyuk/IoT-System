using AutoMapper;
using IoT_System.Application.DTOs.Response.Auth;
using IoT_System.Application.DTOs.Response.Groups;
using IoT_System.Application.DTOs.Response.Roles;
using IoT_System.Application.DTOs.Response.Users;
using IoT_System.Domain.Entities.Auth;

namespace IoT_System.Application.Mappers;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // ====== USERS ======
        CreateMap<User, UserResponse>()
            .ForMember(dest => dest.Roles, opt => opt.MapFrom(src =>
                src.UserGroups
                    .SelectMany(ug => ug.Group.GroupRoles.Select(gr => gr.Role))
                    .Distinct()
                    .ToList()
            ))
            .ForMember(dest => dest.Groups, opt => opt.MapFrom(src =>
                src.UserGroups.Select(ug => ug.Group).ToList()
            ));

        CreateMap<User, UserShortResponse>();

        // ====== ROLES ======
        CreateMap<Role, RoleResponse>()
            .ForMember(dest => dest.Users, opt => opt.MapFrom(src =>
                src.GroupRoles
                    .SelectMany(gr => gr.Group.UserGroups.Select(ug => ug.User))
                    .Distinct()
                    .ToList()
            ))
            .ForMember(dest => dest.Groups, opt => opt.MapFrom(src =>
                src.GroupRoles.Select(gr => gr.Group).ToList()
            ));

        CreateMap<Role, RoleShortResponse>();

        // ====== GROUPS ======
        CreateMap<Group, GroupResponse>()
            .ForMember(dest => dest.Users, opt => opt.MapFrom(src =>
                src.UserGroups.Select(ug => ug.User).ToList()
            ))
            .ForMember(dest => dest.Roles, opt => opt.MapFrom(src =>
                src.GroupRoles.Select(gr => gr.Role).ToList()
            ));

        CreateMap<Group, GroupShortResponse>();

        // ====== AUTH SESSION ======
        CreateMap<User, UserSessionResponse>()
            .ForMember(dest => dest.Roles, opt => opt.MapFrom(src =>
                src.UserGroups
                    .SelectMany(ug => ug.Group.GroupRoles.Select(gr => gr.Role))
                    .Distinct()
                    .ToList()
            ))
            .ForMember(dest => dest.Groups, opt => opt.MapFrom(src =>
                src.UserGroups.Select(ug => ug.Group).ToList()
            ));
    }
}