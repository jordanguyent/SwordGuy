using Godot;
using System;

public class Button : Area2D
{
    [Signal] public delegate void area_entered();

    enum ButtonState
    {
        Unpressed,
        Pressed
    }

    private ButtonState state = ButtonState.Unpressed;

    public override void _Ready()
    {
        Connect("area_entered", this, "OnAreaEntered");
    }

    public override void _PhysicsProcess(float delta)
    {
        switch (state)
        {
            case ButtonState.Unpressed:
                break;

            case ButtonState.Pressed:
                SetCollisionMaskBit(5, false);
                break;
        }
    }

    private void OnAreaEntered()
    {
        state = ButtonState.Pressed;
    }

}
