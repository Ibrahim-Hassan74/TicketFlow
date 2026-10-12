namespace TicketFlow.Application.Common.Exceptions
{
    public class RegistrationFailedException(string message, IEnumerable<string> errors) : Exception(message)
    {
        public IEnumerable<string> Errors { get; } = errors;
    }
}
