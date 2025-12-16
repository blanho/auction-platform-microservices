using AuctionService.Domain.Entities;
using Common.Domain.Enums;
using Common.Repository.Specifications;
using System.Linq.Expressions;

namespace AuctionService.Application.Specifications;

public class AuctionFilterSpecification : BaseSpecification<Auction>
{
    public AuctionFilterSpecification(
        string? status = null,
        string? seller = null,
        string? winner = null,
        string? searchTerm = null,
        string? category = null,
        bool? isFeatured = null)
    {
        var predicates = new List<Expression<Func<Auction, bool>>>();

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<Status>(status, true, out var statusEnum))
        {
            predicates.Add(a => a.Status == statusEnum);
        }

        if (!string.IsNullOrWhiteSpace(seller))
        {
            predicates.Add(a => a.SellerUsername.Contains(seller));
        }

        if (!string.IsNullOrWhiteSpace(winner))
        {
            predicates.Add(a => a.WinnerUsername != null && a.WinnerUsername.Contains(winner));
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            predicates.Add(a =>
                a.Item.Title.Contains(searchTerm) ||
                a.Item.Description.Contains(searchTerm));
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            predicates.Add(a =>
                a.Item.Category != null && (
                    a.Item.Category.Slug == category.ToLower() ||
                    a.Item.Category.Name.ToLower() == category.ToLower()));
        }

        if (isFeatured.HasValue)
        {
            predicates.Add(a => a.IsFeatured == isFeatured.Value);
        }

        if (predicates.Any())
        {
            SetCriteria(CombinePredicates(predicates));
        }

        AddInclude(a => a.Item);
        AddInclude("Item.Category");
        AddInclude("Item.Files");
    }

    private static Expression<Func<Auction, bool>> CombinePredicates(List<Expression<Func<Auction, bool>>> predicates)
    {
        if (!predicates.Any())
            return a => true;

        var combined = predicates[0];
        for (int i = 1; i < predicates.Count; i++)
        {
            combined = And(combined, predicates[i]);
        }
        return combined;
    }

    private static Expression<Func<T, bool>> And<T>(Expression<Func<T, bool>> left, Expression<Func<T, bool>> right)
    {
        var parameter = Expression.Parameter(typeof(T));
        var leftVisitor = new ReplaceExpressionVisitor(left.Parameters[0], parameter);
        var leftBody = leftVisitor.Visit(left.Body);
        var rightVisitor = new ReplaceExpressionVisitor(right.Parameters[0], parameter);
        var rightBody = rightVisitor.Visit(right.Body);
        return Expression.Lambda<Func<T, bool>>(Expression.AndAlso(leftBody!, rightBody!), parameter);
    }

    private class ReplaceExpressionVisitor : ExpressionVisitor
    {
        private readonly Expression _oldValue;
        private readonly Expression _newValue;

        public ReplaceExpressionVisitor(Expression oldValue, Expression newValue)
        {
            _oldValue = oldValue;
            _newValue = newValue;
        }

        public override Expression? Visit(Expression? node)
        {
            return node == _oldValue ? _newValue : base.Visit(node);
        }
    }
}
