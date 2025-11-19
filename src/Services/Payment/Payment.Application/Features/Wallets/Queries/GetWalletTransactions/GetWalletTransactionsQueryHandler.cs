using AutoMapper;
using Payment.Application.DTOs;
using Payment.Application.Interfaces;

namespace Payment.Application.Features.Wallets.Queries.GetWalletTransactions;

public class GetWalletTransactionsQueryHandler : IQueryHandler<GetWalletTransactionsQuery, GetWalletTransactionsResult>
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

    public async Task<Result<GetWalletTransactionsResult>> Handle(GetWalletTransactionsQuery request, CancellationToken cancellationToken)
    {
        var transactions = await _transactionRepository.GetByUsernameAsync(request.Username, request.Page, request.PageSize);
        var totalCount = await _transactionRepository.GetCountByUsernameAsync(request.Username);
        
        return new GetWalletTransactionsResult(
            _mapper.Map<IEnumerable<WalletTransactionDto>>(transactions),
            totalCount);
    }
}
