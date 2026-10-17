using System.Text.Json.Serialization;

namespace TicketFlow.Domain.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum SortOrderOptions
    {
        Ascending,
        Descending
    }
}
