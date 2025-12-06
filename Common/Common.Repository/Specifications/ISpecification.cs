using System.Linq.Expressions;

namespace Common.Repository.Specifications;

public interface ISpecification<T> where T : class
{
    Expression<Func<T, bool>> Criteria { get; }

    List<Expression<Func<T, object>>> Includes { get; }

    List<string> IncludeStrings { get; }

    Expression<Func<T, object>>? OrderBy { get; }

    Expression<Func<T, object>>? OrderByDescending { get; }

    int Take { get; }
    int Skip { get; }
    bool IsPagingEnabled { get; }
}

public abstract class BaseSpecification<T> : ISpecification<T> where T : class
{
    public Expression<Func<T, bool>> Criteria { get; private set; } = _ => true;
    public List<Expression<Func<T, object>>> Includes { get; } = new();
    public List<string> IncludeStrings { get; } = new();
    public Expression<Func<T, object>>? OrderBy { get; private set; }
    public Expression<Func<T, object>>? OrderByDescending { get; private set; }
    public int Take { get; private set; }
    public int Skip { get; private set; }
    public bool IsPagingEnabled { get; private set; }

    protected BaseSpecification()
    {
    }

    protected BaseSpecification(Expression<Func<T, bool>> criteria)
    {
        Criteria = criteria;
    }

    protected virtual void AddInclude(Expression<Func<T, object>> includeExpression)
    {
        Includes.Add(includeExpression);
    }

    protected virtual void AddInclude(string includeString)
    {
        IncludeStrings.Add(includeString);
    }

    protected virtual void ApplyPaging(int skip, int take)
    {
        Skip = skip;
        Take = take;
        IsPagingEnabled = true;
    }

    protected virtual void ApplyOrderBy(Expression<Func<T, object>> orderByExpression)
    {
        OrderBy = orderByExpression;
    }

    protected virtual void ApplyOrderByDescending(Expression<Func<T, object>> orderByDescendingExpression)
    {
        OrderByDescending = orderByDescendingExpression;
    }

    protected virtual void SetCriteria(Expression<Func<T, bool>> criteria)
    {
        Criteria = criteria;
    }
}

public static class SpecificationExtensions
{
    public static ISpecification<T> And<T>(this ISpecification<T> left, ISpecification<T> right) where T : class
    {
        return new AndSpecification<T>(left, right);
    }
    public static ISpecification<T> Or<T>(this ISpecification<T> left, ISpecification<T> right) where T : class
    {
        return new OrSpecification<T>(left, right);
    }

    public static ISpecification<T> Not<T>(this ISpecification<T> specification) where T : class
    {
        return new NotSpecification<T>(specification);
    }
}

internal sealed class AndSpecification<T> : BaseSpecification<T> where T : class
{
    public AndSpecification(ISpecification<T> left, ISpecification<T> right)
    {
        var parameter = Expression.Parameter(typeof(T), "x");

        var leftVisitor = new ReplaceExpressionVisitor(left.Criteria.Parameters[0], parameter);
        var leftBody = leftVisitor.Visit(left.Criteria.Body);

        var rightVisitor = new ReplaceExpressionVisitor(right.Criteria.Parameters[0], parameter);
        var rightBody = rightVisitor.Visit(right.Criteria.Body);

        SetCriteria(Expression.Lambda<Func<T, bool>>(
            Expression.AndAlso(leftBody, rightBody), parameter));

        foreach (var include in left.Includes.Concat(right.Includes))
            Includes.Add(include);
    }
}

internal sealed class OrSpecification<T> : BaseSpecification<T> where T : class
{
    public OrSpecification(ISpecification<T> left, ISpecification<T> right)
    {
        var parameter = Expression.Parameter(typeof(T), "x");

        var leftVisitor = new ReplaceExpressionVisitor(left.Criteria.Parameters[0], parameter);
        var leftBody = leftVisitor.Visit(left.Criteria.Body);

        var rightVisitor = new ReplaceExpressionVisitor(right.Criteria.Parameters[0], parameter);
        var rightBody = rightVisitor.Visit(right.Criteria.Body);

        SetCriteria(Expression.Lambda<Func<T, bool>>(
            Expression.OrElse(leftBody, rightBody), parameter));
    }
}

internal sealed class NotSpecification<T> : BaseSpecification<T> where T : class
{
    public NotSpecification(ISpecification<T> specification)
    {
        var parameter = Expression.Parameter(typeof(T), "x");

        var visitor = new ReplaceExpressionVisitor(specification.Criteria.Parameters[0], parameter);
        var body = visitor.Visit(specification.Criteria.Body);

        SetCriteria(Expression.Lambda<Func<T, bool>>(
            Expression.Not(body), parameter));
    }
}

internal sealed class ReplaceExpressionVisitor : ExpressionVisitor
{
    private readonly Expression _oldValue;
    private readonly Expression _newValue;

    public ReplaceExpressionVisitor(Expression oldValue, Expression newValue)
    {
        _oldValue = oldValue;
        _newValue = newValue;
    }

    public override Expression Visit(Expression? node)
    {
        return node == _oldValue ? _newValue : base.Visit(node)!;
    }
}
