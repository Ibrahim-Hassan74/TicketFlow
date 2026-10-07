namespace TicketFlow.Domain.Exceptions
{
    /// <summary>
    /// Exception thrown when a core business rule is violated within the Domain Entities.
    /// </summary>
    public class DomainValidationException : Exception
    {
        public DomainValidationException()
            : base("A domain validation rule was violated.")
        {
        }
        public DomainValidationException(string message)
            : base(message)
        {
        }
        public DomainValidationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
