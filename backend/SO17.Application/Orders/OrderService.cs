using SO17.Application.Abstractions;
using SO17.Application.Common;
using SO17.Domain.Entities;

namespace SO17.Application.Orders;

public sealed class OrderService(IOrderRepository orders, ICustomerRepository customers,
    IItemRepository items, IUnitOfWork unitOfWork) : IOrderService
{
    public async Task<Page<OrderDto>> ListAsync(PageQuery query, OrderStatus? status, CancellationToken ct)
    {
        Validation.Ensure(query);
        if (status.HasValue && !Enum.IsDefined(status.Value))
            throw new AppException(400, "Неизвестный статус заказа.");
        var page = await orders.ListAsync(query, status, ct);
        return new(page.Items.Select(Map).ToList(), page.Total, page.PageNumber, page.PageSize);
    }

    public async Task<OrderDto> GetAsync(int id, CancellationToken ct) => Map(await FindAsync(id, ct));

    public async Task<OrderDto> CreateAsync(CreateOrderRequest request, string userId, CancellationToken ct)
    {
        Validation.Ensure(request);
        foreach (var line in request.Lines)
        {
            if (line is null) throw new AppException(400, "Строка заказа не может быть пустой.");
            Validation.Ensure(line);
        }
        if (request.Lines.Select(line => line.ItemId).Distinct().Count() != request.Lines.Count)
            throw new AppException(400, "Об’єднайте однакові товари в один рядок.");
        var customer = await customers.FindAsync(request.CustomerId, ct) ?? throw AppException.NotFound();
        var catalog = (await items.FindManyAsync(request.Lines.Select(line => line.ItemId), ct)).ToDictionary(item => item.Id);
        var order = new Order
        {
            CustomerId = customer.Id,
            CustomerName = $"{customer.Name} {customer.Surname}",
            CustomerPhone = customer.Phone,
            CreatedById = userId
        };
        foreach (var line in request.Lines)
        {
            if (!catalog.TryGetValue(line.ItemId, out var item))
                throw new AppException(400, "Один із товарів не знайдено.");
            if (item.Quantity < line.Quantity)
                throw AppException.Conflict($"Недостатньо товару «{item.Title}». Доступно: {item.Quantity}.");
            item.Quantity -= line.Quantity;
            order.Lines.Add(new OrderLine
            {
                ItemId = item.Id,
                Title = item.Title,
                Sku = item.Sku,
                Quantity = line.Quantity,
                UnitPrice = item.Price
            });
        }
        orders.Add(order);
        // One SaveChanges transaction commits the order, all lines, and stock together.
        // Item concurrency tokens prevent two requests from selling the same stock.
        await unitOfWork.SaveAsync(ct);
        return Map(order);
    }

    public async Task<OrderDto> ChangeStatusAsync(int id, StatusRequest request, CancellationToken ct)
    {
        var order = await FindAsync(id, ct);
        Validation.Version(order.Version, request.Version);
        if (!order.CanTransitionTo(request.Status))
            throw AppException.Conflict("Цей перехід статусу недоступний.");
        if (request.Status == OrderStatus.Cancelled)
        {
            var catalog = (await items.FindManyAsync(order.Lines.Select(line => line.ItemId), ct)).ToDictionary(item => item.Id);
            foreach (var line in order.Lines)
            {
                var item = catalog[line.ItemId];
                if (item.Quantity > 1000000 - line.Quantity)
                    throw AppException.Conflict("Повернення перевищить максимальний залишок. Спершу скоригуйте склад.");
                item.Quantity += line.Quantity;
            }
        }
        order.TransitionTo(request.Status);
        await unitOfWork.SaveAsync(ct);
        return Map(order);
    }

    public async Task<DashboardDto> DashboardAsync(CancellationToken ct)
    {
        var totals = await orders.StatisticsAsync(ct);
        var recent = await orders.ListAsync(new PageQuery { PageSize = 5 }, null, ct);
        return new(totals.Customers, totals.Items, totals.ActiveOrders, totals.Revenue, recent.Items.Select(Map).ToList());
    }

    private async Task<Order> FindAsync(int id, CancellationToken ct) =>
        await orders.FindAsync(id, ct) ?? throw AppException.NotFound();

    private static OrderDto Map(Order order) => new(order.Id, order.Number, order.CreatedAt, order.CustomerId,
        order.CustomerName, order.CustomerPhone, order.Status, order.Total,
        order.Lines.Select(line => new OrderLineDto(line.ItemId, line.Title, line.Sku, line.Quantity,
            line.UnitPrice, line.UnitPrice * line.Quantity)).ToList(), order.Version);
}
