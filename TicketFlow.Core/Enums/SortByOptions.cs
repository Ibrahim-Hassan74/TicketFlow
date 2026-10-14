using System.Text.Json.Serialization;

namespace TicketFlow.Core.Enums
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
