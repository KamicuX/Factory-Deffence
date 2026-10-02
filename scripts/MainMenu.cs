using Godot;
using System;

public partial class MainMenu : Control
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

    // Called when the "Start Game" button is pressed.
    private void OnPlayButtonPressed()
    {
        GetTree().ChangeSceneToFile("res://scenes/game.tscn");
    }
    
    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}
    
    private void OnQuitButtonPressed()
    {
        GetTree().Quit();
    }

}
