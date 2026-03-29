using UmbLink.Infrastructure.Data.Entities;

namespace UmbLink.Infrastructure.Repositories;

public interface IPaymentMethodRepository
{
    Task<PaymentMethod?> GetByUserIdAsync(Guid userId);
    Task SaveAsync(PaymentMethod method);
}
