using Godot;

public partial class ResearchManager : Node
{
    // =========================
    // TURRET DAMAGE RESEARCH
    // =========================

    private int turretDamageResearchLevel = 0;

    private const float TurretDamageBonusPerLevel = 0.5f;


    // =========================
    // PUBLIC ACCESS
    // =========================

    public int TurretDamageResearchLevel =>
        turretDamageResearchLevel;


    // =========================
    // PURCHASE RESEARCH
    // =========================

    public bool PurchaseTurretDamageResearch()
    {
        turretDamageResearchLevel++;

        GD.Print(
            $"Turret Damage Research Level: " +
            $"{turretDamageResearchLevel}"
        );

        ApplyTurretDamageResearch();

        return true;
    }


    // =========================
    // APPLY RESEARCH
    // =========================

    private void ApplyTurretDamageResearch()
    {
        float multiplier =
            GetTurretDamageMultiplier();

        foreach (
            Node node
            in GetTree().GetNodesInGroup("turrets")
        )
        {
            if (node is Turret turret)
            {
                turret.SetDamageMultiplier(
                    multiplier
                );
            }
        }
    }


    // =========================
    // GET MULTIPLIER
    // =========================

    public float GetTurretDamageMultiplier()
    {
        return 1.0f +
            turretDamageResearchLevel *
            TurretDamageBonusPerLevel;
    }
}