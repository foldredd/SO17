namespace SO17.Domain.Entities;

public sealed class Customer : Entity
{
    public string Name { get; set; } = "";
    public string Surname { get; set; } = "";
    public string Phone { get; set; } = "";
}
