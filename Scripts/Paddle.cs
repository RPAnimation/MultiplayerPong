using Godot;
using System;

public class Paddle : Area2D
{
    private const int MoveSpeed = 300;
    public float input = 0;

    // IMPORT NODES
    private Global global;


    public override void _Ready()
    {
        global = GetNode<Global>("/root/Global");
    }

    public override void _Process(float delta)
    {
        if (global.is_host)
        {
            Vector2 position = Position;
            position += new Vector2(0, input * MoveSpeed * delta);
            position.y = Mathf.Clamp(position.y, 85, GetViewportRect().Size.y - 85);
            Position = position;
        }
    }
}
