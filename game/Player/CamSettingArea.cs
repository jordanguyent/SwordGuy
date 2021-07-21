using Godot;
using System;

public class CamSettingArea : Area2D
{
    [Signal] public delegate void area_entered();
    [Signal] public delegate void camera_settings(int a1, int a2, int b1, int b2, Vector2 os);

    [Export] Vector2 offset = Vector2.Zero;
    [Export] int leftBound = 0;
    [Export] int rightBound = 256;
    [Export] int topBound = 0;
    [Export] int bottomBound = 144;

    PlayerCamera camera = null;

    public override void _Ready()
    {
        camera = GetNode<PlayerCamera>("../Player/PlayerCamera");
        
        Connect("area_entered", this, "OnAreaEntered");
        Connect("camera_settings", camera, "ChangeSettings");
    }

    private void OnAreaEntered(object area)
    {
        EmitSignal("camera_settings", leftBound, rightBound, topBound, bottomBound, offset);
    }
}
