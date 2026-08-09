using Godot;

public partial class BuildMenu : Panel
{
    private Button productionButton;
    private Button defenseButton;

    private VBoxContainer productionList;
    private VBoxContainer defenseList;

    private Button minerButton;
    private Button turretButton;
    private Button wallButton;
    private Grid grid;

    public override void _Ready()
    {
        productionButton = GetNode<Button>("CategoryBar/ProductionButton");
        defenseButton = GetNode<Button>("CategoryBar/DefenseButton");

        productionList = GetNode<VBoxContainer>("ProductionList");
        defenseList = GetNode<VBoxContainer>("DefenseList");

        minerButton = GetNode<Button>("ProductionList/MinerButton");

        turretButton = GetNode<Button>("DefenseList/TurretButton");
        wallButton = GetNode<Button>("DefenseList/WallButton");

        productionButton.Pressed += OnProductionPressed;
        defenseButton.Pressed += OnDefensePressed;

        grid = GetNode<Grid>("../../Grid");

        minerButton.Pressed += OnMinerPressed;
        turretButton.Pressed += OnTurretPressed;
        wallButton.Pressed += OnWallPressed;

        ShowProduction();
        SetActiveCategory(productionButton);
    }
    private void SetActiveCategory(Button activeButton)
    {
        productionButton.RemoveThemeStyleboxOverride("normal");
        defenseButton.RemoveThemeStyleboxOverride("normal");

        StyleBoxFlat activeStyle = new StyleBoxFlat();
        activeStyle.BorderWidthLeft = 3;
        activeStyle.BorderWidthRight = 3;
        activeStyle.BorderWidthTop = 3;
        activeStyle.BorderWidthBottom = 3;
        activeStyle.BorderColor = Colors.Green;

        activeButton.AddThemeStyleboxOverride("normal", activeStyle);
    }

    private void OnProductionPressed()
    {
        ShowProduction();
        SetActiveCategory(productionButton);
    }

    private void OnDefensePressed()
    {
        ShowDefense();
        SetActiveCategory(defenseButton);
    }
    private void OnMinerPressed()
    {
        GD.Print("Miner selected");
        grid.SelectBuilding("Miner");
    }
    private void OnTurretPressed()
    {
        GD.Print("Turret selected");
        grid.SelectBuilding("Turret");
    }
    private void OnWallPressed()
    {
        GD.Print("Wall selected");
        grid.SelectBuilding("Wall");
    }
    private void ShowProduction()
    {
        productionList.Visible = true;
        defenseList.Visible = false;
    }

    private void ShowDefense()
    {
        productionList.Visible = false;
        defenseList.Visible = true;
    }

}