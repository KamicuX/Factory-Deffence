using Godot;

public partial class BuildMenu : Panel
{
    // =========================
    // BUILDINGS
    // =========================

    private Button productionButton;
    private Button defenseButton;

    private VBoxContainer productionList;
    private VBoxContainer defenseList;

    private Button minerButton;
    private Button turretButton;
    private Button wallButton;

    // =========================
    // MAIN CATEGORIES
    // =========================

    private Button buildingsButton;
    private Button researchButton;

    private Panel buildingsPanel;
    private Panel researchPanel;

    // =========================
    // RESEARCH
    // =========================

    private Button turretDamageResearchButton;
    private ResearchManager researchManager;

    // =========================
    // GAME
    // =========================

    private Grid grid;

    public override void _Ready()
    {

        // =========================
        // MAIN CATEGORY BUTTONS
        // =========================

        buildingsButton =
             GetNode<Button>(
                "MainContainer/CategoryContainer/BuildingsButton"
             );

        researchButton =
             GetNode<Button>(
                "MainContainer/CategoryContainer/ResearchButton"
             );

        // =========================
        // PANELS
        // =========================

        buildingsPanel =
              GetNode<Panel>(
                 "MainContainer/ContentContainer/BuildingsPanel"
               );

        researchPanel =
            GetNode<Panel>(
                "MainContainer/ContentContainer/ResearchPanel"
            );

        // =========================
        // RESEARCH
        // =========================

        researchManager =
        GetNode<ResearchManager>("../../ResearchManager");

        // =========================
        // BUILDINGS
        // =========================

        productionButton =
            GetNode<Button>(
                "MainContainer/ContentContainer/BuildingsPanel/BuildingsContainer/Prod/ProductionButton"
            );

        defenseButton =
            GetNode<Button>(
                "MainContainer/ContentContainer/BuildingsPanel/BuildingsContainer/Prod/DefenseButton"
            );


        productionList =
            GetNode<VBoxContainer>(
                "MainContainer/ContentContainer/BuildingsPanel/BuildingsContainer/BuildingListContainer/ProductionList"
            );

        defenseList =
            GetNode<VBoxContainer>(
                "MainContainer/ContentContainer/BuildingsPanel/BuildingsContainer/BuildingListContainer/DefenseList"
            );


        minerButton =
            GetNode<Button>(
                "MainContainer/ContentContainer/BuildingsPanel/BuildingsContainer/BuildingListContainer/ProductionList/MinerButton"
            );

        turretButton =
            GetNode<Button>(
                "MainContainer/ContentContainer/BuildingsPanel/BuildingsContainer/BuildingListContainer/DefenseList/TurretButton"
            );

        wallButton =
            GetNode<Button>(
                "MainContainer/ContentContainer/BuildingsPanel/BuildingsContainer/BuildingListContainer/DefenseList/WallButton"
            );

        turretDamageResearchButton =
             GetNode<Button>(
                "MainContainer/ContentContainer/ResearchPanel/ResearchContainer/ResearchList/TurretDamageResearchButton"
            );

        // =========================
        // GRID
        // =========================


        grid = GetNode<Grid>("../../Grid");


        // =========================
        // SIGNALS
        // =========================

        buildingsButton.Pressed += OnBuildingsPressed;
        researchButton.Pressed += OnResearchPressed;

        productionButton.Pressed += OnProductionPressed;
        defenseButton.Pressed += OnDefensePressed;

        minerButton.Pressed += OnMinerPressed;
        turretButton.Pressed += OnTurretPressed;
        wallButton.Pressed += OnWallPressed;

        turretDamageResearchButton.Pressed +=
        OnTurretDamageResearchPressed;

        // =========================
        // INITIAL STATE
        // =========================

        ShowBuildings();
        ShowProduction();

        SetActiveMainCategory(buildingsButton);
        SetActiveBuildingCategory(productionButton);
    }


    

    // =========================================================
    // MAIN CATEGORY
    // =========================================================

    private void OnBuildingsPressed()
    {
        ShowBuildings();
        SetActiveMainCategory(buildingsButton);
    }

    private void OnResearchPressed()
    {
        ShowResearch();
        SetActiveMainCategory(researchButton);
    }

    private void ShowBuildings()
    {
        buildingsPanel.Visible = true;
        researchPanel.Visible = false;
    }

    private void ShowResearch()
    {
        buildingsPanel.Visible = false;
        researchPanel.Visible = true;
    }

    // =========================================================
    // BUILDING CATEGORY
    // =========================================================

    private void OnProductionPressed()
    {
        ShowProduction();
        SetActiveBuildingCategory(productionButton);
    }

    private void OnDefensePressed()
    {
        ShowDefense();
        SetActiveBuildingCategory(defenseButton);
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

    // =========================================================
    // BUILDING SELECTION
    // =========================================================

    private void OnMinerPressed()
    {
        if (DebugConfig.EnableLogs) GD.Print("Miner selected");
        grid.SelectBuilding("Miner");
    }

    private void OnTurretPressed()
    {
        if (DebugConfig.EnableLogs) GD.Print("Turret selected");
        grid.SelectBuilding("Turret");
    }
    private void OnWallPressed()
    {
        if (DebugConfig.EnableLogs) GD.Print("Wall selected");
        grid.SelectBuilding("Wall");
    }

    // =========================================================
    // MAIN CATEGORY STYLE
    // =========================================================
    private void SetActiveMainCategory(Button activeButton)
    {
        buildingsButton.RemoveThemeStyleboxOverride("normal");
        researchButton.RemoveThemeStyleboxOverride("normal");

        StyleBoxFlat activeStyle = CreateActiveStyle();

        activeButton.AddThemeStyleboxOverride(
            "normal",
            activeStyle
        );
    }

    // =========================================================
    // BUILDING CATEGORY STYLE
    // =========================================================


    private void SetActiveBuildingCategory(Button activeButton)
    {
        productionButton.RemoveThemeStyleboxOverride("normal");
        defenseButton.RemoveThemeStyleboxOverride("normal");

        StyleBoxFlat activeStyle = CreateActiveStyle();

        activeButton.AddThemeStyleboxOverride(
            "normal",
            activeStyle
        );
    }

    // =========================================================
    // ACTIVE BUTTON STYLE
    // =========================================================
    private StyleBoxFlat CreateActiveStyle()
    {
        StyleBoxFlat activeStyle = new StyleBoxFlat();

        activeStyle.BorderWidthLeft = 3;
        activeStyle.BorderWidthRight = 3;
        activeStyle.BorderWidthTop = 3;
        activeStyle.BorderWidthBottom = 3;

        activeStyle.BorderColor = Colors.Green;

        return activeStyle;
    }

    // =========================
    // RESEARCH
    // =========================
    private void OnTurretDamageResearchPressed()
    {
        researchManager.PurchaseTurretDamageResearch();

        int level =
            researchManager.TurretDamageResearchLevel;

        turretDamageResearchButton.Text =
            $"Turret Damage +50% — Level {level}";
    }
}