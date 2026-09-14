using System.ComponentModel.DataAnnotations;

namespace SO17.Application.Customers;

public sealed record CustomerDto(int Id, string Name, string Surname, string Phone, Guid Version);
public sealed class CustomerRequest
{
    [Required, StringLength(80)] public string Name { get; init; } = "";
    [Required, StringLength(80)] public string Surname { get; init; } = "";
    [Required, StringLength(30)] public string Phone { get; init; } = "";
    public Guid Version { get; init; }
}
