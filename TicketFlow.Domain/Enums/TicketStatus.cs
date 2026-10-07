using System.Text.Json.Serialization;

namespace TicketFlow.Domain.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum TicketStatus
    {
        Open = 0,
        InProgress = 1,
        Resolved = 2,
        Closed = 3
    }
}
