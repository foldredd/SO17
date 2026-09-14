namespace SO17.Domain.Entities;

public sealed class Item : Entity
{
    public string Title { get; set; } = "";
    public string Sku { get; set; } = "";
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}
