using Godot;

public partial class ResourceUI : CanvasLayer
{
    private Label metalLabel;
    private ProgressBar metalProgressBar;

    public override void _Ready()
    {
        metalLabel = GetNode<Label>("MetalPanel/MetalLabel");
        metalProgressBar = GetNode<ProgressBar>("MetalPanel/MetalProgressBar");

        ResourceManager.Instance.ResourceChanged += OnResourceChanged;

        UpdateMetalUI(
            ResourceManager.Instance.GetAmount(ResourceType.Metal),
            ResourceManager.Instance.GetLimit(ResourceType.Metal)
        );
    }

    private void OnResourceChanged(ResourceType type, int amount, int limit)
    {
        if (type != ResourceType.Metal)
            return;

        UpdateMetalUI(amount, limit);
    }

    private void UpdateMetalUI(int amount, int limit)
    {
        metalLabel.Text = $"Metal: {amount} / {limit}";
        metalProgressBar.MaxValue = limit;
        metalProgressBar.Value = amount;
    }

    public override void _ExitTree()
    {
        if (ResourceManager.Instance != null)
        {
            ResourceManager.Instance.ResourceChanged -= OnResourceChanged;
        }
    }
}