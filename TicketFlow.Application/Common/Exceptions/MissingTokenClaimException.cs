namespace TicketFlow.Application.Common.Exceptions
{
    public class MissingTokenClaimException(string message) : Exception(message);
}
