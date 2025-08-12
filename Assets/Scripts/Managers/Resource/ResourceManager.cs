using System;
using System.Collections.Generic;

public class ResourceManager
{
    private Dictionary<ResourceType, Resource> resources = new Dictionary<ResourceType, Resource>();

    // ������� ������ ������ (����� ����� �� ������ �����)
    public event Action<ResourceType, int> OnResourceAmountChanged;

    public ResourceManager()
    {
        // ����� ������� ������� - ���� ����� ��� ����
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

    // ����� �� �� ����� ������ ������ ����� (�����)
    public bool HasEnoughResources(Dictionary<ResourceType, int> requiredResources)
    {
        foreach (var pair in requiredResources)
        {
            if (GetAmount(pair.Key) < pair.Value)
                return false;
        }
        return true;
    }

    // ����� ����� ������ (�� ������� ����)
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
