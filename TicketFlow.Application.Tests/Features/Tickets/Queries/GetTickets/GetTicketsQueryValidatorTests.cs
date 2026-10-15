using FluentValidation.TestHelper;
using TicketFlow.Application.Features.Tickets.DTOs;
using TicketFlow.Application.Features.Tickets.Queries.GetTickets;
using TicketFlow.Domain.Enums;
using Xunit;

namespace TicketFlow.Application.Tests.Features.Tickets.Queries.GetTickets
{
    public class GetTicketsQueryValidatorTests
    {
        private readonly GetTicketsQueryValidator _validator = new();

        [Fact]
        public void Validate_WhenQueryHasDefaultValues_ShouldPass()
        {
            // Arrange
            var query = new GetTicketsQuery
            {
                Page = 1,
                PageSize = 10
            };

            // Act
            var result = _validator.TestValidate(query);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-10)]
        public void Validate_WhenPageIsLessThanOne_ShouldHaveValidationError(int invalidPage)
        {
            // Arrange
            var query = new GetTicketsQuery
            {
                Page = invalidPage,
                PageSize = 10
            };

            // Act
            var result = _validator.TestValidate(query);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Page)
                .WithErrorMessage("Page number must be at least 1.");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-5)]
        [InlineData(101)]
        [InlineData(500)]
        public void Validate_WhenPageSizeIsOutOfRange_ShouldHaveValidationError(int invalidPageSize)
        {
            // Arrange
            var query = new GetTicketsQuery
            {
                Page = 1,
                PageSize = invalidPageSize
            };

            // Act
            var result = _validator.TestValidate(query);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.PageSize)
                .WithErrorMessage("Page size must be between 1 and 100.");
        }

        [Fact]
        public void Validate_WhenStatusIsInvalidEnum_ShouldHaveValidationError()
        {
            // Arrange
            var query = new GetTicketsQuery
            {
                Status = (TicketStatus)999
            };

            // Act
            var result = _validator.TestValidate(query);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Status)
                .WithErrorMessage("Invalid ticket status.");
        }

        [Fact]
        public void Validate_WhenPriorityIsInvalidEnum_ShouldHaveValidationError()
        {
            // Arrange
            var query = new GetTicketsQuery
            {
                Priority = (TicketPriority)999
            };

            // Act
            var result = _validator.TestValidate(query);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Priority)
                .WithErrorMessage("Invalid ticket priority.");
        }

        [Fact]
        public void Validate_WhenSortByIsInvalidEnum_ShouldHaveValidationError()
        {
            // Arrange
            var query = new GetTicketsQuery
            {
                SortBy = (SortByOptions)999
            };

            // Act
            var result = _validator.TestValidate(query);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.SortBy)
                .WithErrorMessage("Invalid sort field.");
        }

        [Fact]
        public void Validate_WhenSortOrderIsInvalidEnum_ShouldHaveValidationError()
        {
            // Arrange
            var query = new GetTicketsQuery
            {
                SortOrder = (SortOrderOptions)999
            };

            // Act
            var result = _validator.TestValidate(query);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.SortOrder)
                .WithErrorMessage("Invalid sort order.");
        }

        [Fact]
        public void Validate_WhenAllOptionalFiltersAreValid_ShouldPass()
        {
            // Arrange
            var query = new GetTicketsQuery
            {
                Page = 2,
                PageSize = 25,
                Status = TicketStatus.InProgress,
                Priority = TicketPriority.High,
                SortBy = SortByOptions.Priority,
                SortOrder = SortOrderOptions.Descending,
                Search = "Database"
            };

            // Act
            var result = _validator.TestValidate(query);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
