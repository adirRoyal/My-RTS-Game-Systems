public enum ResourceType
{
    Wood,
    Gold,
    Stone,
    Food,
    // ניתן להוסיף משאבים חדשים בלי לשנות לוגיקה אחרת
}

public class Resource
{
    public ResourceType Type { get; private set; }
    public int Amount { get; private set; }

    public Resource(ResourceType type, int initialAmount)
    {
        Type = type;
        Amount = initialAmount;
    }

    // הוספת או הפחתת כמות משאב
    public void Add(int amount)
    {
        Amount += amount;
    }

    public bool Consume(int amount)
    {
        if (Amount >= amount)
        {
            Amount -= amount;
            return true;
        }
        return false; // לא מספיק משאבים
    }
}
