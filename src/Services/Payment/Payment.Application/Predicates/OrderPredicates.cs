using System.Linq.Expressions;
using Payment.Domain.Entities;

namespace Payment.Application.Predicates;

public static class OrderPredicates
{
    public static Expression<Func<Order, bool>> IsCompleted =>
        o => o.PaymentStatus == PaymentStatus.Completed;

    public static Expression<Func<Order, bool>> IsPending =>
        o => o.PaymentStatus == PaymentStatus.Pending;

    public static Expression<Func<Order, bool>> IsRefunded =>
        o => o.PaymentStatus == PaymentStatus.Refunded;

    public static Expression<Func<Order, bool>> CompletedAfter(DateTimeOffset startDate) =>
        o => o.PaymentStatus == PaymentStatus.Completed && o.PaidAt >= startDate;

    public static Expression<Func<Order, bool>> CompletedBetween(DateTimeOffset start, DateTimeOffset end) =>
        o => o.PaymentStatus == PaymentStatus.Completed && o.PaidAt >= start && o.PaidAt < end;

    public static Expression<Func<Order, bool>> ByBuyer(string username) =>
        o => o.BuyerUsername == username;

    public static Expression<Func<Order, bool>> BySeller(string username) =>
        o => o.SellerUsername == username;

    public static Expression<Func<Order, bool>> HasPaymentStatus(PaymentStatus status) =>
        o => o.PaymentStatus == status;
}
