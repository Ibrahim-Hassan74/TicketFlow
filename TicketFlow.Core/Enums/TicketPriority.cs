using System.Text.Json.Serialization;

namespace SupportTicketManagement.Core.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum TicketPriority
    {
        Low = 0,
        Medium = 1,
        High = 2,
        Critical = 3
    }
}
