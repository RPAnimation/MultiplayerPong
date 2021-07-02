using Godot;
using System;
using System.Text.RegularExpressions;

public class ModeChoice : Control
{
    // EXPORTS
    [Export(PropertyHint.File, "*.tscn")]
    public string host_scene;
    [Export(PropertyHint.File, "*.tscn")]
    public string join_scene;

    // IMPORT NODES
    private Global global;
    private LineEdit nickname;
    private Label warning;
    private Button host;
    private Button join;

    // STRING PROCESSING
    private Regex regex_alfa = new Regex("^[a-zA-Z0-9 ]*$");
    private string old_nickname = "";
    public override void _Ready()
    {
        global = GetNode<Global>("/root/Global");
        nickname = GetNode<LineEdit>("MarginContainer/VBoxContainer/ControlContainer/NicknameContainer/Nickname");
        nickname.Text = "Player";
        warning = GetNode<Label>("MarginContainer/VBoxContainer/ControlContainer/Warning");
        warning.Visible = false;

        host = GetNode<Button>("MarginContainer/VBoxContainer/ControlContainer/ButtonContainer/Host");
        join = GetNode<Button>("MarginContainer/VBoxContainer/ControlContainer/ButtonContainer/Join");
    }
    public void _on_Host_pressed()
    {
        global.set_nick_name(nickname.Text);
        global.go_to_scene(host_scene);
    }
    public void _on_Join_pressed()
    {

        global.set_nick_name(nickname.Text);
        global.go_to_scene(join_scene);
    }
    public void _on_Nickname_text_changed(string new_text)
    {
        Match match = regex_alfa.Match(new_text);
        if (match.Success)
        {
            old_nickname = nickname.Text;
            nickname.Text = new_text;
        }
        else
        {
            nickname.Text = old_nickname;
        }
        nickname.CaretPosition = nickname.Text.Length;
        if (nickname.Text.Length < 1)
        {
            warning.Visible = true;
            join.Disabled = true;
            host.Disabled = true;
        } else
        {
            warning.Visible = false;
            join.Disabled = false;
            host.Disabled = false;
        }
    }
}
