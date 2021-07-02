using Godot;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

public class Global : Node
{
    // CURRENT SCENE
    public string last_score = "0-0";
    private Node current_scene { get; set; }
    private const string game_scene = "res://Scenes/Game.tscn";
    // ADDRESS TO HOST/JOIN
    private string ip = "";
    private int    port = -1;
    // ADDRESS TO BROADCAST
    private int    broadcast_port = -1;
    private const string broadcast_ip = "255.255.255.255";
    // PLAYER/GAME INFO
    private string game_name = "";
    public string nick_name = "Player";
    public string p_nick_name = "Player";
    public bool   is_host = false;
    // UDP
    private PacketPeerUDP socket_udp;
    private string broadcast_data = "";
    // TCP
    private TCP_Server server_tcp;
    private StreamPeerTCP socket_tcp;
    // TICK TIMER
    private Timer broadcast_tick;
    private const float broadcast_tick_rate = 0.5f;
    public override void _Ready()
    {
        Viewport root = GetTree().Root;
        current_scene = root.GetChild(root.GetChildCount() - 1);
        socket_udp = new PacketPeerUDP();
        socket_udp.SetBroadcastEnabled(true);
        broadcast_tick = new Timer();
        broadcast_tick.Autostart = false;
        broadcast_tick.WaitTime = broadcast_tick_rate;
        broadcast_tick.Connect("timeout", this, nameof(host_broadcast_data));
        AddChild(broadcast_tick);
    }
    public void set_nick_name(string nick_name)
    {
        if (nick_name != null  || nick_name != "")
        {
            this.nick_name = nick_name;
        }
    }
    public bool start_host_broadcast(string ip, int port, int broadcast_port, string game_name)
    {
        // LOCAL VARS
        this.ip = ip;
        this.port = port;
        this.broadcast_port = broadcast_port;
        this.game_name = game_name;
        this.broadcast_data = this.game_name + " | " + this.ip + " | " + this.port;
        this.is_host = true;

        // UDP
        if (socket_udp.SetDestAddress(broadcast_ip, broadcast_port) != Error.Ok)
        {
            return false;
        }

        // TCP
        server_tcp = new TCP_Server();
        if (server_tcp.Listen((ushort)port, ip) != Error.Ok)
        {
            return false;
        }
        // START TICK TIMER
        broadcast_tick.Start();
        return true;
    }
    // ON TICK TIMER
    public void host_broadcast_data()
    {
        // UDP BROADCAST
        if (socket_udp.PutPacket(broadcast_data.ToAscii()) == Error.Ok)
        {
            GD.Print("Broadcasting");
        }
        // TCP ACCEPT
        if (server_tcp.IsConnectionAvailable())
        {
            StreamPeerTCP a = server_tcp.TakeConnection();
            if (a != null)
            {
                if (socket_tcp == null)
                {
                    // AVAILABLE
                    a.Put16(1);
                    // WAIT
                    int len = 0;
                    while ((len = a.GetAvailableBytes()) <= 0) ;
                    // GET NICK
                    string got = a.GetString(len);
                    if (got == nick_name)
                    {
                        a.PutData("a".ToAscii());
                        return;
                    }
                    p_nick_name = got;
                    // NICKNAME GOOD
                    a.PutData(nick_name.ToAscii());

                    socket_tcp = a;
                    go_to_scene(game_scene);
                }
                else
                {
                    // NOT AVAILABLE
                    a.Put16(0);
                    a.DisconnectFromHost();
                }
            }
        }
        
    }
    public void stop_host_broadcast()
    {
        broadcast_tick.Stop();
        socket_udp.Close();
        server_tcp.Stop();
    }
    public bool start_player_search(int broadcast_port)
    {
        // LOCAL VARS
        this.broadcast_port = broadcast_port;


        // UDP
        if (socket_udp.Listen(this.broadcast_port) != Error.Ok)
        {
            return false;
        }

        return true;
    }
    public void search_hosts(ItemList server_list)
    {
        if (socket_udp.GetAvailablePacketCount() > 0)
        {
            GD.Print("Found!");
            string packet = socket_udp.GetPacket().GetStringFromASCII();
            bool not_in_list = true;
            for (int i = 0; i < server_list.GetItemCount(); i++)
            {
                if (server_list.GetItemText(i).Contains(packet) == true)
                {
                    not_in_list = false;
                }
            }
            if (not_in_list)
            {
                server_list.AddItem(packet);
            }
        }
    }
    public void stop_player_search()
    {
        socket_udp.Close();
    }
    public int start_player_conn(string game_name, string ip, int port)
    {
        // LOCAL VARS
        this.game_name = game_name;
        this.ip = ip;
        this.port = port;
        this.is_host = false;

        // TCP
        socket_tcp = new StreamPeerTCP();
        if (socket_tcp.ConnectToHost(this.ip, (ushort)this.port) == Error.Ok)
        {
            // WAIT FOR SERVER
            while (socket_tcp.GetAvailableBytes() <= 0) ;

            // IF SERVER BUSY
            short resp = socket_tcp.Get16();
            if (resp != 1)
            {
                return 2;
            }
            // SEND NICKNAME
            socket_tcp.PutData(nick_name.ToAscii());
            // WAIT FOR SERVER
            int len = 0;
            while ((len = socket_tcp.GetAvailableBytes()) <= 0) ;
            // GET RESPONSE
            p_nick_name = socket_tcp.GetString(len);
            if (p_nick_name == "a")
            {
                return 0;
            }

            return 1;
        }
        return 2;
    }
    // ============ TCP FOR BOTH HOST & PLAYER ==============
    public bool get_tcp_conn_status()
    {
        return socket_tcp.GetStatus() == StreamPeerTCP.Status.Connected ? true : false;
    }
    public void send_game_data(GameState gs)
    {
        socket_tcp.PutData(gs.ToString().ToAscii());
    }
    public GameState get_game_data()
    {
        int len = socket_tcp.GetAvailableBytes();
        if (len > 0)
        {
            string got = socket_tcp.GetString(len);
            return new GameState(got);
        }
        return null;
    }
    // ======================================================
    public void stop_player_conn()
    {
        socket_tcp.DisconnectFromHost();
        socket_tcp = null;
    }
    public string get_host_data()
    {
        return broadcast_data + "\nBroadcast port: " + broadcast_port;
    }

    public void go_to_scene(string path)
    {
        CallDeferred(nameof(deferred_go_to_scene), path);
    }
    public void deferred_go_to_scene(string path)
    {
        current_scene.Free();
        var nextScene = (PackedScene)GD.Load(path);
        current_scene = nextScene.Instance();
        GetTree().Root.AddChild(current_scene);
        GetTree().CurrentScene = current_scene;
    }
}