using CladTracker.Services;

namespace CladTracker.Models;

public enum EntryType
{
    Planned,
    Actual
}

public sealed class CladEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public int CladmanNumber { get; set; }
    public decimal WeightGrams { get; set; }
    public EntryType Type { get; set; }
    public DateTime CreatedAt { get; set; }

    public decimal Amount => WeightGrams * EntryStore.RatePerGram;
}
