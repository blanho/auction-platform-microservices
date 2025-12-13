using Common.CQRS.Abstractions;

namespace AuctionService.Application.Commands.DeleteCategory;

public record DeleteCategoryCommand(Guid Id) : ICommand<bool>;
