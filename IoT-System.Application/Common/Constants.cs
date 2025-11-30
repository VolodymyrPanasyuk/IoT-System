using System.IdentityModel.Tokens.Jwt;
using SystemClaimTypes = System.Security.Claims.ClaimTypes;

namespace IoT_System.Application.Common;

public static class Constants
{
    public static class Roles
    {
        public const string SuperAdmin = "SuperAdmin";
        public const string Admin = "Admin";
        public const string Scientist = "Scientist";
        public const string Viewer = "Viewer";

        public static readonly List<string> SystemDefault = [SuperAdmin, Admin, Scientist, Viewer];
    }

    public static class DefaultSuperAdmin
    {
        public const string UserName = "superadmin";
        public const string Email = "superadmin@iotsystem.local";
        public const string FirstName = "Super";
        public const string LastName = "Administrator";
    }

    public static class ClaimTypes
    {
        public const string JwtUserId = JwtRegisteredClaimNames.Sub;
        public const string JwtUserName = JwtRegisteredClaimNames.UniqueName;

        public const string UserId = SystemClaimTypes.NameIdentifier;
        public const string UserName = SystemClaimTypes.Name;
        public const string FirstName = "first_name";
        public const string LastName = "last_name";

        public const string RoleId = "role_id";
        public const string Role = SystemClaimTypes.Role;

        public const string GroupId = "group_id";
        public const string GroupName = "group_name";
    }

    public static class ApiHeaders
    {
        public const string ApiKey = "X-API-Key";
    }

    public static class SignalR
    {
        public static class HubMethods
        {
            public const string MeasurementAdded = "MeasurementAdded";
            public const string MeasurementUpdated = "MeasurementUpdated";
            public const string MeasurementDeleted = "MeasurementDeleted";
            public const string ThresholdExceeded = "ThresholdExceeded";
            public const string DeviceStatusChanged = "DeviceStatusChanged";
        }

        public static class Groups
        {
            public static string DeviceGroup(Guid deviceId) => $"Device_{deviceId}";
            public static string AllDevices => "AllDevices";
        }
    }

    public static class Thresholds
    {
        public const string Normal = "Normal";
        public const string Warning = "Warning";
        public const string Critical = "Critical";
    }
}