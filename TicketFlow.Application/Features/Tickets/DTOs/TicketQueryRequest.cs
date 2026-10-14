using FluentValidation;
using TicketFlow.Domain.Enums;

namespace TicketFlow.Application.Features.Tickets.DTOs
{
    public class TicketQueryRequest
    {
        public TicketStatus? Status { get; set; }
        public TicketPriority? Priority { get; set; }
        public string? Search { get; set; }
        public SortByOptions? SortBy { get; set; }
        public SortOrderOptions? SortOrder { get; set; }

        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 10;
    }

    public sealed class TicketQueryRequestValidator : AbstractValidator<TicketQueryRequest>
    {
        public TicketQueryRequestValidator()
        {
            RuleFor(x => x.Page)
                .GreaterThanOrEqualTo(1)
                .WithMessage("Page number must be at least 1.");

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 100)
                .WithMessage("Page size must be between 1 and 100.");
        }
    }
}
