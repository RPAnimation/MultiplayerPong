using Godot;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class JoinGame : Control
{
    // EXPORTS
    [Export(PropertyHint.File, "*.tscn")]
    public string mode_scene;
    [Export(PropertyHint.File, "*.tscn")]
    public string game_scene;

    // INPUT DEFAULTS
    private const float default_wait_time = 0.5f;
    private const string default_port = "7777";

    // IMPORT NODES
    private Global global;
    private ItemList ui_list;
    private Timer timer;
    private LineEdit port;
    private Label warning_port;
    private Label warning_disc;
    private Label warning_nickname;
    private Button join_button;

    // STRING PROCESSING
    private Regex regex_num = new Regex("^[0-9]*$");
    private string old_port = "";

    public override void _Ready()
    {
        global = GetNode<Global>("/root/Global");
        ui_list = GetNode<ItemList>("MarginContainer/VBoxContainer/ServerList");

        port = GetNode<LineEdit>("MarginContainer/VBoxContainer/PortContainer/Port");
        port.Text = old_port = default_port;

        timer = GetNode<Timer>("Timer");
        timer.WaitTime = default_wait_time;

        warning_port = GetNode<Label>("MarginContainer/VBoxContainer/Warning_port");
        warning_port.Visible = false;
        warning_disc = GetNode<Label>("MarginContainer/VBoxContainer/Warning_disc");
        warning_disc.Visible = false;
        warning_nickname = GetNode<Label>("MarginContainer/VBoxContainer/Warning_nickname");
        warning_nickname.Visible = false;

        join_button = GetNode<Button>("MarginContainer/VBoxContainer/ButtonContainer/Join");

        global.start_player_search(port.Text.ToInt());
    }
    public void _on_Cancel_pressed()
    {
        global.stop_player_search();
        global.go_to_scene(mode_scene);
    }
    public void _on_List_refresh()
    {
        global.search_hosts(ui_list);
    }
    public void _on_Port_text_changed(string new_text)
    {
        global.stop_player_search();
        ui_list.Clear();
        join_button.Disabled = true;

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
            // VERIFY PORT
            if (port.Text.ToInt() > 65535 || port.Text.ToInt() < 0)
            {
                warning_port.Visible = true;
            }
            if (global.start_player_search(port.Text.ToInt()) == true)
            {
                warning_port.Visible = false;
            }
        } else
        {
            warning_port.Visible = true;
        }
    }
    public void _on_ServerList_item_selected(int i)
    {
        join_button.Disabled = false;

    }
    public void _on_Join_pressed()
    {
        warning_port.Visible = false;
        warning_disc.Visible = false;
        warning_nickname.Visible = false;

        if (ui_list.IsAnythingSelected())
        {
            timer.Stop();
            var items = ui_list.GetSelectedItems();
            string[] got = ui_list.GetItemText(items[0]).Split('|');

            int ret = global.start_player_conn(got[0].Trim(), got[1].Trim(), got[2].Trim().ToInt());

            if (ret == 1)
            {
                GD.Print("Connected!");
                global.stop_player_search();
                global.go_to_scene(game_scene);
            }
            else if (ret == 2)
            {
                warning_disc.Visible = true;
            } else
            {
                warning_nickname.Visible = true;
            }
            timer.Start();
        }
    }
}