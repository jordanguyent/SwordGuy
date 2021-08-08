using Godot;
using System;

public class Ball : KinematicBody2D
{
    [Signal] public delegate void area_entered();

    enum BallState
    {
        Active,
        Move
    }

    private BallState state = BallState.Active;
    private Vector2 dir = new Vector2();
    private Vector2 velocity = new Vector2();
    private int maxSpeed = 400;

    private Area2D hitBox = null;

    public override void _Ready()
    {
        hitBox = GetNode<Area2D>("Hitbox");

        hitBox.Connect("area_entered", this, "OnAreaEntered");
    }

    public override void _PhysicsProcess(float delta)
    {
        switch(state)
        {
            case BallState.Active:
                break;

            case BallState.Move:
                velocity = dir * maxSpeed;
                var collision = MoveAndCollide(velocity * delta);

                if (collision != null)
                {
                    QueueFree();
                }
                break;
        }
    }

    private Vector2 GetDirectionToMouse()
    {
        Vector2 result = GetGlobalMousePosition() - Position;
        return result.Normalized();
    }

    private void OnAreaEntered(Area2D area)
    {
        state = BallState.Move;
        dir = GetDirectionToMouse();
    }
}
