// Types of resources in the game
public enum ResourceType
{
    Wood,
    Gold,
    Stone,
    Food,
    // You can add new resources here without changing other logic
}

/// <summary>
/// Represents a single resource type and its current amount.
/// </summary>
public class Resource
{
    // Type of this resource (Wood, Gold, etc.)
    public ResourceType Type { get; private set; }

    // Current amount of this resource
    public int Amount { get; private set; }

    // Constructor to create resource with initial amount
    public Resource(ResourceType type, int initialAmount)
    {
        Type = type;
        Amount = initialAmount;
    }

    // Add some amount to this resource
    public void Add(int amount)
    {
        Amount += amount;
        // Optionally, you could add a max cap here
    }

    // Try to spend/consume some of this resource
    public bool Consume(int amount)
    {
        if (Amount >= amount) // enough to spend?
        {
            Amount -= amount;
            return true; // success
        }
        return false; // not enough resource
    }
}
