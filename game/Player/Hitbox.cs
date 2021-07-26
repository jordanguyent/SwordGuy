using Godot;
using System;

public class Hitbox : Area2D
{
    public override void _Ready()
    {
        
    }

    public override void _PhysicsProcess(float delta)
    {
        LookAt(GetGlobalMousePosition());
    }
}
