using BuildingBlocks.Application.Constants;
using BuildingBlocks.Domain.Constants;
using FluentValidation;

namespace BuildingBlocks.Application.Paging;

public class QueryParametersValidator : AbstractValidator<QueryParameters>
{
    public QueryParametersValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage(ValidationConstants.Messages.MustBeAtLeast("Page", 1));

        RuleFor(x => x.PageSize)
            .GreaterThanOrEqualTo(1)
            .WithMessage(ValidationConstants.Messages.MustBeAtLeast("PageSize", 1))
            .LessThanOrEqualTo(PaginationDefaults.MaxPageSize)
            .WithMessage(ValidationConstants.Messages.MustNotExceed("PageSize", PaginationDefaults.MaxPageSize));

        RuleFor(x => x.SortBy)
            .MaximumLength(100)
            .When(x => x.SortBy != null)
            .WithMessage(ValidationConstants.Messages.MaxLength("SortBy", 100));
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
