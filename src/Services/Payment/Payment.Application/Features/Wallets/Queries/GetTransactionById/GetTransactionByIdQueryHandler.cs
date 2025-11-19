using AutoMapper;
using Payment.Application.DTOs;
using Payment.Application.Interfaces;

namespace Payment.Application.Features.Wallets.Queries.GetTransactionById;

public class GetTransactionByIdQueryHandler : IQueryHandler<GetTransactionByIdQuery, WalletTransactionDto?>
{
    private readonly IWalletTransactionRepository _transactionRepository;
    private readonly IMapper _mapper;

    public GetTransactionByIdQueryHandler(
        IWalletTransactionRepository transactionRepository,
        IMapper mapper)
    {
        _transactionRepository = transactionRepository;
        _mapper = mapper;
    }

    public async Task<Result<WalletTransactionDto?>> Handle(GetTransactionByIdQuery request, CancellationToken cancellationToken)
    {
        var transaction = await _transactionRepository.GetByIdAsync(request.TransactionId);
        return transaction != null ? _mapper.Map<WalletTransactionDto>(transaction) : null;
    }
}
