namespace ScheduleService.Domain.Models;

public abstract class Entity
{
    protected Entity()
    {
    }

    protected Entity(string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        Id = id;
    }

    public string Id { get; protected set; } = string.Empty;
}
