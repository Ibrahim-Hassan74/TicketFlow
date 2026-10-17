using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using TicketFlow.Application.Common.Exceptions;
using TicketFlow.Application.Features.Tickets.Commands.CreateTicket;
using TicketFlow.Domain.Contracts;
using TicketFlow.Domain.Entities;
using TicketFlow.Domain.Enums;
using TicketFlow.Domain.IdentityEntities;
using Xunit;

namespace TicketFlow.Application.Tests.Features.Tickets.Commands.CreateTicket
{
    public class CreateTicketCommandHandlerTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<ITicketsRepository> _ticketsRepositoryMock;
        private readonly Mock<ITicketActivityRepository> _ticketActivityRepositoryMock;
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly Mock<ILogger<CreateTicketCommandHandler>> _loggerMock;
        private readonly CreateTicketCommandHandler _handler;

        public CreateTicketCommandHandlerTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _ticketsRepositoryMock = new Mock<ITicketsRepository>();
            _ticketActivityRepositoryMock = new Mock<ITicketActivityRepository>();

            _unitOfWorkMock.Setup(u => u.TicketsRepository).Returns(_ticketsRepositoryMock.Object);
            _unitOfWorkMock.Setup(u => u.TicketActivityRepository).Returns(_ticketActivityRepositoryMock.Object);

            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);

            _loggerMock = new Mock<ILogger<CreateTicketCommandHandler>>();

            _handler = new CreateTicketCommandHandler(_unitOfWorkMock.Object, _userManagerMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_WhenCustomerNotFound_ShouldThrowNotFoundException()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var command = new CreateTicketCommand
            {
                CustomerId = customerId,
                Title = "Cannot login",
                Description = "Password reset not working",
                Priority = TicketPriority.High
            };

            _userManagerMock
                .Setup(m => m.FindByIdAsync(customerId.ToString()))
                .ReturnsAsync((ApplicationUser?)null);

            // Act
            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("Customer not found.");

            _ticketsRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Ticket>(), It.IsAny<CancellationToken>()), Times.Never);
            _ticketActivityRepositoryMock.Verify(r => r.AddAsync(It.IsAny<TicketActivity>(), It.IsAny<CancellationToken>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.CompleteAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WhenCustomerExists_ShouldCreateTicketAndActivityAndReturnTicketResponse()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var customer = new ApplicationUser
            {
                Id = customerId,
                UserName = "janedoe",
                DisplayName = "Jane Doe",
                Email = "jane@example.com"
            };

            var command = new CreateTicketCommand
            {
                CustomerId = customerId,
                Title = "VPN connection drops",
                Description = "VPN disconnects every 10 minutes",
                Priority = TicketPriority.High
            };

            _userManagerMock
                .Setup(m => m.FindByIdAsync(customerId.ToString()))
                .ReturnsAsync(customer);

            var generatedTicketId = Guid.NewGuid();
            Ticket? capturedTicket = null;
            _ticketsRepositoryMock
                .Setup(r => r.AddAsync(It.IsAny<Ticket>(), It.IsAny<CancellationToken>()))
                .Callback<Ticket, CancellationToken>((t, _) =>
                {
                    capturedTicket = t;
                    t.Id = generatedTicketId;
                })
                .ReturnsAsync((Ticket t, CancellationToken _) => t);

            TicketActivity? capturedActivity = null;
            _ticketActivityRepositoryMock
                .Setup(r => r.AddAsync(It.IsAny<TicketActivity>(), It.IsAny<CancellationToken>()))
                .Callback<TicketActivity, CancellationToken>((a, _) => capturedActivity = a)
                .ReturnsAsync((TicketActivity a, CancellationToken _) => a);

            _unitOfWorkMock
                .Setup(u => u.CompleteAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(generatedTicketId);
            result.Title.Should().Be("VPN connection drops");
            result.Description.Should().Be("VPN disconnects every 10 minutes");
            result.Priority.Should().Be(TicketPriority.High.ToString());
            result.Status.Should().Be(TicketStatus.Open.ToString());
            result.CustomerId.Should().Be(customerId);
            result.CustomerName.Should().Be("Jane Doe");

            capturedTicket.Should().NotBeNull();
            capturedTicket!.Title.Should().Be("VPN connection drops");
            capturedTicket.Status.Should().Be(TicketStatus.Open);
            capturedTicket.CustomerId.Should().Be(customerId);

            capturedActivity.Should().NotBeNull();
            capturedActivity!.TicketId.Should().Be(generatedTicketId);
            capturedActivity.UserId.Should().Be(customerId);
            capturedActivity.Type.Should().Be(ActivityType.Created);
            capturedActivity.Description.Should().Be("Ticket created.");

            _unitOfWorkMock.Verify(u => u.CompleteAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
