using System.Text.Json.Serialization;

namespace TicketFlow.Domain.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum UserRole
    {
        Admin,
        SupportAgent,
        Customer
    }
}
