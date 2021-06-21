using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using dwProScoreboardWPF.Models;
using dwProScoreboardWPF.Properties;
using dwPostmaster;
using Newtonsoft.Json;
using System.Windows.Threading;
using System.Media;
using System.Text.RegularExpressions;
using System.IO.Ports;


namespace dwProScoreboardWPF
{
    public partial class MainWindow : Window
    {
        private SerialPort serial = new SerialPort(Settings.Default.serialPort);
        private static readonly Regex _regex = new Regex("[^0-9.-]+"); // for numeric only textboxes
        private SoundPlayer sound { get; set; }
        private Scoreboard theScoreboard { get; set; }
        private string assemblyPath { get { return System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).Replace("file:\\", ""); } }
        public void soundEvent(string eventKey, bool sync = false)
        {
            switch (eventKey.ToLower())
            {
                case "5m":
                    {
                        sound.SoundLocation = string.Format(@"{0}\sfx\5min.wav", assemblyPath);
                        break;
                    }
                case "2m":
                    {
                        sound.SoundLocation = string.Format(@"{0}\sfx\2min.wav", assemblyPath);
                        break;
                    }
                case "1m":
                    {
                        sound.SoundLocation = string.Format(@"{0}\sfx\1min.wav", assemblyPath);
                        break;
                    }
                case "30s":
                    {
                        sound.SoundLocation = string.Format(@"{0}\sfx\30sec.wav", assemblyPath);
                        break;
                    }
                case "20s":
                    {
                        sound.SoundLocation = string.Format(@"{0}\sfx\20sec.wav", assemblyPath);
                        break;
                    }
                case "10s":
                    {
                        sound.SoundLocation = string.Format(@"{0}\sfx\ten.wav", assemblyPath);
                        break;
                    }
                case "9s":
                    {
                        sound.SoundLocation = string.Format(@"{0}\sfx\nine.wav", assemblyPath);
                        break;
                    }
                case "8s":
                    {
                        sound.SoundLocation = string.Format(@"{0}\sfx\eight.wav", assemblyPath);
                        break;
                    }
                case "7s":
                    {
                        sound.SoundLocation = string.Format(@"{0}\sfx\seven.wav", assemblyPath);
                        break;
                    }
                case "6s":
                    {
                        sound.SoundLocation = string.Format(@"{0}\sfx\six.wav", assemblyPath);
                        break;
                    }
                case "5s":
                    {
                        sound.SoundLocation = string.Format(@"{0}\sfx\five.wav", assemblyPath);
                        break;
                    }
                case "4s":
                    {
                        sound.SoundLocation = string.Format(@"{0}\sfx\four.wav", assemblyPath);
                        break;
                    }
                case "3s":
                    {
                        sound.SoundLocation = string.Format(@"{0}\sfx\three.wav", assemblyPath);
                        break;
                    }
                case "2s":
                    {
                        sound.SoundLocation = string.Format(@"{0}\sfx\two.wav", assemblyPath);
                        break;
                    }
                case "1s":
                    {
                        sound.SoundLocation = string.Format(@"{0}\sfx\one.wav", assemblyPath);
                        break;
                    }
                case "redscore":
                    {
                        sound.SoundLocation = string.Format(@"{0}\sfx\redscore.wav", assemblyPath);
                        break;
                    }
                case "reddom":
                    {
                        sound.SoundLocation = string.Format(@"{0}\sfx\reddom.wav", assemblyPath);
                        break;
                    }
                case "redwins":
                    {
                        sound.SoundLocation = string.Format(@"{0}\sfx\redwins.wav", assemblyPath);
                        break;
                    }
                case "bluescore":
                    {
                        sound.SoundLocation = string.Format(@"{0}\sfx\bluescore.wav", assemblyPath);
                        break;
                    }
                case "bluedom":
                    {
                        sound.SoundLocation = string.Format(@"{0}\sfx\bluedom.wav", assemblyPath);
                        break;
                    }
                case "bluewins":
                    {
                        sound.SoundLocation = string.Format(@"{0}\sfx\bluewins.wav", assemblyPath);
                        break;
                    }
            }
            try
            {
                if (sync == true)
                    sound.PlaySync();
                else
                    sound.Play();
            }
            catch (Exception ex) { }
        }
        private bool isInit = true;
        private DispatcherTimer theTimer = new DispatcherTimer();
        private Postmaster pm = new Postmaster();
        public MainWindow()
        {
            InitializeComponent();
            initThis();
            isInit = false;
            
        }
        private void initThis()
        {
            // init scoreboard
            sound = new SoundPlayer();
            theScoreboard = new Scoreboard();

            // init handlers
            txtSetRaceTo.PreviewTextInput += checkAllowedText;
            txtSetGameMin.PreviewTextInput += checkAllowedText;
            txtSetGameSec.PreviewTextInput += checkAllowedText;
            txtSetPointBreakMin.PreviewTextInput += checkAllowedText;
            txtSetPointBreakSec.PreviewTextInput += checkAllowedText;
            txtSetGameBreakMin.PreviewTextInput += checkAllowedText;
            txtSetGameBreakSec.PreviewTextInput += checkAllowedText;
            txtSetRefSec.PreviewTextInput += checkAllowedText;

            // load settings
            txtSetRaceTo.Text = Settings.Default.raceTo.ToString();
            txtSetGameMin.Text = TimeSpan.FromSeconds(Settings.Default.gameTicks).Minutes.ToString();
            txtSetGameSec.Text = TimeSpan.FromSeconds(Settings.Default.gameTicks).Seconds.ToString();
            txtSetPointBreakMin.Text = TimeSpan.FromSeconds(Settings.Default.pointBreakTicks).Minutes.ToString();
            txtSetPointBreakSec.Text = TimeSpan.FromSeconds(Settings.Default.pointBreakTicks).Seconds.ToString();
            txtSetGameBreakMin.Text = TimeSpan.FromSeconds(Settings.Default.gameBreakTicks).Minutes.ToString();
            txtSetGameBreakSec.Text = TimeSpan.FromSeconds(Settings.Default.gameBreakTicks).Seconds.ToString();
            txtSetRefSec.Text = Settings.Default.refTicks.ToString();
            txtAPIURI.Text = Settings.Default.API_URI;

            //init scoreboard
            theScoreboard.state = Scoreboard.eGameState.Init;
            theScoreboard.maxTicks = Settings.Default.gameTicks;
            theScoreboard.maxRefTicks = Settings.Default.refTicks;
            theScoreboard.maxGameBreakTicks = Settings.Default.gameBreakTicks;
            theScoreboard.maxPointBreakTicks = Settings.Default.pointBreakTicks;
            theScoreboard.ticks = theScoreboard.maxTicks;
            theScoreboard.breakTicks = theScoreboard.maxGameBreakTicks;

            // init postmaster
            pm.URL = Settings.Default.API_URI;
            pm.ContentType = "application/json; charset=utf-8";
            pm.Accept = "application/json; charset=utf-8";

            // set up serial port
            serial.BaudRate = 9600;
            serial.Parity = Parity.None;
            serial.StopBits = StopBits.One;
            serial.DataBits = 8;
            serial.Handshake = Handshake.None;
            serial.RtsEnable = true;
            serial.DataReceived += serial_received;
            //if (serial.IsOpen) serial.Close();
            //serial.Open();

            updateBoard();

            // init & start timer
            theTimer.Interval = TimeSpan.FromMilliseconds(1000);
            theTimer.Tick += theTimer_elapsed;
            theTimer.Start();

            
        }
        private static bool isAllowedText(string text)
        {
            return !_regex.IsMatch(text);
        }
        private void theTimer_elapsed(object sender, System.EventArgs e)
        {
            updateBoard();
            //post_data();
        }
        private void serial_received(object sender, SerialDataReceivedEventArgs e)
        {
            string s = serial.ReadExisting();
            if (theScoreboard.state == Scoreboard.eGameState.Running) { 
                if (s.Length > 0) { 
                    
                    switch (s)
                    {
                        case "0":
                            MessageBox.Show("HOME Team SCORES!");
                            break;
                        case "1":
                            MessageBox.Show("HOME Team TOWELS!");
                            break;
                        case "2":
                            MessageBox.Show("AWAY Team SCORES!");
                            break;
                        case "3":
                            MessageBox.Show("AWAY Team TOWELS!");
                            break;
                    }
                    theScoreboard.state = Scoreboard.eGameState.RefCheck;
                }
            }
        }
        private void post_data()
        {
            pm.Method = Postmaster.eMethod.POST;
            pm.Body = JsonConvert.SerializeObject(theScoreboard);
            string s = pm.send();
        }
        private void updateBoard()
        {
            txtBlueName.Text = theScoreboard.blueTeamName;
            txtRedName.Text = theScoreboard.redTeamName;
            lblBlueScoreName.Content = string.Format("{0}'s Score", theScoreboard.blueTeamName);
            lblBlueScore.Content = theScoreboard.blueScore;
            lblRedScoreName.Content = string.Format("{0}'s Score", theScoreboard.redTeamName);
            lblRedScore.Content = theScoreboard.redScore;
            lblBreakClock.Content = TimeSpan.FromSeconds(theScoreboard.maxGameBreakTicks).ToString("mm\\:ss");
            lblGameClock.Content = TimeSpan.FromSeconds(theScoreboard.maxTicks).ToString("mm\\:ss");
            switch (theScoreboard.state)    
            {

                case Scoreboard.eGameState.Running:
                    {
                        if (theScoreboard.ticks >= 0)
                        {
                            lblGameClock.Content = TimeSpan.FromSeconds(theScoreboard.ticks).ToString("mm\\:ss");
                            if (theScoreboard.ticks == 0)
                            {
                                theScoreboard.breakTicks = theScoreboard.maxGameBreakTicks;
                                theScoreboard.state = Scoreboard.eGameState.GameBreak;
                            }
                        }
                        break;
                    }
                case Scoreboard.eGameState.GameBreak:
                    {
                        if (theScoreboard.breakTicks >= 0)
                        {
                            lblBreakClock.Content = TimeSpan.FromSeconds(theScoreboard.breakTicks).ToString("mm\\:ss");
                            if (theScoreboard.breakTicks == 0)
                            {
                                theScoreboard.state = Scoreboard.eGameState.Running;
                                theScoreboard.breakTicks = theScoreboard.maxPointBreakTicks;
                            }
                        }
                        break;
                    }
                case Scoreboard.eGameState.PointBreak:
                    {
                        if (theScoreboard.breakTicks >= 0)
                        {
                            lblBreakClock.Content = TimeSpan.FromSeconds(theScoreboard.breakTicks).ToString("mm\\:ss");
                            if (theScoreboard.breakTicks == 0)
                            {
                                theScoreboard.state = Scoreboard.eGameState.Running;
                                theScoreboard.breakTicks = theScoreboard.maxPointBreakTicks;
                            }
                        }
                        break;
                    }

                case Scoreboard.eGameState.RefCheck:
                    {
                        // TODO:
                        // ref check clock
                        break;
                    }
            }
        }
        private void cmdStart_Click(object sender, RoutedEventArgs e)
        {

            switch (theScoreboard.state)
            {
                case Scoreboard.eGameState.Init:
                    {
                        theScoreboard.state = Scoreboard.eGameState.GameBreak;
                        cmdStart.Content = "Stop";
                        break;
                    }

                case Scoreboard.eGameState.Stopped:
                    {
                        theScoreboard.state = Scoreboard.eGameState.Running;
                        cmdStart.Content = "Stop";
                        break;
                    }

                case Scoreboard.eGameState.Running:
                    {
                        theScoreboard.state = Scoreboard.eGameState.Stopped;
                        cmdStart.Content = "Start";
                        break;
                    }
            }
        }
        private void cmdReset_Click(object sender, RoutedEventArgs e)
        {
            if (theScoreboard.state == Scoreboard.eGameState.Running || theScoreboard.state == Scoreboard.eGameState.GameBreak || theScoreboard.state == Scoreboard.eGameState.PointBreak || theScoreboard.state == Scoreboard.eGameState.RefCheck)
            {
                if (MessageBox.Show("Really reset while a game is in progress?","Game in progress",MessageBoxButton.YesNo,MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    theScoreboard.state = Scoreboard.eGameState.Stopped;
                    cmdStart.Content = "Start";
                    // reset from settings
                }
            }
            else
            {
                theScoreboard.state = Scoreboard.eGameState.Stopped;
                cmdStart.Content = "Start";
                // reset from settings
            }
        }
        private void txtBlueName_TextChanged(object sender, TextChangedEventArgs e)
        {
            
            if (!isInit) {
                theScoreboard.blueTeamName = txtBlueName.Text;
                updateBoard(); 
            }
        }
        private void txtRedName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!isInit) {
                theScoreboard.redTeamName = txtRedName.Text;
                updateBoard(); 
            }
        }
        private void txtSetRaceTo_TextChanged(object sender, TextChangedEventArgs e)
        {
            int o = 0;
            int.TryParse(txtSetRaceTo.Text, out o);
            Settings.Default.raceTo = o;
            Settings.Default.Save();
        }
        private void setGameTicks()
        {
            int o1 = 0;
            int o2 = 0;
            int.TryParse(txtSetGameMin.Text, out o1);
            int.TryParse(txtSetGameSec.Text, out o2);
            Settings.Default.gameTicks = ((o1 * 60) + o2);
            Settings.Default.Save();
        }
        private void setPointBreakTicks()
        {
            int o1 = 0;
            int o2 = 0;
            int.TryParse(txtSetPointBreakMin.Text, out o1);
            int.TryParse(txtSetPointBreakSec.Text, out o2);
            Settings.Default.pointBreakTicks = ((o1 * 60) + o2);
            MessageBox.Show(string.Format("PointBreak Set To '{0}'", ((o1 * 60) + o2).ToString()));
            Settings.Default.Save();
        }
        private void setGameBreakTicks()
        {
            int o1 = 0;
            int o2 = 0;
            int.TryParse(txtSetGameBreakMin.Text, out o1);
            int.TryParse(txtSetGameBreakSec.Text, out o2);
            Settings.Default.gameBreakTicks = ((o1 * 60) + o2);
            MessageBox.Show(string.Format("GameBreak Set To '{0}'", ((o1 * 60) + o2).ToString()));
            Settings.Default.Save();
        }
        private void txtSetGameMin_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (isInit == false) setGameTicks();
        }
        private void txtSetGameSec_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (isInit == false) setGameTicks();
        }
        private void txtSetPointBreakMin_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (isInit == false) setPointBreakTicks();
        }
        private void txtSetPointBreakSec_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (isInit == false) setPointBreakTicks();
        }
        private void txtSetGameBreakMin_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (isInit == false) setGameBreakTicks();
        }
        private void txtSetGameBreakSec_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (isInit == false) setGameBreakTicks();
        }
        private void txtSetRefSec_TextChanged(object sender, TextChangedEventArgs e)
        {
            int o = 0;
            int.TryParse(txtSetRefSec.Text, out o);
            Settings.Default.refTicks = (o);
            Settings.Default.Save();
        }
        private void txtAPIURI_TextChanged(object sender, TextChangedEventArgs e)
        {
            Settings.Default.API_URI = txtAPIURI.Text;
        }
        private void checkAllowedText(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !isAllowedText(e.Text);
        }
        private void wpfMain_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (serial.IsOpen) serial.Close();
        }
    }
}
