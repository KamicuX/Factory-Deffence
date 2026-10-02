using Godot;

public partial class Camera2D : Godot.Camera2D
{
    [Export]
    public float Speed { get; set; } = 500f;

    public override void _Ready()
    {
        // Ensure camera processing stops when the scene tree is paused
        // Use Inherit so the camera will not process while the tree is paused
        ProcessMode = ProcessModeEnum.Inherit;
    }

    public override void _Process(double delta)
    {
        Vector2 direction = Vector2.Zero;

        if (Input.IsKeyPressed(Key.W))
            direction.Y -= 1;

        if (Input.IsKeyPressed(Key.S))
            direction.Y += 1;

        if (Input.IsKeyPressed(Key.A))
            direction.X -= 1;

        if (Input.IsKeyPressed(Key.D))
            direction.X += 1;

        if (direction != Vector2.Zero)
        {
            direction = direction.Normalized();
            Position += direction * Speed * (float)delta;
        }
    }
}