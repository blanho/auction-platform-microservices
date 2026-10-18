using Catalog.Api.Endpoints.Brands;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Catalog.Infrastructure.Tests;

public sealed class BrandRequestBindingTests
{
    [Fact]
    public async Task OmittedQueryValues_KeepTheExistingBrandListingDefaults()
    {
        var request = await BindAsync(string.Empty);

        Assert.True(request.ActiveOnly);
        Assert.False(request.FeaturedOnly);
        Assert.Equal(1, request.Page);
        Assert.Equal(10, request.PageSize);
        Assert.Null(request.Search);
        Assert.Null(request.SortBy);
        Assert.Null(request.SortOrder);
    }

    [Fact]
    public async Task ExistingQueryNames_BindFiltersPagingAndSortDirection()
    {
        var request = await BindAsync("?activeOnly=false&featuredOnly=true&page=2&pageSize=5&search=watch&sortBy=name&sortOrder=asc");

        Assert.False(request.ActiveOnly);
        Assert.True(request.FeaturedOnly);
        Assert.Equal(2, request.Page);
        Assert.Equal(5, request.PageSize);
        Assert.Equal("watch", request.Search);
        Assert.Equal("name", request.SortBy);
        Assert.Equal("asc", request.SortOrder);
    }

    [Fact]
    public async Task InvalidNumericQuery_RemainsAnHttpBindingError()
    {
        var error = await Assert.ThrowsAsync<BadHttpRequestException>(() => BindAsync("?page=invalid"));
        Assert.Equal(StatusCodes.Status400BadRequest, error.StatusCode);
    }

    private static async Task<GetBrandsRequest> BindAsync(string query)
    {
        using var services = new ServiceCollection().AddLogging().BuildServiceProvider();
        GetBrandsRequest? bound = null;
        void Capture([AsParameters] GetBrandsRequest request) => bound = request;
        var handler = RequestDelegateFactory.Create(Capture, new RequestDelegateFactoryOptions
        {
            ServiceProvider = services,
            ThrowOnBadRequest = true
        }).RequestDelegate;
        var context = new DefaultHttpContext { RequestServices = services };
        context.Request.Method = HttpMethods.Get;
        context.Request.QueryString = new QueryString(query);
        await handler(context);
        return Assert.IsType<GetBrandsRequest>(bound);
    }
}
