using Godot;
using System;

public class Player : KinematicBody2D
{

    enum PlayerState
    {
        Attack,
        Death,
        Init,
        Move
    }

    // Constants
    private Vector2 GRAVITY = new Vector2(0, 100);

    // Player Vars
    private PlayerState state = PlayerState.Init;
    private Vector2 velocity = Vector2.Zero;
    private Vector2 direction = Vector2.Zero;
    

    public override void _Ready()
    {
        
    }

    public override void _PhysicsProcess(float delta)
    {
        switch (state)
        {
            case PlayerState.Attack:
                break;

            case PlayerState.Death:
                break;

            case PlayerState.Init:
                state = PlayerState.Move;
                break;

            case PlayerState.Move:
                UpdateVelocityX(delta); // move right and left
                UpdateVelocityY(delta); // may need to update when jump is pressed rather than its own function
                MoveAndSlide(velocity);
                break;
        }
    }

    private void UpdateVelocityX(float delta)
    {
        float directionX = Input.GetActionStrength("ui_right") - Input.GetActionStrength("ui_left");

    }

    private void UpdateVelocityY(float delta)
    {
        
    }
}
