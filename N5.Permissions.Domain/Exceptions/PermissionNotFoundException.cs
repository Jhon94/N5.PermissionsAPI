namespace N5.Permissions.Domain.Exceptions
{
    public class PermissionNotFoundException : Exception
    {
        public PermissionNotFoundException(int permissionId)
            : base($"Permission with ID {permissionId} was not found.")
        {
        }
    }
}
