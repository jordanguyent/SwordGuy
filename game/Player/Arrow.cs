using Godot;
using System;

public class Arrow : Node2D
{
    Player player = null;
    Sprite sprite = null;

    public override void _Ready()
    {
        player = GetNode<Player>("/root/World/Player");
        sprite = GetNode<Sprite>("Sprite");
    }

    public override void _PhysicsProcess(float delta)
    {
        LookAt(GetGlobalMousePosition());
        
        // Testing Purpose =================================
        if (player.attackCount > 0)
        {
            sprite.Modulate = new Color(1, 0, 0, 1);
        }
        else
        {
            sprite.Modulate = new Color(0, 1, 0, 1);
        }
        // =================================================
    }
}
