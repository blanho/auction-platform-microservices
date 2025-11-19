namespace Analytics.Api.Models;

public record RevenueSummary(
    decimal TotalRevenue,
    int TotalOrders,
    int PaidOrders,
    int ShippedOrders,
    int DeliveredOrders,
    decimal AverageOrderValue
);

public record RevenueByDate(
    DateOnly Date,
    decimal Revenue,
    int OrderCount,
    int PaidCount
);

public record PaymentsByStatus(
    string Status,
    int Count,
    decimal TotalAmount
);
