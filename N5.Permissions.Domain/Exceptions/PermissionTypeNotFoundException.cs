namespace N5.Permissions.Domain.Exceptions
{
    public class PermissionTypeNotFoundException : Exception
    {
        public PermissionTypeNotFoundException(int permissionTypeId)
            : base($"Permission type with ID {permissionTypeId} was not found.")
        {
        }
    }
}
