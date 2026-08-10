using BookLoverECommerce.Order.Application.Interfaces;
using BookLoverECommerce.Order.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BookLoverECommerce.Order.Infrastructure.Repositories;

public sealed class OrderRepository : IOrderRepository
{
    private readonly OrderDbContext _dbContext;

    public OrderRepository(OrderDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(
        Domain.Entities.Order order,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.Orders.AddAsync(
            order,
            cancellationToken);
    }

    public Task<Domain.Entities.Order?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Orders
            .Include(order => order.Items)
            .FirstOrDefaultAsync(
                order => order.Id == id,
                cancellationToken);
    }

    public async Task<IReadOnlyCollection<Domain.Entities.Order>>
        GetByCustomerIdAsync(
            string customerId,
            CancellationToken cancellationToken = default)
    {
        return await _dbContext.Orders
            .AsNoTracking()
            .Include(order => order.Items)
            .Where(order => order.CustomerId == customerId)
            .OrderByDescending(order => order.CreatedAt)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Domain.Entities.Order>>
        GetAllAsync(
            CancellationToken cancellationToken = default)
    {
        return await _dbContext.Orders
            .AsNoTracking()
            .Include(order => order.Items)
            .OrderByDescending(order => order.CreatedAt)
            .ToArrayAsync(cancellationToken);
    }

    public Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(
            cancellationToken);
    }
}