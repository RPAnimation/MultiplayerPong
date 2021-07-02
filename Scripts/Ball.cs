using Godot;
using System;

public class Ball : Area2D
{
    private const int DefaultSpeed = 250;
    public Vector2 direction = Vector2.Left;
    private Vector2 _initialPos;
    private float _speed = DefaultSpeed;

    public int player_score = 0;
    public int host_score = 0;

    // IMPORT NODES
    private Global global;

    public override void _Ready()
    {
        _initialPos = Position;
        global = GetNode<Global>("/root/Global");
    }

    public override void _Process(float delta)
    {
        if (global.is_host)
        {
            _speed += delta * 2;
            Position += _speed * delta * direction;
        }
    }
    public void Reset()
    {
        direction = Vector2.Left;
        Position = _initialPos;
        _speed = DefaultSpeed;
    }
    public void _on_Ball_area_entered(Area2D area)
    {
        if (global.is_host)
        {
            if (area.Name == "Host_Paddle")
            {
                direction = new Vector2(1, ((float)new Random().NextDouble()) * 2 - 1).Normalized();
            }
            if (area.Name == "Player_Paddle")
            {
                direction = new Vector2(-1, ((float)new Random().NextDouble()) * 2 - 1).Normalized();
            }
            if (area.Name == "Wall_1")
            {
                direction = (direction + new Vector2(0, 1)).Normalized();
            }
            if (area.Name == "Wall_2")
            {
                direction = (direction + new Vector2(0, -1)).Normalized();
            }
            if (area.Name == "Host_wall")
            {
                player_score++;
                Reset();
            }
            if (area.Name == "Join_wall")
            {
                host_score++;
                Reset();
            }
        }
    }
}
