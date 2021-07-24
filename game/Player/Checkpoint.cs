using Godot;
using System;

public class Checkpoint : Area2D
{
    [Signal] public delegate void area_entered();
    [Signal] public delegate void update_checkpoint(Vector2 spawn);

    private Player player = null;

    private Vector2 SPAWNPOINT;

    public override void _Ready()
    {
        SPAWNPOINT = Position + (Vector2.Down * 5);

        player = GetNode<Player>("/root/World/Player");

        Connect("area_entered", this, "OnAreaEntered");
        Connect("update_checkpoint", player, "UpdateCheckpoint");
    }

    private void OnAreaEntered(object area)
    {
        EmitSignal("update_checkpoint", SPAWNPOINT);
    }
}
