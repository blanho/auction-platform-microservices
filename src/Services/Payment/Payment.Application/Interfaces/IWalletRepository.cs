using Payment.Domain.Entities;

namespace Payment.Application.Interfaces;

public interface IWalletRepository
{
    Task<Wallet> GetByUsernameAsync(string username);
    Task<Wallet> AddAsync(Wallet wallet);
    Task<Wallet> UpdateAsync(Wallet wallet);
    Task<bool> ExistsAsync(string username);
}
