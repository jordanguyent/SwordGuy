using Godot;
using System;
using System.Diagnostics;

public class Player : KinematicBody2D
{
    // [ TODO ] Add attack animation
    // [ TODO ] Disappearing blocks
    // [ TODO ] Buttons and doors
    // [ TODO ] Breakable blocks
    // [ TODO ] Blocks attached to a rope. Cut rope makes block fall. 
    // [ TODO ] Deflect projectiles to the opposite direction

    // [ CONSIDER ] Add pause frames on attack hit

    [Signal] public delegate void attack_pressed();

    public enum PlayerState
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
    [Export] int ATTACKDELTAFRAMES = 7;
    [Export] int ATTACKFRAMES = 12;
    [Export] int ATTACKMAGNITUDE = 200;
    [Export] int ATTACKTOTAL = 1;
    [Export] int COYOTEFRAMES = 3;
    [Export] int GRAVITY = 500;
    [Export] int INPUTBUFFERFRAMES = 5;
    [Export] int JUMPFRAMES = 8;
    [Export] int JUMPMAGNITUDE = 100;
    [Export] int LOCKFRAMES = 5;
    [Export] int SPEEDXMAX = 50;
    [Export] int SPEEDYMAX = 200;
    [Export] int STILLFRAMES = 5;
    [Export] int WALLJUMPMAGX = 140;

    // Player Vars
    private PlayerState state = PlayerState.Init;
    private Vector2 attackDirection = Vector2.Zero;
    private Vector2 direction = Vector2.Zero;
    private Vector2 spawnPoint = Vector2.Zero;
    private Vector2 velocity = Vector2.Zero;
    private float previousLeftRightInput = 0;
    private float recentLeftRightInput = 0;
    private float wallCollisionX = 0;
    public int attackCount = 0; // Temp public
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
    private bool justPressedLeft = false;
    private bool justPressedRight = false;
    private bool justReleasedLeft = false;
    private bool justReleasedRight = false;
    private bool pressedLeft = false;
    private bool pressedRight = false;

    // Nodes
    AnimatedSprite playerSprite = null;
    Area2D attackBox = null;
    RayCast2D floorCollisionRayR = null;
    RayCast2D floorCollisionRayL = null;
    RayCast2D wallCollisionRayR = null;
    RayCast2D wallCollisionRayL = null;
    
    


    public override void _Ready()
    {
        ErrorHandler();

        playerSprite = GetNode<AnimatedSprite>("AnimatedSprite");
        attackBox = GetNode<Area2D>("Hitbox");
        floorCollisionRayR = GetNode<RayCast2D>("FloorCollisionRayR");
        floorCollisionRayL = GetNode<RayCast2D>("FloorCollisionRayL");
        wallCollisionRayR = GetNode<RayCast2D>("WallCollisionRayR");
        wallCollisionRayL = GetNode<RayCast2D>("WallCollisionRayL");
    }

