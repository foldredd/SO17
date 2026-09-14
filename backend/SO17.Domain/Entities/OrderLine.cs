namespace SO17.Domain.Entities;

public sealed class OrderLine
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int ItemId { get; set; }
    public Item Item { get; set; } = null!;
    public string Title { get; set; } = "";
    public string Sku { get; set; } = "";
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
