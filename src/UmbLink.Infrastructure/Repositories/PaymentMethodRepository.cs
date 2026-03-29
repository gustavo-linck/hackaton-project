using Microsoft.EntityFrameworkCore;
using UmbLink.Infrastructure.Data;
using UmbLink.Infrastructure.Data.Entities;

namespace UmbLink.Infrastructure.Repositories;

public class PaymentMethodRepository(AppDbContext db) : IPaymentMethodRepository
{
    public async Task<PaymentMethod?> GetByUserIdAsync(Guid userId)
        => await db.PaymentMethods.FirstOrDefaultAsync(p => p.UserId == userId);

    public async Task SaveAsync(PaymentMethod method)
    {
        var existing = await GetByUserIdAsync(method.UserId);
        if (existing is null)
            db.PaymentMethods.Add(method);
        else
        {
            existing.CardLastFour = method.CardLastFour;
            existing.CardHolder = method.CardHolder;
            existing.ExpiryMonth = method.ExpiryMonth;
            existing.ExpiryYear = method.ExpiryYear;
            existing.Brand = method.Brand;
        }
        await db.SaveChangesAsync();
    }
}
