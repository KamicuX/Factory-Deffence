using Godot;
using System;
using System.Collections.Generic;

public partial class ResourceManager : Node
{
    public static ResourceManager Instance { get; private set; }

    private readonly Dictionary<ResourceType, int> resources = new();
    private readonly Dictionary<ResourceType, int> resourceLimits = new();

    public event Action<ResourceType, int, int> ResourceChanged;

    public override void _Ready()
    {
        if (Instance != null && Instance != this)
        {
            QueueFree();
            return;
        }

        Instance = this;

        InitializeResources();
        GD.Print($"ResourceManager initialized. Metal: {GetAmount(ResourceType.Metal)}/{GetLimit(ResourceType.Metal)}");
        Add(ResourceType.Metal, 100);
    }

    private void InitializeResources()
    {
        resources[ResourceType.Metal] = 0;
        resources[ResourceType.Ammunition] = 0;
        resources[ResourceType.Fuel] = 0;

        resourceLimits[ResourceType.Metal] = 500;
        resourceLimits[ResourceType.Ammunition] = 500;
        resourceLimits[ResourceType.Fuel] = 500;
    }

    public int GetAmount(ResourceType type)
    {
        return resources.TryGetValue(type, out int amount) ? amount : 0;
    }

    public int GetLimit(ResourceType type)
    {
        return resourceLimits.TryGetValue(type, out int limit) ? limit : 0;
    }

    public bool Has(ResourceType type, int amount)
    {
        return GetAmount(type) >= amount;
    }

    public void Add(ResourceType type, int amount)
    {
        if (amount <= 0)
            return;

        int currentAmount = GetAmount(type);
        int limit = GetLimit(type);

        int newAmount = Mathf.Min(currentAmount + amount, limit);

        if (newAmount == currentAmount)
            return;

        resources[type] = newAmount;

        ResourceChanged?.Invoke(type, newAmount, limit);
    }

    public bool Remove(ResourceType type, int amount)
    {
        if (amount <= 0)
            return false;

        int currentAmount = GetAmount(type);

        if (currentAmount < amount)
            return false;

        int newAmount = currentAmount - amount;

        resources[type] = newAmount;

        ResourceChanged?.Invoke(type, newAmount, GetLimit(type));

        return true;
    }

    public bool TrySpend(ResourceType type, int amount)
    {
        return Remove(type, amount);
    }

    public void SetLimit(ResourceType type, int newLimit)
    {
        if (newLimit < 0)
            return;

        resourceLimits[type] = newLimit;

        int currentAmount = GetAmount(type);

        if (currentAmount > newLimit)
        {
            resources[type] = newLimit;
        }

        ResourceChanged?.Invoke(
            type,
            GetAmount(type),
            newLimit
        );
    }
}