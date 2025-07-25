namespace N5.Permissions.Domain.Events
{
    public class PermissionRetrievedEvent : PermissionEvent
    {
        public PermissionRetrievedEvent()
        {
            Operation = "get";
        }
    }
}
