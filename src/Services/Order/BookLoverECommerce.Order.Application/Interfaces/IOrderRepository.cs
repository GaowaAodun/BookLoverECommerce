using BookLoverECommerce.Order.Domain.Entities;

namespace BookLoverECommerce.Order.Application.Interfaces;

public interface IOrderRepository
{
    Task AddAsync(
        Domain.Entities.Order order,
        CancellationToken cancellationToken = default);

    Task<Domain.Entities.Order?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Domain.Entities.Order>> GetByCustomerIdAsync(
        string customerId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Domain.Entities.Order>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}