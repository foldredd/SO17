using System.ComponentModel.DataAnnotations;

namespace SO17.Application.Items;

public sealed record ItemDto(int Id, string Title, string Sku, int Quantity, decimal Price, Guid Version);
public sealed class ItemRequest
{
    [Required, StringLength(160)] public string Title { get; init; } = "";
    [Required, StringLength(50), RegularExpression(@"[A-Za-z0-9][A-Za-z0-9._\-]*")]
    public string Sku { get; init; } = "";
    [Range(0, 1000000)] public int Quantity { get; init; }
    [Range(typeof(decimal), "0.01", "99999999.99", ParseLimitsInInvariantCulture = true, ConvertValueInInvariantCulture = true)]
    public decimal Price { get; init; }
    public Guid Version { get; init; }
}
