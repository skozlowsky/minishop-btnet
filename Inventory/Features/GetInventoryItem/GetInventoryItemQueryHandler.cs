using Inventory.Database;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Features.GetInventoryItem;

public class GetInventoryItemQueryHandler : IRequestHandler<GetInventoryItemQuery, ItemDetailDto?>
{
    private readonly InventoryContext _context;

    public GetInventoryItemQueryHandler(InventoryContext context)
    {
        _context = context;
    }

    public async Task<ItemDetailDto?> Handle(GetInventoryItemQuery query, CancellationToken ct)
    {
        var item = await _context.Items
            .Include(i => i.Transactions.OrderByDescending(t => t.CreatedAt).Take(10))
            .FirstOrDefaultAsync(i => i.Id == query.Id, ct);

        if (item == null)
            return null;

        return new ItemDetailDto(
            item.Id,
            item.Sku,
            item.Name,
            item.AvailableQuantity,
            item.ReservedQuantity,
            item.Price,
            item.CreatedAt,
            item.UpdatedAt,
            item.Transactions.Select(t => new TransactionDto(
                t.Id,
                t.Type,
                t.Quantity,
                t.Reference ?? string.Empty,
                t.CreatedAt,
                t.CreatedBy)));
    }
}