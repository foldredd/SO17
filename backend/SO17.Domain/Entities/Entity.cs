namespace SO17.Domain.Entities;

public abstract class Entity
{
    public int Id { get; set; }
    public Guid Version { get; set; } = Guid.NewGuid();
}
