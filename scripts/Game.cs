using Godot;
using System;

public partial class Game : Node2D
{
    private Control pauseMenu;
    public override void _Ready()
    {
        // PauseMenu is now a child of the UI CanvasLayer
        pauseMenu = GetNode<Control>("UI/PauseMenu");
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed("ui_cancel"))
        {
            TogglePause();
        }
    }

    private void TogglePause()
    {
        bool isPaused = !GetTree().Paused;

        GetTree().Paused = isPaused;
        pauseMenu.Visible = isPaused;
    }

    public override void _Process(double delta)
    {
    }
}