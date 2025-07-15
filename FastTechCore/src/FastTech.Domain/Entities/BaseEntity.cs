using FastTech.Domain.Entities.Interfaces;

namespace FastTech.Domain.Entities;

public abstract class BaseEntity : Identifier, IBaseEntity
{
    public DateTime CreatedAt { get; set; }
    public Guid CreatedBy { get; set; }
    public bool Removed { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public DateTime? RemovedAt { get; set; }
    public Guid? RemovedBy { get; set; }

    public virtual void PrepareToInsert(Guid userId)
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.Now;
        CreatedBy = userId;
    }

    public virtual void PrepareToUpdate(Guid userId)
    {
        UpdatedAt = DateTime.Now;
        UpdatedBy = userId;
    }

    public virtual void PrepareToRemove(Guid userId)
    {
        Removed = true;
        RemovedAt = DateTime.Now;
        RemovedBy = userId;
    }
}