
using System.Linq.Expressions;
using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.Paging;
using Microsoft.EntityFrameworkCore;
using Payment.Application.Filtering;
using Payment.Application.Interfaces;
using Payment.Domain.Entities;
using Payment.Domain.Enums;
using Payment.Infrastructure.Persistence;

namespace Payment.Infrastructure.Repositories;

public class WalletTransactionRepository : IWalletTransactionRepository
{
    private readonly PaymentDbContext _context;

    private static readonly Dictionary<string, Expression<Func<WalletTransaction, object>>> TransactionSortMap =
        new(StringComparer.OrdinalIgnoreCase)
    {
        ["createdat"] = t => t.CreatedAt,
        ["amount"] = t => t.Amount,
        ["type"] = t => t.Type,
        ["status"] = t => t.Status
    };

    public WalletTransactionRepository(PaymentDbContext context)
    {
        _context = context;
    }

    public async Task<WalletTransaction> GetByIdAsync(Guid id)
    {
        return await _context.WalletTransactions
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<WalletTransaction?> GetByReferenceIdAsync(string referenceId, TransactionType type, CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(referenceId, out var refGuid))
            return null;
            
        return await _context.WalletTransactions
            .AsNoTracking()
            .Where(t => t.ReferenceId == refGuid && t.Type == type)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<PaginatedResult<WalletTransaction>> GetByUsernameAsync(WalletTransactionQueryParams queryParams, CancellationToken cancellationToken = default)
    {
        var query = _context.WalletTransactions.AsNoTracking();
        
        if (queryParams.Filter != null)
        {
            query = queryParams.Filter.Apply(query);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .ApplySorting(queryParams, TransactionSortMap, t => t.CreatedAt)
            .ApplyPaging(queryParams)
            .ToListAsync(cancellationToken);

        return new PaginatedResult<WalletTransaction>(items, totalCount, queryParams.Page, queryParams.PageSize);
    }

    public async Task<WalletTransaction> AddAsync(WalletTransaction transaction)
    {
        await _context.WalletTransactions.AddAsync(transaction);
        return transaction;
    }

    public async Task<WalletTransaction> UpdateAsync(WalletTransaction transaction)
    {
        _context.WalletTransactions.Update(transaction);
        return transaction;
    }

    public async Task<int> GetCountByUsernameAsync(string username)
    {
        return await _context.WalletTransactions
            .AsNoTracking()
            .CountAsync(t => t.Username == username);
    }

    public async Task<decimal> GetTotalByUsernameAndTypeAsync(string username, TransactionType type)
    {
        return await _context.WalletTransactions
            .AsNoTracking()
            .Where(t => t.Username == username && t.Type == type && t.Status == TransactionStatus.Completed)
            .SumAsync(t => t.Amount);
    }
}
