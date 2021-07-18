using Godot;
using System;
using System.Diagnostics;

public class Player : KinematicBody2D
{

    enum PlayerState
    {
        Attack,
        Death,
        Init,
        Move,
        Still
    }

    // Universal Constants
	private Vector2 E1 = new Vector2(1, 0);
	private Vector2 E2 = new Vector2(0, -1);

    // Constants
    [Export] int ACCELERATION = 1000;
    [Export] int ATTACKACCELERATION = 2000;
    [Export] int ATTACKDELTAFRAMES = 5;
    [Export] int ATTACKFRAMES = 10;
    [Export] int ATTACKMAGNITUDE = 200;
    [Export] int COYOTEFRAMES = 5;
    [Export] int GRAVITY = 500;
    [Export] int INPUTBUFFERFRAMES = 5;
    [Export] int JUMPFRAMES = 8;
    [Export] int JUMPMAGNITUDE = 100;
    [Export] int LOCKFRAMES = 5;
    [Export] int SPEEDXMAX = 50;
    [Export] int SPEEDYMAX = 300;
    [Export] int STILLFRAMES = 5;
    [Export] int WALLJUMPMAGX = 140;

    // Player Vars
    private PlayerState state = PlayerState.Init;
    private Vector2 attackDirection = Vector2.Zero;
    private Vector2 direction = Vector2.Zero;
    private Vector2 velocity = Vector2.Zero;
    private float inputX = 0;
    private float wallCollisionX = 0;
    private int attackFrame = 0;
    private int coyoteFrame = 0;
    private int jumpBufferFrame = 0;
    private int jumpFrame = 0;
    private int lockFrame = 0;
    private int stillFrame = 0;
    private bool canWallJump = false;
    private bool holdingJump = false;
    private bool isAttacking = false;
    private bool isJumping = false;
    private bool isWallJumping = false;
    private bool justPressedJump = false;

    // Nodes
    AnimatedSprite playerSprite = null;
    RayCast2D wallCollisionRayR = null;
    RayCast2D wallCollisionRayL = null;


    public override void _Ready()
    {
        ErrorHandler();

        playerSprite = GetNode<AnimatedSprite>("AnimatedSprite");
        wallCollisionRayR = GetNode<RayCast2D>("WallCollisionRayR");
        wallCollisionRayL = GetNode<RayCast2D>("WallCollisionRayL");
    }

    public override void _PhysicsProcess(float delta)
    {
        switch (state)
        {
            case PlayerState.Attack:

                AttackTowardMouse(delta);
                velocity = MoveAndSlide(velocity, E2);

                attackFrame++;
                if (attackFrame >= ATTACKFRAMES)
                {
                    attackFrame = 0;
                    state = PlayerState.Still;
                }
                break;

            case PlayerState.Death:
                break;

            case PlayerState.Init:
                state = PlayerState.Move;
                break;

            case PlayerState.Move:

                // [ TODO ] Add coyote frames
                SetInputs();

                if (isAttacking)
                {
                    state = PlayerState.Attack;
                    attackDirection = GetLocalMousePosition().Normalized();
                }

                UpdateVelocityX(delta);
                UpdateVelocityY(delta);

                velocity = MoveAndSlide(velocity, E2);
                Position = Position.Snapped(Vector2.One);
                break;

            case PlayerState.Still:
                stillFrame++;
                if (stillFrame >= STILLFRAMES)
                {
                    state = PlayerState.Move;
                    stillFrame = 0;
                }
                break;
        }
    }

    private void AttackTowardMouse(float delta)
    {
        int frameDif = ATTACKFRAMES - attackFrame;    
        if (frameDif <= ATTACKDELTAFRAMES)
        {
            velocity = velocity.MoveToward(Vector2.Zero, ATTACKACCELERATION * delta);
        }
        else 
        {
            velocity = attackDirection * ATTACKMAGNITUDE;
        }
    }

