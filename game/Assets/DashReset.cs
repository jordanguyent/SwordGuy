using Godot;
using System;

public class DashReset : Area2D
{
    enum State
    {
        Active,
        Cooldown,
        Init
    }

    [Signal] public delegate void area_entered();

    // Constants
    private const int CDFRAMES = 180;

    // Vars
    private State state = State.Init;
    private Player player = null;
    private ColorRect clrRect = null;
    private int cdFrame = 0;

    public override void _Ready()
    {
        player = GetNode<Player>("/root/World/Player");
        clrRect = GetNode<ColorRect>("ColorRect");

        Connect("area_entered", player, "ResetDash");   
        Connect("area_entered", this, "OnAreaEntered");
    }

    public override void _PhysicsProcess(float delta)
    {
        switch (state)
        {
            case State.Active:
                SetCollisionMaskBit(1, true);
                clrRect.Modulate = new Color(0, 1, 0, 1);
                break;

            case State.Cooldown:
                SetCollisionMaskBit(1, false);
                clrRect.Modulate = new Color(1, 0, 0, 1);
                cdFrame++;
                if (cdFrame >= CDFRAMES)
                {
                    state = State.Active;
                    cdFrame = 0;
                }
                break;

            case State.Init:
                state = State.Active;
                break;
        }
    }

    private void OnAreaEntered(object area)
    {
        state = State.Cooldown;
    }
}
