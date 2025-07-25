namespace N5.Permissions.Domain.Events
{
    public class PermissionModifiedEvent : PermissionEvent
    {
        public PermissionModifiedEvent(int permissionId)
        {
            PermissionId = permissionId;
            Operation = "modify";
        }
    }
}
