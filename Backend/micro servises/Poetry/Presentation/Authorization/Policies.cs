namespace Presentation.Authorization
{
    public static class Policies
    {
        public const string RequireAdminRole = "RequireAdminRole";
        public const string RequireModeratorRole = "RequireModeratorRole";
        public const string CanManageContent = "CanManageContent";
    }
}
