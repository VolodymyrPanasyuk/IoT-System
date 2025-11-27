namespace IoT_System.Application.DTOs.Request;

public record BaseUserDto(string UserName, string Password, string FirstName, string LastName);

public record BaseNamedEntityDto(string Name);