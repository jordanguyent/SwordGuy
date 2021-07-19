using Godot;
using System;

public class Arrow : Node2D
{
    public override void _PhysicsProcess(float delta)
    {
        LookAt(GetGlobalMousePosition());
    }
}
