using System;
using System.Collections.Generic;

/// <summary>
/// Manages all resources and population/supply in the game.
/// Tracks amounts, fires events for UI updates, and handles supply caps.
/// </summary>
public class ResourceManager
{
    // Dictionary holding all resources (Gold, Wood, Food, etc.)
    private Dictionary<ResourceType, Resource> resources = new Dictionary<ResourceType, Resource>();

    // Population / Supply tracking
    public int CurrentSupply { get; private set; } // current used supply
    public int MaxSupply { get; private set; }     // max allowed supply

    // Events so UI or other systems can react when resources or supply change
    public event Action<ResourceType, int> OnResourceAmountChanged;
    public event Action<int, int> OnSupplyChanged; // current, max

    // Constructor
    public ResourceManager()
    {
        // Initialize all resource types to 0
        foreach (ResourceType type in Enum.GetValues(typeof(ResourceType)))
        {
            resources[type] = new Resource(type, 0);
        }

        CurrentSupply = 0;
        MaxSupply = 0;
    }

    #region Generic Resource Access

    // Get the current amount of a resource
    public int GetAmount(ResourceType type) => resources[type].Amount;

    // Add some resource (e.g., player collected gold)
    public void AddResource(ResourceType type, int amount)
    {
        resources[type].Add(amount);
        OnResourceAmountChanged?.Invoke(type, resources[type].Amount); // notify UI
    }

    // Spend a resource, return true if success
    public bool ConsumeResource(ResourceType type, int amount)
    {
        bool success = resources[type].Consume(amount);
        if (success)
            OnResourceAmountChanged?.Invoke(type, resources[type].Amount); // update UI
        return success;
    }

    // Check if we have enough of multiple resources
    public bool HasEnoughResources(Dictionary<ResourceType, int> requiredResources)
    {
        foreach (var pair in requiredResources)
        {
            if (GetAmount(pair.Key) < pair.Value)
                return false; // not enough
        }
        return true; // enough
    }

    // Consume multiple resources at once, only if enough
    public bool ConsumeResources(Dictionary<ResourceType, int> requiredResources)
    {
        if (!HasEnoughResources(requiredResources))
            return false; // fail if not enough

        foreach (var pair in requiredResources)
        {
            ConsumeResource(pair.Key, pair.Value);
        }
        return true;
    }

    #endregion

    #region Convenience Wrappers
    // Quick access properties for common resources
    public int Gold => GetAmount(ResourceType.Gold);
    public int Wood => GetAmount(ResourceType.Wood);
    public int Food => GetAmount(ResourceType.Food);
    #endregion

    #region Supply System
    // Increase max supply (e.g., building a house or supply depot)
    public void AddSupplyCap(int amount)
    {
        MaxSupply += amount;
        OnSupplyChanged?.Invoke(CurrentSupply, MaxSupply);
    }

    // Spend supply when creating a unit
    public void ConsumeSupply(int amount)
    {
        CurrentSupply += amount;
        OnSupplyChanged?.Invoke(CurrentSupply, MaxSupply);
    }

    // Free supply when a unit dies or is removed
    public void ReleaseSupply(int amount)
    {
        CurrentSupply -= amount;
        if (CurrentSupply < 0) CurrentSupply = 0;
        OnSupplyChanged?.Invoke(CurrentSupply, MaxSupply);
    }

    // Check if there is enough free supply to create a unit
    public bool HasFreeSupply(int amountNeeded)
    {
        return CurrentSupply + amountNeeded <= MaxSupply;
    }
    #endregion
}
