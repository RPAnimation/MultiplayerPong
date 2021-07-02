using Godot;

public class Game : Node2D
{

    private const string wait_scene = "res://Scenes/WaitForPlayer.tscn";
    private const string disc_scene = "res://Scenes/ServerDisconnect.tscn";
    // GAME STATE
    private GameState game;


    // IMPORT NODES
    private Global global;

    private Paddle player_paddle;
    private Label player_score;
    private Label player_nick;

    private Paddle host_paddle;
    private Label host_score;
    private Label host_nick;

    private Ball ball;

    private Timer tcp_timer;



    public override void _Ready()
    {
        // MAIN VARIABLES
        global = GetNode<Global>("/root/Global");
        global.last_score = "0-0";

        game = new GameState();

        // IMPORT VARIABLES
        player_paddle = GetNode<Paddle>("Player_Paddle");
        host_paddle = GetNode<Paddle>("Host_Paddle");

        player_score = GetNode<Label>("Score/ScoreContainer/Player_Score");
        host_score = GetNode<Label>("Score/ScoreContainer/Host_Score");

        player_nick = GetNode<Label>("Nickname/Player_Nick");
        host_nick = GetNode<Label>("Nickname/Host_Nick");

        if (global.is_host)
        {
            player_nick.Text = global.p_nick_name;
            host_nick.Text = global.nick_name;
        } else
        {
            player_nick.Text = global.nick_name;
            host_nick.Text = global.p_nick_name;
        }
        

        ball = GetNode<Ball>("Ball");
        tcp_timer = GetNode<Timer>("Tick");
        tcp_timer.WaitTime = 0.015625f;
        tcp_timer.Connect("timeout", this, nameof(_Fetch_TCP_Data));

        // WAIT FOR CONN
        while (global.get_tcp_conn_status() == false);

        tcp_timer.Start();
    }
    public void _Fetch_TCP_Data()
    {
        // GET INPUT
        int opt = 0;
        if (Input.IsActionPressed("ui_down"))
        {
            opt = 1;
        }
        else if (Input.IsActionPressed("ui_up"))
        {
            opt = -1;
        }
        else
        {
            opt = 0;
        }
        if (global.is_host)
        {
            // COPY LOCAL DATA
            game.host_position = host_paddle.Position;
            game.player_position = player_paddle.Position;
            game.ball_position = ball.Position;
            game.host_score = ball.host_score;
            game.player_score = ball.player_score;

            player_score.Text = game.player_score.ToString();
            host_score.Text = game.host_score.ToString();


            // SEND DATA
            global.send_game_data(game);

            // GET 
            GameState got = null;
            while (got == null)
            {
                if (global.get_tcp_conn_status() == false)
                {
                    global.last_score = game.host_score + "-" + game.player_score;
                    GD.Print("Player disconnected");
                    tcp_timer.Stop();
                    Quit_Game();
                    return;
                }
                got = global.get_game_data();
            }
            
            // UPDATE INPUT
            host_paddle.input = opt;
            player_paddle.input = got.player_input;
        }
        else
        {
            // GET 
            GameState got = null;
            while (got == null)
            {
                if (global.get_tcp_conn_status() == false)
                {
                    global.last_score = game.host_score + "-" + game.player_score;
                    GD.Print("Player disconnected");
                    tcp_timer.Stop();
                    Quit_Game();
                    return;
                }
                got = global.get_game_data();
            }

            game = got;
            // COPY REMOTE DATA
            host_paddle.Position = game.host_position;
            player_paddle.Position = game.player_position;
            ball.Position = game.ball_position;
            player_score.Text = game.player_score.ToString();
            host_score.Text = game.host_score.ToString();

            game.player_input = opt;
            // SEND DATA
            global.send_game_data(game);
        }
    }
    public override void _Notification(int what)
    {
        if (what == MainLoop.NotificationWmQuitRequest)
        {
            if (global.is_host)
            {
                global.stop_host_broadcast();
            }
            global.stop_player_conn();
            global.GetTree().Quit();
        }
    }
    public void Quit_Game()
    {
        global.stop_player_conn();
        if (global.is_host)
        {
            global.go_to_scene(wait_scene);
        } else
        {
            global.go_to_scene(disc_scene);
        }
    }
}
