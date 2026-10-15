using FluentAssertions;
using MediatR;
using Moq;
using TicketFlow.Application.Common.Exceptions;
using TicketFlow.Application.Features.Tickets.Commands.UpdateTicket;
using TicketFlow.Domain.Contracts;
using TicketFlow.Domain.Entities;
using TicketFlow.Domain.Enums;
using Xunit;

namespace TicketFlow.Application.Tests.Features.Tickets.Commands.UpdateTicket
{
    public class UpdateTicketCommandHandlerTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<ITicketsRepository> _ticketsRepositoryMock;
        private readonly UpdateTicketCommandHandler _handler;

        public UpdateTicketCommandHandlerTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _ticketsRepositoryMock = new Mock<ITicketsRepository>();
            _unitOfWorkMock.Setup(u => u.TicketsRepository).Returns(_ticketsRepositoryMock.Object);

            _handler = new UpdateTicketCommandHandler(_unitOfWorkMock.Object);
        }

        [Fact]
        public async Task Handle_WhenTicketNotFound_ShouldThrowNotFoundException()
        {
            // Arrange
            var ticketId = Guid.NewGuid();
            var command = new UpdateTicketCommand
            {
                Id = ticketId,
                UserId = Guid.NewGuid(),
                Title = "New Title",
                Description = "New Description"
            };

            _ticketsRepositoryMock
                .Setup(r => r.GetByIdAsync(ticketId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Ticket?)null);

            // Act
            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("Ticket not found.");

            _unitOfWorkMock.Verify(u => u.CompleteAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WhenTicketExists_ShouldUpdateTicketAndCommit()
        {
            // Arrange
            var ticketId = Guid.NewGuid();
            var originalUpdatedAt = DateTimeOffset.UtcNow.AddDays(-1);
            var ticket = new Ticket
            {
                Id = ticketId,
                Title = "Old Title",
                Description = "Old Description",
                Priority = TicketPriority.Low,
                Status = TicketStatus.Open,
                CustomerId = Guid.NewGuid(),
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-2),
                UpdatedAt = originalUpdatedAt
            };

            var command = new UpdateTicketCommand
            {
                Id = ticketId,
                UserId = Guid.NewGuid(),
                Title = "Brand New Title",
                Description = "Brand New Description"
            };

            _ticketsRepositoryMock
                .Setup(r => r.GetByIdAsync(ticketId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ticket);

            _unitOfWorkMock
                .Setup(u => u.CompleteAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().Be(Unit.Value);
            ticket.Title.Should().Be("Brand New Title");
            ticket.Description.Should().Be("Brand New Description");
            ticket.UpdatedAt.Should().BeAfter(originalUpdatedAt);

            _unitOfWorkMock.Verify(u => u.CompleteAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
