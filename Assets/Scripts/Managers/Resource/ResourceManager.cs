using System;
using System.Collections.Generic;

public class ResourceManager
{
    private Dictionary<ResourceType, Resource> resources = new Dictionary<ResourceType, Resource>();

    // אירועים לשינוי משאבים (לממשק משתמש או מערכות אחרות)
    public event Action<ResourceType, int> OnResourceAmountChanged;

    public ResourceManager()
    {
        // אתחול המשאבים ההתחלתי - אפשר לשנות לפי צורך
        foreach (ResourceType type in Enum.GetValues(typeof(ResourceType)))
        {
            resources[type] = new Resource(type, 0);
        }
    }

    public int GetAmount(ResourceType type)
    {
        return resources[type].Amount;
    }

    public void AddResource(ResourceType type, int amount)
    {
        resources[type].Add(amount);
        OnResourceAmountChanged?.Invoke(type, resources[type].Amount);
    }

    public bool ConsumeResource(ResourceType type, int amount)
    {
        bool success = resources[type].Consume(amount);
        if (success)
        {
            OnResourceAmountChanged?.Invoke(type, resources[type].Amount);
        }
        return success;
    }

    // בדיקה אם יש מספיק משאבים מסוגים שונים (רשימה)
    public bool HasEnoughResources(Dictionary<ResourceType, int> requiredResources)
    {
        foreach (var pair in requiredResources)
        {
            if (GetAmount(pair.Key) < pair.Value)
                return false;
        }
        return true;
    }

    // צריכה מרובת משאבים (כל המשאבים ביחד)
    public bool ConsumeResources(Dictionary<ResourceType, int> requiredResources)
    {
        if (!HasEnoughResources(requiredResources))
            return false;

        foreach (var pair in requiredResources)
        {
            ConsumeResource(pair.Key, pair.Value);
        }
        return true;
    }
}
