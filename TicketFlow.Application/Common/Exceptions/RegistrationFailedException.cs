namespace TicketFlow.Application.Common.Exceptions
{
    public class RegistrationFailedException(IEnumerable<string> errors) : Exception("Registration failed.")
    {
        public IEnumerable<string> Errors { get; } = errors;
    }
}
