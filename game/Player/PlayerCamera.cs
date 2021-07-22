using Godot;
using System;

public class PlayerCamera : Camera2D
{
    enum CameraState
    {
        Init,
        Active
    }

    // Camera Constants
    private int CAMERAWIDTH;
    private int CAMERAHEIGHT;

    // Camera Variables
    private CameraState state = CameraState.Init;
    private Vector2 offset;
    private Vector2 target;
    private float smoothingDuration = 0.2f;
    private int loadDelay = 2;
    private int x1, x2, y1, y2;

    // Nodes
    private Node2D parent = null;

    public override void _Ready()
    {
        SetAsToplevel(true);

        parent = GetNode<Node2D>("../");

        CAMERAWIDTH = (int) GetViewport().GetVisibleRect().Size.x;
        CAMERAHEIGHT = (int) GetViewport().GetVisibleRect().Size.y;
    }

    public override void _PhysicsProcess(float delta)
    {   
        switch (state)
        {
            case CameraState.Active:
                target = parent.Position + offset;
                target = ClampViewport(target, x1, x2, y1, y2);
                ArriveTo(target, delta);
                break;

            case CameraState.Init:
                loadDelay--;

                target = parent.Position + offset;
                target = ClampViewport(target, x1, x2, y1, y2);
                Position = target;

                if (loadDelay <= 0)
                {
                    state = CameraState.Active;
                    MakeCurrent();
                }
                break;
        }
    }

    private void ArriveTo(Vector2 targetPosition, float delta)
	{
		Vector2 distance = new Vector2(targetPosition.x - Position.x, targetPosition.y - Position.y);
        distance = distance.Snapped(Vector2.One);
		Position += distance / smoothingDuration * delta; 
        if (distance == Vector2.Zero)
        {
            Position = Position.Snapped(Vector2.One);
        }
        // [ CONSIDER ] Add current position and then make current position snap and return result to position
        // Make more pixelated feel.
		// Position = Position.Snapped(Vector2.One);

		ForceUpdateScroll();
	}

    private Vector2 ClampViewport(Vector2 tg, int a1, int a2, int b1, int b2)
    {
        float x;
		float y;

		int cameraPaddingX = CAMERAWIDTH / 2;
		int cameraPaddingY = CAMERAHEIGHT / 2;

		if (tg.x - cameraPaddingX < x1)
		{
			x = a1 + cameraPaddingX;
		} 
		else if (tg.x + cameraPaddingX > x2)
		{
			x = a2 - cameraPaddingX;
		}
		else 
		{
			x = tg.x;
		}

		if (tg.y - cameraPaddingY < y1)
		{
			y = b1 + cameraPaddingY;
		}
		else if (tg.y + cameraPaddingY > y2)
		{
			y = b2 - cameraPaddingY;
		}
		else 
		{
			y = tg.y;
		}

		return new Vector2(x, y);
    }

    // Signals ================================================================================================================================================
    private void ChangeSettings(int a1, int a2, int b1, int b2, Vector2 os)
    {
        x1 = a1;
        x2 = a2;
        y1 = b1;
        y2 = b2;
        offset = os;
    }
}