    public override void _PhysicsProcess(float delta)
    {
        // Actions
        // Animation
        // Transition

        switch (state)
        {
            case PlayerState.Attack:
                SetInputs();
                HandleEffectCollisions();
                AttackTowardMouse(delta);
                velocity = MoveAndSlide(velocity, E2);

                attackFrame++;
                if (attackFrame >= ATTACKFRAMES)
                {
                    jumpFrame = 0;
                    isJumping = false;
                    attackFrame = 0;
                    state = PlayerState.Still;
                }
                break;

            case PlayerState.Death:
                // [ CONSIDER ] create a transition for death and respawn
                Position = spawnPoint;

                Reset();
                break;

            case PlayerState.Init:
                state = PlayerState.Move;
                break;

            case PlayerState.Move:
                SetInputs();
                HandleCoyoteFrames();
                HandleEffectCollisions();
                UpdateVelocityX(delta);
                UpdateVelocityY(delta);

                velocity = MoveAndSlide(velocity, E2);
                Position = Position.Snapped(Vector2.One);

                if (isAttacking && attackCount < ATTACKTOTAL)
                {
                    state = PlayerState.Attack;

                    // Update attack vars
                    attackCount++;
                    attackDirection = GetLocalMousePosition().Normalized();
                }
                else if (RayIsOnFloor())
                {
                    attackCount = 0;
                }
                break;

            case PlayerState.Still:
                SetInputs();
                CheckIfCanJump();
                CheckIfCanWallJump();

                stillFrame++;
                if (stillFrame >= STILLFRAMES || isJumping)
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
        if (frameDif < ATTACKDELTAFRAMES)
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

        if (selfBool && inputBufferFrames < INPUTBUFFERFRAMES)
        {
            inputBufferFrames++;
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

    private void CheckIfCanJump()
    {
        // Jump
        if ((RayIsOnFloor() || coyoteFrame < COYOTEFRAMES) && justPressedJump)
        {
            isJumping = true;
            coyoteFrame = COYOTEFRAMES;
        }
    }

    private void CheckIfCanWallJump()
    {
        // Wall Jump
        // [ WARNING ] At the moment, anything can be a wall. May need to make a seperate function to check for specific collision that returns bool.
        // [ WARNING ] May have an issue when it comes to wall jumping between walls.
        if (!RayIsOnFloor() && RayIsOnWall())
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

    public PlayerState GetState()
    {
        return state;
    }

    private void HandleCoyoteFrames()
    {
        if (coyoteFrame < COYOTEFRAMES)
        {
            coyoteFrame++;
        }

        if (RayIsOnFloor())
        {
            coyoteFrame = 0;
        }

        if (isAttacking)
        {
            coyoteFrame = COYOTEFRAMES;
        }
    }

    private void HandleEffectCollisions()
    {
        int collisionCount = GetSlideCount();

        for(int i = 0; i < collisionCount; i++)
        {
            KinematicCollision2D currentCollision = GetSlideCollision(i);
            Godot.Object collidedWith = currentCollision.Collider;
            if ((collidedWith as DangerTMs) != null)
            {
                state = PlayerState.Death;
            }
        }
    }

    private void HandleJumpAndWallJump()
    {
        if (isJumping)
        {
            // Wall Jump
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
            // Jump
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
        else
        {
            jumpFrame = 0;
            lockFrame = 0;
            isWallJumping = false;
        }
    }

    private float HelperMoveToward(float current, float desire, float acceleration)
	{
		return (E1 * current).MoveToward(E1 * desire, acceleration).x;
	}

    private bool RayIsOnFloor()
    {
        return floorCollisionRayR.IsColliding() || floorCollisionRayL.IsColliding();
    }

    private bool RayIsOnWall()
    {
        return wallCollisionRayR.IsColliding() || wallCollisionRayL.IsColliding();
    }

    private void Reset()
    {
        GD.Print("Reset");
        state = PlayerState.Init;
        attackDirection = Vector2.Zero;
        direction = Vector2.Zero;
        velocity = Vector2.Zero;
        // [ WARNING ] This makes it feel a little weird when respawning, cannot move right away. May need to fix.
        recentLeftRightInput = 0; 
        wallCollisionX = 0;
        attackCount = 0;
        attackFrame = 0;
        coyoteFrame = 0;
        jumpBufferFrame = 0;
        jumpFrame = 0;
        lockFrame = 0;
        stillFrame = 0;
        canWallJump = false;
        holdingJump = false;
        isAttacking = false; 
        isJumping = false;
        isWallJumping = false;
        justPressedJump = false;
    }

    private void SetInputs()
    {
        isAttacking = Input.IsActionJustPressed("ui_attack");
        holdingJump = Input.IsActionPressed("ui_jump");
        justPressedLeft = Input.IsActionJustPressed("ui_left");
        justPressedRight = Input.IsActionJustPressed("ui_right");
        justReleasedLeft = Input.IsActionJustReleased("ui_left");
        justReleasedRight = Input.IsActionJustReleased("ui_right");
        pressedLeft = Input.IsActionPressed("ui_left");
        pressedRight = Input.IsActionPressed("ui_right");
        
        if (justPressedLeft)
        {
            previousLeftRightInput = recentLeftRightInput;
            recentLeftRightInput = -1;
        }
        else if (justPressedRight)
        {
            previousLeftRightInput = recentLeftRightInput;
            recentLeftRightInput = 1;
        }

        if (justReleasedLeft)
        {
            if (recentLeftRightInput == -1 && pressedRight)
            {
                recentLeftRightInput = previousLeftRightInput;
            }
        }
        else if (justReleasedRight)
        {
            if (recentLeftRightInput == 1 && pressedLeft)
            {
                recentLeftRightInput = previousLeftRightInput;
            }
        }

        if (!pressedLeft && !pressedRight)
        {
            previousLeftRightInput = recentLeftRightInput;
            recentLeftRightInput = 0;
        }
        
        BufferJustPressedInput(ref justPressedJump, ref jumpBufferFrame, "ui_jump", isJumping);
    }

    private void UpdateVelocityX(float delta)
    {
        // last action takes priority
        
        velocity.x = HelperMoveToward(velocity.x, recentLeftRightInput * SPEEDXMAX, ACCELERATION * delta);
    }

    private void UpdateVelocityY(float delta)
    {
        // [ CONSIDER ] Make where the conditions that determine if you can do stuff like isJumping and canWallJump into a separate function
        
        // Gravity
        if (!RayIsOnFloor() && coyoteFrame < COYOTEFRAMES)
        {
            velocity.y = 0;
        }
        else
        {
            velocity.y = HelperMoveToward(velocity.y, SPEEDYMAX, GRAVITY * delta);
        }

        // Handles jumping and wall jumping
        CheckIfCanJump();
        CheckIfCanWallJump();
        HandleJumpAndWallJump();
    }

    // Signals ================================================================================================================================================
    private void ResetDash(object area)
    {
        attackCount = 0;
    }

    private void UpdateCheckpoint(Vector2 pos)
    {
        spawnPoint = pos;
        GD.Print("Checkpoint Updated : " + spawnPoint);
    }
}
