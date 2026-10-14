using System.Text.Json.Serialization;

namespace TicketFlow.Core.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum UserRole
    {
        Admin,
        SupportAgent,
        Customer
    }
}
