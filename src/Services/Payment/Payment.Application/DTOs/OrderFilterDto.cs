using Payment.Domain.Enums;

namespace Payment.Application.DTOs;

public record GetAllOrdersFilter(
    int Page = 1,
    int PageSize = 10,
    string? Search = null,
    OrderStatus? Status = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null);
