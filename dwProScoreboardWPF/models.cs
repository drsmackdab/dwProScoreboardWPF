using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using dwProScoreboardWPF.Properties;

namespace dwProScoreboardWPF.Models
{
    public class Scoreboard
    {

        private int _maxTicks;
        private int _maxPointBreakTicks;
        private int _maxGameBreakTicks;
        private int _maxRefTicks;
        private int _ticks;
        private int _breakTicks;
        private int _refTicks;
        private int _blueScore;
        private int _redScore;
        private string _redTeamName;
        private string _blueTeamName;
        private string _data;
        private eGameState _state = eGameState.Init;


        private System.Timers.Timer gameTime;
        public enum eGameState
        {
            Init,
            Stopped,
            GameBreak,
            PointBreak,
            RefCheck,
            Running
        }
        public int maxTicks {
            get { return _maxTicks; }
            set { _maxTicks = value; onPropertyUpdate?.Invoke(this, new PropertyUpdateEventArgs("maxTicks",value));}
        }
        public int maxPointBreakTicks
        {
            get { return _maxPointBreakTicks; }
            set { _maxPointBreakTicks = value; onPropertyUpdate?.Invoke(this, new PropertyUpdateEventArgs("maxPointBreakTicks", value));}
        }
        public int maxGameBreakTicks
        {
            get { return _maxGameBreakTicks; }
            set { _maxGameBreakTicks = value; onPropertyUpdate?.Invoke(this, new PropertyUpdateEventArgs("maxGameBreakTicks", value));}
        }
        public int maxRefTicks
        {
            get { return _maxRefTicks; }
            set { _maxRefTicks = value; onPropertyUpdate?.Invoke(this, new PropertyUpdateEventArgs("maxRefTicks", value));} 
        }
        public int ticks {
            get { return _ticks; }
            set { _ticks = value; onPropertyUpdate?.Invoke(this, new PropertyUpdateEventArgs("ticks", value));}
        }
        public int breakTicks
        {
            get { return _breakTicks; }
            set { _breakTicks = value; onPropertyUpdate?.Invoke(this, new PropertyUpdateEventArgs("breakTicks", value));}
        }
        public int refTicks { 
            get { return _refTicks; }
            set { _refTicks = value; onPropertyUpdate?.Invoke(this, new PropertyUpdateEventArgs("refTicks", value));}
        }
        public int blueScore {
            get { return _blueScore; }
            set { _blueScore = value; onPropertyUpdate?.Invoke(this, new PropertyUpdateEventArgs("blueScore", value));}
        }
        public int redScore {
            get { return _redScore; }
            set { _redScore = value; onPropertyUpdate?.Invoke(this, new PropertyUpdateEventArgs("redScore", value));}
        }
        public string redTeamName { 
            get { return _redTeamName; }
            set { _redTeamName = value; onPropertyUpdate?.Invoke(this, new PropertyUpdateEventArgs("redTeamName", value));}
        }
        public string blueTeamName { 
            get { return _blueTeamName; }
            set { _blueTeamName = value; onPropertyUpdate?.Invoke(this, new PropertyUpdateEventArgs("blueTeamName", value));}
        }
        public string data { 
            get { return _data; }
            set { _data = value; onPropertyUpdate?.Invoke(this, new PropertyUpdateEventArgs("data", value));}
        }
        public eGameState state { 
            get { return _state; }
            set { _state = value; onPropertyUpdate?.Invoke(this, new PropertyUpdateEventArgs("state", value));}
        }
        private void gametime_tick(object sender, System.Timers.ElapsedEventArgs e)
        {
            switch (this.state)
            {
                case eGameState.Running:
                    {
                        if (this.ticks > 0) { this.ticks -= 1; }
                        else
                        {
                            // buzzer
                            // game time expired
                            // reset game
                        }
                        break;
                    }
                case eGameState.GameBreak:
                    {
                        if (this.breakTicks > 0){this.breakTicks -= 1;}
                        else
                        {
                            // game break clock expired
                            // start game clock (set game state running)
                            // reset break clock
                        }
                        break;
                    }
                case eGameState.PointBreak:
                    {
                        if (this.breakTicks > 0) { this.breakTicks -= 1; }
                        else
                        {
                            // game break clock expired
                            // start game clock (set game state running)
                            // reset break clock
                        }
                        break;
                    }
                case eGameState.RefCheck:
                    {
                        if (this.refTicks > 0){this.refTicks -= 1;}
                        else
                        {
                            // ref check clock expired
                            // start break clock (set game state break)
                            // reset ref check clock
                        }
                        break;
                    }
            }
        }
        public Scoreboard()
        {
            initThis();
        }
        private void initThis()
        {
            this.maxTicks = Settings.Default.gameTicks;
            this.maxGameBreakTicks = Settings.Default.gameBreakTicks;
            this.maxPointBreakTicks = Settings.Default.pointBreakTicks;
            this.maxRefTicks = Settings.Default.refTicks;
            this.ticks = this.maxTicks;
            this.breakTicks = this.maxGameBreakTicks;
            this.refTicks = this.maxRefTicks;
            this.blueScore = 0;
            this.redScore = 0;
            this.blueTeamName = "Blue Team";
            this.redTeamName = "Red Team";
            this.state = eGameState.Stopped;
            this.data = "";
            this.gameTime = new System.Timers.Timer();
            this.gameTime.Interval = 1000;
            this.gameTime.Elapsed += gametime_tick;
            this.gameTime.Start();
        }
        public void processCommand(ScoreboardCommand c)
        {
            switch (c.command.ToUpper())
            {
                case "SET.MAXTICKS":
                    this.maxTicks = (int)c.value;
                    break;
                case "SET.TICKS":
                    this.ticks = (int)c.value;
                    break;
                case "SET.BLUENAME":
                    this.blueTeamName = c.value.ToString();
                    break;
                case "SET.REDNAME":
                    this.redTeamName = c.value.ToString();
                    break;
                case "SET.BLUESCORE":
                    this.blueScore = (int)c.value;
                    break;
                case "SET.REDSCORE":
                    this.redScore = (int)c.value;
                    break;
                case "SET.STATE":
                    this.state = (Scoreboard.eGameState)c.value;
                    break;
                case "CONTROL.INCBLUE":
                    this.blueScore += 1;
                    break;
                case "CONTROL.INCRED":
                    this.redScore += 1;
                    break;
                case "CONTROL.DECBLUE":
                    if (this.blueScore - 1 >= 0) { this.blueScore -= 1; }
                    break;
                case "CONTROL.DECRED":
                    if (this.redScore - 1 >= 0) { this.redScore -= 1; }
                    break;
                case "CONTROL.START":
                    this.state = Scoreboard.eGameState.Running;
                    break;
                case "CONTROL.STOP":
                    this.state = Scoreboard.eGameState.Stopped;
                    break;
                case "CONTROL.RESET":
                    initThis();
                    break;
                case "EVENT.POINTBLUE":
                    this.blueScore += 1;
                    break;
                case "EVENT.POINTRED":
                    this.redScore += 1;
                    break;
                case "EVENT.TOWELBLUE":
                    this.redScore += 1;
                    break;
                case "EVENT.TOWELRED":
                    this.blueScore += 1;
                    break;
                case "CONTROL.CLOCKINC1":
                    if ((this.maxTicks + 1) > this.maxTicks) { this.maxTicks += 1; this.ticks += 1; }
                    break;
                case "CONTROL.CLOCKINC10":
                    if ((this.maxTicks + 10) > this.maxTicks) { this.maxTicks += 10; this.ticks += 10; }
                    break;
                case "CONTROL.CLOCKDEC1":
                    if ((this.ticks - 1) > 0) { this.ticks -= 1; } else { this.ticks = 0; }
                    break;
                case "CONTROL.CLOCKDEC10":
                    if ((this.ticks - 10) > 0) { this.ticks -= 10; } else { this.ticks = 0; }
                    break;
                case "CONTROL.BREAKCLOCKINC1":
                    //if ((this.maxBreakTicks + 1) > this.maxBreakTicks) { this.maxBreakTicks += 1; this.breakTicks += 1; }
                    break;
                case "CONTROL.BREAKCLOCKINC10":
                    //if ((this.maxBreakTicks + 10) > this.maxBreakTicks) { this.maxBreakTicks += 10; this.breakTicks += 10; }
                    break;
                case "CONTROL.BREAKCLOCKDEC1":
                    //if ((this.breakTicks - 1) > 0) { this.breakTicks -= 1; } else { this.breakTicks = 0; }
                    break;
                case "CONTROL.BREAKCLOCKDEC10":
                    //if ((this.breakTicks - 10) > 0) { this.breakTicks -= 10; } else { this.breakTicks = 0; }
                    break;
            }
        }

        // events
        public delegate void onPropertyUpdateEventHandler(object sender, PropertyUpdateEventArgs e);
        public event onPropertyUpdateEventHandler onPropertyUpdate;
    }
    public class ScoreboardCommand
    {
        public string command { get; set; }
        public object value { get; set; }
    }
    public class PropertyUpdateEventArgs
    {
        public string propertyName { get; set; }
        public object value { get; set; }
        public PropertyUpdateEventArgs(string propertyName, object value)
        {
            this.propertyName = propertyName;
            this.value = value;
        }
    }
}
