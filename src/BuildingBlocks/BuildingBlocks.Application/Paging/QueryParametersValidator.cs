using BuildingBlocks.Application.Constants;
using FluentValidation;

namespace BuildingBlocks.Application.Paging;

public class QueryParametersValidator : AbstractValidator<QueryParameters>
{
    public QueryParametersValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Page must be greater than or equal to 1");

        RuleFor(x => x.PageSize)
            .GreaterThanOrEqualTo(1)
            .WithMessage("PageSize must be greater than or equal to 1")
            .LessThanOrEqualTo(PaginationDefaults.MaxPageSize)
            .WithMessage($"PageSize must not exceed {PaginationDefaults.MaxPageSize}");

        RuleFor(x => x.SortBy)
            .MaximumLength(100)
            .When(x => x.SortBy != null)
            .WithMessage("SortBy field name is too long");
    }
}

public abstract class QueryParametersValidator<TFilter> : AbstractValidator<QueryParameters<TFilter>>
    where TFilter : class, new()
{
    protected QueryParametersValidator(IValidator<TFilter>? filterValidator = null)
    {
        Include(new QueryParametersValidator());

        if (filterValidator != null)
        {
            RuleFor(x => x.Filter)
                .SetValidator(filterValidator);
        }
    }
}
