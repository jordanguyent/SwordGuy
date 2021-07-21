using Godot;
using System;

public class PlayerCamera : Camera2D
{
    enum CameraState
    {
        Init,
        Active,
        Transition
    }

    // Camera Constants
    private int CAMERAWIDTH;
    private int CAMERAHEIGHT;

    // Camera Variables
    private CameraState state = CameraState.Init;
    private Vector2 offset;
    private float smoothingDuration = 0.2f;
    private int loadDelay = 1;
    private int x1, x2, y1, y2;

    // Nodes
    private Node2D parent = null;

    public override void _Ready()
    {
        SetAsToplevel(true);

        parent = GetNode<Node2D>("../");

        CAMERAWIDTH = (int) GetViewport().GetVisibleRect().Size.x;
        CAMERAHEIGHT = (int) GetViewport().GetVisibleRect().Size.y;
        MakeCurrent();
    }

    public override void _PhysicsProcess(float delta)
    {
        switch (state)
        {
            case CameraState.Active: // Standard
                ArriveTo(parent.Position + offset, delta);
                ClampViewport(0, 256, 0, 144); // Temp values
                break;

            case CameraState.Init: // On start
                loadDelay--;
                if (loadDelay <= 0)
                {
                    state = CameraState.Active;
                    Position = parent.Position + offset;
                }
                break;

            case CameraState.Transition:
                break;
        }
    }

    public void ArriveTo(Vector2 targetPosition, float delta)
	{
		Vector2 distance = new Vector2(targetPosition.x - Position.x, targetPosition.y - Position.y);
		Position += distance / smoothingDuration * delta;
		Position = Position.Snapped(Vector2.One);

		ForceUpdateScroll();
	}

    public void ClampViewport(int a1, int a2, int b1, int b2)
    {
        float x;
		float y;

		int cameraPaddingX = CAMERAWIDTH / 2;
		int cameraPaddingY = CAMERAHEIGHT / 2;

		if (Position.x - cameraPaddingX < x1)
		{
			x = a1 + cameraPaddingX;
		} 
		else if (Position.x + cameraPaddingX > x2)
		{
			x = a2 - cameraPaddingX;
		}
		else 
		{
			x = Position.x;
		}

		if (Position.y - cameraPaddingY < y1)
		{
			y = b1 + cameraPaddingY;
		}
		else if (Position.y + cameraPaddingY > y2)
		{
			y = b2 - cameraPaddingY;
		}
		else 
		{
			y = Position.y;
		}

		Position = new Vector2(x, y);
    }
}
