using Godot;

[GlobalClass]
public partial class MinerData : Resource
{
    [Export]
    public int MaxHealth { get; set; } = 150;

    [Export]
    public int ProductionAmount { get; set; } = 5;

    [Export]
    public float ProductionInterval { get; set; } = 2.0f;
}