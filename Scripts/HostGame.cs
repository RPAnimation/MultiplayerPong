using Godot;
using System;
using System.Text.RegularExpressions;

public class HostGame : Control
{
    // EXPORTS
    [Export(PropertyHint.File, "*.tscn")]
    public string mode_scene;
    [Export(PropertyHint.File, "*.tscn")]
    public string wait_scene;

    // IMPORT NODES
    private Global global;
    private LineEdit port;
    private LineEdit broadcast_port;
    private LineEdit name;
    private LineEdit ip;
    private Label warning;
    private Button host_button;

    // INPUT DEFAULTS
    private const string default_name = "Game";
    private const string default_port = "8080";
    private const string default_broadcast_port = "7777";

    // STRING PROCESSING
    private Regex regex_num = new Regex("^[0-9]*$");
    private Regex regex_alfa = new Regex("^[a-zA-Z0-9 ]*$");
    // BACKUP FIELD STRINGS
    private string old_port = "";
    private string old_broadcast_port = "";
    private string old_name = "";


    public override void _Ready() {
        global = GetNode<Global>("/root/Global");
        global.last_score = "0-0";

        ip = GetNode<LineEdit>("MarginContainer/VBoxContainer/OptionContainer/AddressContainer/IP");
        var adresses = IP.GetLocalAddresses();
        ip.Text = adresses[adresses.Count - 1].ToString();

        port = GetNode<LineEdit>("MarginContainer/VBoxContainer/OptionContainer/PortContainer/Port");
        RandomNumberGenerator rng = new RandomNumberGenerator();
        rng.Randomize();
        port.Text = old_port = rng.RandiRange(0, 65535).ToString();

        broadcast_port = GetNode<LineEdit>("MarginContainer/VBoxContainer/OptionContainer/BroadcastPortContainer/BroadcastPort");
        broadcast_port.Text = old_broadcast_port = default_broadcast_port;

        name = GetNode<LineEdit>("MarginContainer/VBoxContainer/OptionContainer/NameContainer/Name");
        name.Text = old_name = default_name;
        
        warning = GetNode<Label>("MarginContainer/VBoxContainer/OptionContainer/Warning");
        warning.Visible = false;

        host_button = GetNode<Button>("MarginContainer/VBoxContainer/ButtonContainer/Host");
    }
    public void _on_Cancel_pressed()
    {
        global.go_to_scene(mode_scene);
    }
    public void _on_Host_pressed()
    {
        // NO BROADCAST PORT GIVEN
        if (broadcast_port.Text.Length <= 0)
        {
            broadcast_port.Text = default_broadcast_port;
        }
        // NO PORT GIVEN
        if (port.Text.Length <= 0)
        {
            port.Text = default_port;
        }
        // NO NAME GIVEN
        if (name.Text.Length <= 0)
        {
            name.Text = default_name;
        }
        // START BROADCAST, SWITCH SCENE
        global.start_host_broadcast(ip.Text, port.Text.ToInt(), broadcast_port.Text.ToInt(), name.Text);
        global.go_to_scene(wait_scene);
    }
    public void _on_Port_text_changed(string new_text)
    {
        
        Match match = regex_num.Match(new_text);
        if (match.Success)
        {
            old_port = port.Text;
            port.Text = new_text;
        }
        else
        {
            port.Text = old_port;
        }
        port.CaretPosition = port.Text.Length;
        if (port.Text.Length > 0)
        {
            if (port.Text.ToInt() > 65535 || port.Text.ToInt() < 0)
            {
                warning.Visible = true;
                host_button.Disabled = true;
            }
            else
            {
                warning.Visible = false;
                host_button.Disabled = false;
            }
        }
    }
    public void _on_BroadcastPort_text_changed(string new_text)
    {

        Match match = regex_num.Match(new_text);
        if (match.Success)
        {
            old_broadcast_port = broadcast_port.Text;
            broadcast_port.Text = new_text;
        }
        else
        {
            broadcast_port.Text = old_broadcast_port;
        }
        broadcast_port.CaretPosition = broadcast_port.Text.Length;
        if (broadcast_port.Text.Length > 0)
        {
            if (broadcast_port.Text.ToInt() > 65535 || broadcast_port.Text.ToInt() < 0)
            {
                warning.Visible = true;
                host_button.Disabled = true;
            }
            else
            {
                warning.Visible = false;
                host_button.Disabled = false;
            }
        }
    }
    public void _on_Name_text_changed(string new_text)
    {
        Match match = regex_alfa.Match(new_text);
        if (match.Success)
        {
            old_name = name.Text;
            name.Text = new_text;
        }
        else
        {
            name.Text = old_name;
        }
        name.CaretPosition = name.Text.Length;
    }
}