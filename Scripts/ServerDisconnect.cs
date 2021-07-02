using Godot;
using System;

public class ServerDisconnect : Control
{
    // IMPORT NODES
    private Global global;
    private Label score;
    private const string mode_scene = "res://Scenes/ModeChoice.tscn";
    public override void _Ready()
    {
        global = GetNode<Global>("/root/Global");
        score = GetNode<Label>("MarginContainer/VBoxContainer/InfoContainer/GameInfo");
        score.Text += global.last_score;
    }
    public void _on_Ok_pressed()
    {
        global.go_to_scene(mode_scene);
    }
}
