using Godot;

public class GameState
{
    // GAME VARIABLES
    public Vector2 host_position;
    public int host_score;

    public Vector2 player_position;
    public int player_score;
    public int player_input;

    public Vector2 ball_position;

    public bool is_paused;

    public GameState()
    {
        // DEFAULT GAME VALUES
        host_position = new Vector2(0, 0);
        host_score = 0;

        player_position = new Vector2(0, 0);
        player_score = 0;
        player_input = 0;

        ball_position = new Vector2(0, 0);

        is_paused = false;
    }
    public GameState(string s)
    {
        string[] got = s.Split('|');
        this.host_position.x =   got[0].ToFloat();
        this.host_position.y =   got[1].ToFloat();
        this.host_score =        got[2].ToInt();
        this.player_position.x = got[3].ToFloat();
        this.player_position.y = got[4].ToFloat();
        this.player_score =      got[5].ToInt();
        this.player_input =      got[6].ToInt();
        this.ball_position.x =   got[7].ToFloat();
        this.ball_position.y =   got[8].ToFloat();
        this.is_paused =         got[9].ToInt() != 0 ? true : false;
    }
    public override string ToString()
    {
        string ret = "";
        ret += host_position.x.ToString() + " | ";
        ret += host_position.y.ToString() + " | ";
        ret += host_score.ToString() + " | ";
        ret += player_position.x.ToString() + " | ";
        ret += player_position.y.ToString() + " | ";
        ret += player_score.ToString() + " | ";
        ret += player_input.ToString() + " | ";
        ret += ball_position.x.ToString() + " | ";
        ret += ball_position.y.ToString() + " | ";
        ret += is_paused ? 1 : 0;
        return ret;
    }
}