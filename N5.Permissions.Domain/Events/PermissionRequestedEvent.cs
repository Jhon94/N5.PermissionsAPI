namespace N5.Permissions.Domain.Events
{
    public class PermissionRequestedEvent : PermissionEvent
    {
        public PermissionRequestedEvent(int permissionId)
        {
            PermissionId = permissionId;
            Operation = "request";
        }
    }
}
