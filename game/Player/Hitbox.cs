using Godot;
using System;

public class Hitbox : Area2D
{
    private Player player = null;

    public override void _Ready()
    {
        player = GetNode<Player>("../");
    }

    public override void _PhysicsProcess(float delta)
    {
        LookAt(GetGlobalMousePosition());

        if (player.GetState() == Player.PlayerState.Attack)
        {
            SetCollisionLayerBit(4, true);
        }
        else 
        {
            SetCollisionLayerBit(4, false);
        }
    }
}
