using AutoMapper;
using BuildingBlocks.Application.Abstractions;
using Payment.Application.DTOs;
using Payment.Application.Filtering;
using Payment.Application.Interfaces;

namespace Payment.Application.Features.Wallets.GetWalletTransactions;

public class GetWalletTransactionsQueryHandler : IQueryHandler<GetWalletTransactionsQuery, PaginatedResult<WalletTransactionDto>>
{
    private readonly IWalletTransactionRepository _transactionRepository;
    private readonly IMapper _mapper;

    public GetWalletTransactionsQueryHandler(
        IWalletTransactionRepository transactionRepository,
        IMapper mapper)
    {
        _transactionRepository = transactionRepository;
        _mapper = mapper;
    }

    public async Task<Result<PaginatedResult<WalletTransactionDto>>> Handle(GetWalletTransactionsQuery request, CancellationToken cancellationToken)
    {
        var queryParams = new WalletTransactionQueryParams
        {
            Page = request.Page,
            PageSize = request.PageSize,
            SortBy = request.SortBy,
            SortDescending = request.SortDescending,
            Filter = new WalletTransactionFilter
            {
                Username = request.Username,
                Type = request.Type,
                Status = request.Status,
                MinAmount = request.MinAmount,
                MaxAmount = request.MaxAmount,
                FromDate = request.FromDate,
                ToDate = request.ToDate
            }
        };

        var result = await _transactionRepository.GetByUsernameAsync(queryParams, cancellationToken);
        
        return new PaginatedResult<WalletTransactionDto>(
            result.Items.ToDtoList(_mapper),
            result.TotalCount,
            result.Page,
            result.PageSize);
    }
}
