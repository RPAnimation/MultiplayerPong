using Godot;
using System;

public class WaitForPlayer : Control
{
    // EXPORTS
    [Export(PropertyHint.File, "*.tscn")]
    public string mode_scene;
    [Export(PropertyHint.File, "*.tscn")]
    public string game_scene;

    // INPUT DEFAULTS
    private const string default_title = "Waiting for players to join";
    private const float default_wait_time = 0.5f;


    // IMPORT NODES
    private Global global;
    private Label info;
    private Label wait_title;
    private Timer timer;

    public override void _Ready()
    {
        global = GetNode<Global>("/root/Global");

        info = GetNode<Label>("MarginContainer/VBoxContainer/InfoContainer/GameInfo");
        info.Text = global.get_host_data();
        if (global.last_score != "0-0")
        {
            info.Text += "\n" + "Last score: " + global.last_score;
        }

        wait_title = GetNode<Label>("MarginContainer/VBoxContainer/TitleContainer/Subtitle");
        wait_title.Text = default_title;

        timer = GetNode<Timer>("Timer");
        timer.WaitTime = default_wait_time;
        timer.Connect("timeout", this, nameof(_on_Broadcast_tick));
    }
    public void _on_Broadcast_tick()
    {
        if (wait_title.Text.Length == default_title.Length + 3)
        {
            wait_title.Text = wait_title.Text.Substring(0, default_title.Length);
        } else
        {
            wait_title.Text += ".";
        }
    }
    public void _on_Cancel_pressed()
    {
        global.stop_host_broadcast();
        global.go_to_scene(mode_scene);
    }
}
