namespace SO17.Domain.Entities;

public sealed class Order : Entity
{
    public string Number { get; set; } = Guid.NewGuid().ToString("N");
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public string CustomerName { get; set; } = "";
    public string CustomerPhone { get; set; } = "";
    public string CreatedById { get; set; } = "";
    public OrderStatus Status { get; private set; } = OrderStatus.Pending;
    public List<OrderLine> Lines { get; set; } = [];
    public decimal Total => Lines.Sum(line => line.UnitPrice * line.Quantity);

    public bool CanTransitionTo(OrderStatus next) => (Status, next) switch
    {
        (OrderStatus.Pending, OrderStatus.Processing or OrderStatus.Cancelled) => true,
        (OrderStatus.Processing, OrderStatus.Completed or OrderStatus.Cancelled) => true,
        _ => false
    };

    public void TransitionTo(OrderStatus next)
    {
        if (!CanTransitionTo(next))
            throw new InvalidOperationException("Недопустимое изменение статуса заказа.");
        Status = next;
    }
}
