namespace TicketFlow.Domain.Contracts
{
    public interface IUnitOfWork
    {
        ITicketActivityRepository TicketActivityRepository { get; }
        ITicketCommentRepository TicketCommentRepository { get; }
        ITicketsRepository TicketsRepository { get; }
        ITimeEntryRepository TimeEntryRepository { get; }
        Task<int> CompleteAsync();
    }
}
