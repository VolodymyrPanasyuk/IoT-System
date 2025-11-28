using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using IoT_System.Application.Common;
using IoT_System.Domain.Entities.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace IoT_System.Infrastructure.Services;

public class JwtService
{
    private readonly IConfiguration _config;

    public JwtService(IConfiguration config)
    {
        _config = config;
    }

    public string GenerateAccessToken(User user, List<Role>? roles = null, List<Group>? groups = null)
    {
        roles ??= new List<Role>();
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(Constants.ClaimTypes.JwtUserId, user.Id.ToString()),
            new(Constants.ClaimTypes.JwtUserName, user.UserName!),
            new(Constants.ClaimTypes.FirstName, user.FirstName),
            new(Constants.ClaimTypes.LastName, user.LastName)
        };

        if (groups is { Count: > 0 })
        {
            groups.ForEach(group =>
            {
                claims.Add(new Claim(Constants.ClaimTypes.GroupId, group.Id.ToString()));
                claims.Add(new Claim(Constants.ClaimTypes.GroupName, group.Name));
                roles.AddRange(group.GroupRoles.Select(gr => gr.Role));
            });
        }

        if (roles is { Count: > 0 })
        {
            roles.Where(r => !string.IsNullOrWhiteSpace(r.Name))
                .DistinctBy(r => r.Id)
                .ToList()
                .ForEach(role =>
                {
                    claims.Add(new Claim(Constants.ClaimTypes.RoleId, role.Id.ToString()));
                    if (role.Name != null) claims.Add(new Claim(Constants.ClaimTypes.Role, role.Name!));
                });
        }

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(int.Parse(_config["Jwt:AccessTokenLifetimeMinutes"]!)),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}