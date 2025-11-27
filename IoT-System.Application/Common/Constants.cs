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
}