    private void BufferJustPressedInput(ref bool selfBool, ref int inputBufferFrames, String keypress, bool condition)
    {
        if (Input.IsActionJustPressed(keypress))
        {
            selfBool = true;
        }

        if (selfBool && inputBufferFrames <= INPUTBUFFERFRAMES)
        {
            inputBufferFrames++;
            // [ WARNING ] Because isJumping does not reset the moment when jump, may cause issues here.
            if (condition)
            {
                inputBufferFrames = 0;
                selfBool = false;
            }
        }
        else
        {
            inputBufferFrames = 0;
            selfBool = false;
        }
    }

    private void ErrorHandler()
    {   
        Debug.Assert(ATTACKDELTAFRAMES <= ATTACKFRAMES, "ATTACKDELTAFRAMES <= ATTACKFRAMES : AttackDeltaFrames must be less than or equal to AttackFrames.");
        Debug.Assert(LOCKFRAMES <= JUMPFRAMES, "LOCKFRAMES <= JUMPFRAMES : LockFrames must be less than or equal to JumpFrames.");
    }

    private Vector2 GetCollisionDirection()
    {
        if (wallCollisionRayL.IsColliding())
        {
            return new Vector2(1, 0);
        }
        if (wallCollisionRayR.IsColliding())
        {
            return new Vector2(-1, 0);
        }
        return Vector2.Zero;
    }

    private float HelperMoveToward(float current, float desire, float acceleration)
	{
		return (E1 * current).MoveToward(E1 * desire, acceleration).x;
	}

    private void SetInputs()
    {
        inputX = Input.GetActionStrength("ui_right") - Input.GetActionStrength("ui_left");
        isAttacking = Input.IsActionJustPressed("ui_attack");
        holdingJump = Input.IsActionPressed("ui_jump");
        BufferJustPressedInput(ref justPressedJump, ref jumpBufferFrame, "ui_jump", isJumping);
    }

    private void UpdateVelocityX(float delta)
    {
        velocity.x = HelperMoveToward(velocity.x, inputX * SPEEDXMAX, ACCELERATION * delta);
    }

    private void UpdateVelocityY(float delta)
    {
        // [ TODO ] Make where the conditions that determine if you can do stuff like isJumping and canWallJump into a separate function
        // Need to add a lock to the jump from teh wall. 

        // Gravity
        velocity.y = HelperMoveToward(velocity.y, SPEEDYMAX, GRAVITY * delta);

        // Jump
        if (IsOnFloor() && justPressedJump)
        {
            isJumping = true;
        }

        // Wall Jump
        // [ CONSIDER ] Make it timed walled jumps that jump at the apex of jump.
        // [ WARNING ] At the moment, anything can be a wall. May need to make a seperate function to check for specific collision that returns bool.
        // [ WARNING ] May have an issue when it comes to wall jumping between walls.
        if (!IsOnFloor() && (wallCollisionRayR.IsColliding() || wallCollisionRayL.IsColliding()))
        {
            canWallJump = true;
        }
        else
        {
            canWallJump = false;
        }

        if (canWallJump && justPressedJump)
        {
            isJumping = true;
            isWallJumping = true;
            wallCollisionX = GetCollisionDirection().x;
        }

        if (isJumping)
        {
            if (isWallJumping)
            {
                velocity.x = WALLJUMPMAGX * wallCollisionX;              
                velocity.y = -JUMPMAGNITUDE;
                if (isWallJumping)
                {
                    lockFrame++;
                    if (lockFrame >= LOCKFRAMES)
                    {
                        lockFrame = 0;
                        isWallJumping = false;
                        isJumping = false;
                    }
                }   
            }
            else
            {
                if (holdingJump)
                {
                    velocity.y = -JUMPMAGNITUDE;
                    jumpFrame++;
                    if (jumpFrame >= JUMPFRAMES)
                    {
                        isJumping = false;
                        jumpFrame = 0;
                    }
                }
                else 
                {
                    isJumping = false;
                    jumpFrame = 0;
                }
            }
        }
    }
}
