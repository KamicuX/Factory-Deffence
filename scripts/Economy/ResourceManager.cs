using Godot;

public partial class ResourceManager : Node
{
    public int Metal { get; private set; }
    public int MaxMetal { get; private set; } = 500;

    public void AddMetal(int amount)
    {
        Metal = Mathf.Min(Metal + amount, MaxMetal);
    }

    public bool RemoveMetal(int amount)
    {
        if (Metal < amount)
            return false;

        Metal -= amount;
        return true;
    }

    public bool HasMetal(int amount)
    {
        return Metal >= amount;
    }
}