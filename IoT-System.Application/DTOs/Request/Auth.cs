namespace IoT_System.Application.DTOs.Request;

public record RegisterRequest(string UserName, string Password, string FirstName, string LastName)
    : BaseUserDto(UserName, Password, FirstName, LastName);

public record LoginRequest(string UserName, string Password);