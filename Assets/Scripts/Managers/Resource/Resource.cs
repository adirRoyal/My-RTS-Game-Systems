public enum ResourceType
{
    Wood,
    Gold,
    Stone,
    Food,
    // ���� ������ ������ ����� ��� ����� ������ ����
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

    // ����� �� ����� ���� ����
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
        return false; // �� ����� ������
    }
}
