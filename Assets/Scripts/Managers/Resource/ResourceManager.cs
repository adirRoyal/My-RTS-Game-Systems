using System;
using System.Collections.Generic;

public class ResourceManager
{
    private Dictionary<ResourceType, Resource> resources = new Dictionary<ResourceType, Resource>();

    // Population 
    public int CurrentSupply { get; private set; }
    public int MaxSupply { get; private set; }

    // Events for UI updates
    public event Action<ResourceType, int> OnResourceAmountChanged;
    public event Action<int, int> OnSupplyChanged; // current, max

    public ResourceManager()
    {
        // Initialize all resources at 0
        foreach (ResourceType type in Enum.GetValues(typeof(ResourceType)))
        {
            resources[type] = new Resource(type, 0);
        }

        CurrentSupply = 0;
        MaxSupply = 0;
    }

    #region Generic Resource Access
    public int GetAmount(ResourceType type) => resources[type].Amount;

    public void AddResource(ResourceType type, int amount)
    {
        resources[type].Add(amount);
        OnResourceAmountChanged?.Invoke(type, resources[type].Amount);
    }

    public bool ConsumeResource(ResourceType type, int amount)
    {
        bool success = resources[type].Consume(amount);
        if (success)
            OnResourceAmountChanged?.Invoke(type, resources[type].Amount);
        return success;
    }

    public bool HasEnoughResources(Dictionary<ResourceType, int> requiredResources)
    {
        foreach (var pair in requiredResources)
        {
            if (GetAmount(pair.Key) < pair.Value)
                return false;
        }
        return true;
    }

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
    #endregion

    #region Convenience Wrappers
    // Quick accessors (old style compatibility)
    public int Gold => GetAmount(ResourceType.Gold);
    public int Wood => GetAmount(ResourceType.Wood);
    public int Food => GetAmount(ResourceType.Food);
    #endregion

    #region Supply System
    public void AddSupplyCap(int amount)
    {
        MaxSupply += amount;
        OnSupplyChanged?.Invoke(CurrentSupply, MaxSupply);
    }

    public void ConsumeSupply(int amount)
    {
        CurrentSupply += amount;
        OnSupplyChanged?.Invoke(CurrentSupply, MaxSupply);
    }

    public void ReleaseSupply(int amount)
    {
        CurrentSupply -= amount;
        if (CurrentSupply < 0) CurrentSupply = 0;
        OnSupplyChanged?.Invoke(CurrentSupply, MaxSupply);
    }

    public bool HasFreeSupply(int amountNeeded)
    {
        return CurrentSupply + amountNeeded <= MaxSupply;
    }
    #endregion
}
