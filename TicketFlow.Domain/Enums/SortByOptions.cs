using System.Text.Json.Serialization;

namespace TicketFlow.Domain.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum SortByOptions
    {
        CreatedAt,
        Priority,
        Status,
        UpdatedAt
    }
}
