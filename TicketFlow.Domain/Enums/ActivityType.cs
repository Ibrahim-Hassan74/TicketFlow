using System.Text.Json.Serialization;

namespace TicketFlow.Domain.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ActivityType
    {
        Created = 0,
        StatusChanged = 1,
        PriorityChanged = 2,
        AgentAssigned = 3,
        CommentAdded = 4,
        Closed = 5,
        TimeLogged = 6
    }
}
