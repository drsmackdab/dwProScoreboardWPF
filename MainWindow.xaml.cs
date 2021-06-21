using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using dwProScoreboardWPF.Models;
using dwProScoreboardWPF.Properties;
using dwPostmaster;
using Newtonsoft.Json;
using System.Windows.Threading;
using System.Media;
using System.Text.RegularExpressions;
using System.IO.Ports;
using System.ComponentModel;
using System.Diagnostics;

namespace dwProScoreboardWPF
{
    public partial class MainWindow : Window
    {
        private bool isInit = true;
        private Postmaster pm = new Postmaster();
        private SerialPort serial;
        private Scoreboard.eGameState lastState = Scoreboard.eGameState.GameBreak;
        private static readonly Regex _regex = new Regex("[^0-9.-]+"); // for numeric only textboxes
        private BackgroundWorker bgw = new BackgroundWorker();
        private bool isCOMSetup = false;
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
                case "buzzer":
                    {
                        sound.SoundLocation = string.Format(@"{0}\sfx\buzzer.wav", assemblyPath);
                        break;
                    }
                case "towel":
                    {
                        sound.SoundLocation = string.Format(@"{0}\sfx\sad_trombone.wav", assemblyPath);
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
        public MainWindow()
        {
            InitializeComponent();
            initThis();
        }
        private void initThis()
        {
            // init scoreboard
            isInit = true;
            sound = new SoundPlayer();
            theScoreboard = new Scoreboard();
            theScoreboard.onPropertyUpdate += scoreboard_propertyUpdate;
            theScoreboard.onSoundEvent += scoreboard_soundEvent;

            // init handlers
            txtSetRaceTo.PreviewTextInput += checkAllowedText;
            txtSetGameMin.PreviewTextInput += checkAllowedText;
            txtSetGameSec.PreviewTextInput += checkAllowedText;
            txtSetPointBreakMin.PreviewTextInput += checkAllowedText;
            txtSetPointBreakSec.PreviewTextInput += checkAllowedText;
            txtSetGameBreakMin.PreviewTextInput += checkAllowedText;
            txtSetGameBreakSec.PreviewTextInput += checkAllowedText;
            txtSetRefSec.PreviewTextInput += checkAllowedText;
            txtAPIURI.TextChanged += txtAPIURI_TextChanged;


            // load settings
            TimeSpan t;
            txtSetRaceTo.Text = Settings.Default.raceTo.ToString();
            t = TimeSpan.FromSeconds(Settings.Default.gameTicks);
            txtSetGameMin.Text = t.Minutes.ToString();
            txtSetGameSec.Text = t.Seconds.ToString();
            t = TimeSpan.FromSeconds(Settings.Default.pointBreakTicks);
            txtSetPointBreakMin.Text = t.Minutes.ToString();
            txtSetPointBreakSec.Text = t.Seconds.ToString();
            t = TimeSpan.FromSeconds(Settings.Default.gameBreakTicks);
            txtSetGameBreakMin.Text = t.Minutes.ToString();
            txtSetGameBreakSec.Text = t.Seconds.ToString();
            txtSetRefSec.Text = Settings.Default.refTicks.ToString();
            txtAPIURI.Text = Settings.Default.API_URI;
            updateBoard();

            // serial setup
            if (!isCOMSetup)
            {
                getCOMPorts();
                try
                {
                    cboCOM.SelectedValue = Settings.Default.COMPort;
                    serial = new SerialPort(Settings.Default.COMPort);
                    serial.DataReceived += COMPort_DataReceived;
                    serial.Open();
                    isCOMSetup = true;
                }
                catch
                {
                    MessageBox.Show("The COM port saved in settings is not available. You should check COM port availability and update the settings.", "Invalid COM Port", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }


            // init background worker & postmaster
            bgw.DoWork += bgw_DoWork;
            bgw.RunWorkerCompleted += bgw_workComplete;
            pm.URL = Settings.Default.API_URI;
            pm.Host = "fatboy";
            pm.ContentType = "application/json; charset=utf-8";
            pm.Accept = "application/json; charset=utf-8";
            bgw.RunWorkerAsync();

            var b = new ImageBrush();
            b.ImageSource = new BitmapImage(new Uri("images/cmdPlay.png", UriKind.Relative));
            cmdStart.Background = b;

            isInit = false;
        }
        private void scoreboard_propertyUpdate(object sender, PropertyUpdateEventArgs e)
        {
            updateBoard();
            if (bgw.IsBusy == false) { bgw.RunWorkerAsync(); }
        }
        private void scoreboard_soundEvent(object sender, string eventCode)
        {
            soundEvent(eventCode);
        }
        private void bgw_DoWork(object sender, DoWorkEventArgs e)
        {
            post_data();
        }
        private void bgw_workComplete(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled == true)
            {
                //resultLabel.Text = "Canceled!";
            }
            else if (e.Error != null)
            {
                //resultLabel.Text = "Error: " + e.Error.Message;
            }
            else
            {
                //resultLabel.Text = "Done!";
            }
        }
        private void getCOMPorts()
        {
            cboCOM.Items.Clear();
            string[] ps = SerialPort.GetPortNames();
            foreach (string p in ps)
            {
                cboCOM.Items.Add(p);
            }
        }
        private void COMPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            // serial data received from arduino LoRa
            if (theScoreboard.state == Scoreboard.eGameState.Running)
            {
                string recData = serial.ReadExisting();
                if (recData.Length == 1)
                {
                    switch (recData.Substring(0, 1).ToUpper())
                    {
                        case "0":
                            soundEvent("buzzer");
                            theScoreboard.state = Scoreboard.eGameState.RefCheck;
                            theScoreboard.lastState = Scoreboard.eGameState.PointBreak;
                            break;

                        case "1":
                            soundEvent("towel");
                            theScoreboard.state = Scoreboard.eGameState.RefCheck;
                            theScoreboard.lastState = Scoreboard.eGameState.PointBreak;
                            if (Dispatcher.CheckAccess()) { bdrBlueTowel.Visibility = Visibility.Visible; }
                            else { Dispatcher.Invoke(new Action(() => bdrBlueTowel.Visibility = Visibility.Visible)); }
                            break;

                        case "2":
                            soundEvent("buzzer");
                            theScoreboard.state = Scoreboard.eGameState.RefCheck;
                            theScoreboard.lastState = Scoreboard.eGameState.PointBreak;
                            break;

                        case "3":
                            soundEvent("towel");
                            theScoreboard.state = Scoreboard.eGameState.RefCheck;
                            theScoreboard.lastState = Scoreboard.eGameState.PointBreak;
                            if (Dispatcher.CheckAccess()) { bdrRedTowel.Visibility = Visibility.Visible; }
                            else { Dispatcher.Invoke(new Action(() => bdrRedTowel.Visibility = Visibility.Visible)); }
                            break;
                    }
                    updateBoard();
                }
            }
        }
        private static bool isAllowedText(string text)
        {
            return !_regex.IsMatch(text);
        }
        private void post_data()
        {
            pm.Method = Postmaster.eMethod.POST;
            pm.Body = JsonConvert.SerializeObject(theScoreboard);
            string s = pm.send();
            //System.Diagnostics.Debug.WriteLine(string.Format("Received='{0}'", s));
        }
        private void updateBoard()
        {
            if (Dispatcher.CheckAccess()) { txtBlueName.Text = theScoreboard.blueTeamName; } 
            else { Dispatcher.Invoke(new Action(() => txtBlueName.Text = theScoreboard.blueTeamName)); }
            
            if (Dispatcher.CheckAccess()) { txtRedName.Text = theScoreboard.redTeamName; } 
            else { Dispatcher.Invoke(new Action(() => txtRedName.Text = theScoreboard.redTeamName)); }
            
            if (Dispatcher.CheckAccess()) { lblBlueScoreName.Content = string.Format("{0}'s Score", theScoreboard.blueTeamName); } 
            else { Dispatcher.Invoke(new Action(() => lblBlueScoreName.Content = string.Format("{0}'s Score", theScoreboard.blueTeamName))); }
            
            if (Dispatcher.CheckAccess()) { lblBlueScore.Content = theScoreboard.blueScore; } 
            else { Dispatcher.Invoke(new Action(() => lblBlueScore.Content = theScoreboard.blueScore)); }

            if (Dispatcher.CheckAccess()) { lblRedScoreName.Content = string.Format("{0}'s Score", theScoreboard.redTeamName); } 
            else { Dispatcher.Invoke(new Action(() => lblRedScoreName.Content = string.Format("{0}'s Score", theScoreboard.redTeamName))); }

            if (Dispatcher.CheckAccess()) { lblRedScore.Content = theScoreboard.redScore; } 
            else { Dispatcher.Invoke(new Action(() => lblRedScore.Content = theScoreboard.redScore)); }

            if (Dispatcher.CheckAccess()) { lblGameClock.Content = TimeSpan.FromSeconds(theScoreboard.ticks).ToString("mm\\:ss"); } 
            else { Dispatcher.Invoke(new Action(() => lblGameClock.Content = TimeSpan.FromSeconds(theScoreboard.ticks).ToString("mm\\:ss"))); }

            if (Dispatcher.CheckAccess()) { lblBreakClock.Content = TimeSpan.FromSeconds(theScoreboard.breakTicks).ToString("mm\\:ss"); } 
            else { Dispatcher.Invoke(new Action(() => lblBreakClock.Content = TimeSpan.FromSeconds(theScoreboard.breakTicks).ToString("mm\\:ss"))); }

            if (Dispatcher.CheckAccess()) { lblRefCheck.Content = TimeSpan.FromSeconds(theScoreboard.refTicks).ToString("mm\\:ss"); } 
            else { Dispatcher.Invoke(new Action(() => lblRefCheck.Content = TimeSpan.FromSeconds(theScoreboard.refTicks).ToString("mm\\:ss"))); }
            
            if (theScoreboard.state == Scoreboard.eGameState.RefCheck) {
                if (Dispatcher.CheckAccess()) { bdrRefCheck.Visibility = Visibility.Visible; }
                else { Dispatcher.Invoke(new Action(() => bdrRefCheck.Visibility = Visibility.Visible)); }
            }
            else {
                if (Dispatcher.CheckAccess()) { 
                    bdrRefCheck.Visibility = Visibility.Hidden;
                    bdrBlueTowel.Visibility = Visibility.Hidden;
                    bdrRedTowel.Visibility = Visibility.Hidden;
                }
                else { 
                    Dispatcher.Invoke(new Action(() => bdrRefCheck.Visibility = Visibility.Hidden));
                    Dispatcher.Invoke(new Action(() => bdrBlueTowel.Visibility = Visibility.Hidden));
                    Dispatcher.Invoke(new Action(() => bdrRedTowel.Visibility = Visibility.Hidden));
                }
            }
        }
        private void setIcon(bool isPlay = true)
        {
            var b = new ImageBrush();
            if (isPlay == true) { b.ImageSource = new BitmapImage(new Uri("images/cmdPlay.png", UriKind.Relative)); }
            else { b.ImageSource = new BitmapImage(new Uri("images//cmdPause.png", UriKind.Relative)); }
            cmdStart.Background = b;
        }
        private void cmdStart_Click(object sender, RoutedEventArgs e)
        {
            if (theScoreboard.state == Scoreboard.eGameState.Running || theScoreboard.state == Scoreboard.eGameState.GameBreak || theScoreboard.state == Scoreboard.eGameState.PointBreak) 
            {
                lastState = theScoreboard.state;
                theScoreboard.state = Scoreboard.eGameState.Stopped;
                setIcon();
            }
            else if (theScoreboard.state == Scoreboard.eGameState.Stopped) 
            {
                theScoreboard.state = lastState;
                setIcon(false);
            }
        }
        private void cmdReset_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Reset scoreboard for new game?", "Confirm reset", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                theScoreboard.reset();
                setIcon();
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
            Settings.Default.Save();
        }
        private void setGameBreakTicks()
        {
            int o1 = 0;
            int o2 = 0;
            int.TryParse(txtSetGameBreakMin.Text, out o1);
            int.TryParse(txtSetGameBreakSec.Text, out o2);
            Settings.Default.gameBreakTicks = ((o1 * 60) + o2);
            Settings.Default.Save();
        }
        private void txtSetGameMin_TextChanged(object sender, TextChangedEventArgs e)
        {
            setGameTicks();
        }
        private void txtSetGameSec_TextChanged(object sender, TextChangedEventArgs e)
        {
            setGameTicks();
        }
        private void txtSetPointBreakMin_TextChanged(object sender, TextChangedEventArgs e)
        {
            setPointBreakTicks();
        }
        private void txtSetPointBreakSec_TextChanged(object sender, TextChangedEventArgs e)
        {
            setPointBreakTicks();
        }
        private void txtSetGameBreakMin_TextChanged(object sender, TextChangedEventArgs e)
        {
            setGameBreakTicks();
        }
        private void txtSetGameBreakSec_TextChanged(object sender, TextChangedEventArgs e)
        {
            setGameBreakTicks();
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
            Settings.Default.Save();
        }
        private void checkAllowedText(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !isAllowedText(e.Text);
        }
        
        // game time buttons
        private void cmdGameInc1Min_Click(object sender, RoutedEventArgs e)
        {
            theScoreboard.ticks += 60;
            updateBoard();
        }
        private void cmdGameInc1Sec_Click(object sender, RoutedEventArgs e)
        {
            theScoreboard.ticks += 1;
            updateBoard();
        }
        private void cmdGameDec1Sec_Click(object sender, RoutedEventArgs e)
        {
            if ((theScoreboard.ticks - 1) > 0) { theScoreboard.ticks -= 1; }
            else { theScoreboard.ticks = 0; }
            updateBoard();
        }
        private void cmdGameDec1Min_Click(object sender, RoutedEventArgs e)
        {
            if ((theScoreboard.ticks - 60) > 0) { theScoreboard.ticks -= 60; }
            else { theScoreboard.ticks = 0; }
            updateBoard();
        }

        // break buttons
        private void cmdBreakInc1Min_Click(object sender, RoutedEventArgs e)
        {
            theScoreboard.breakTicks += 60;
            updateBoard();
        }
        private void cmdBreakInc1Sec_Click(object sender, RoutedEventArgs e)
        {
            theScoreboard.breakTicks += 1;
            updateBoard();
        }
        private void cmdBreakDec1Sec_Click(object sender, RoutedEventArgs e)
        {
            if ((theScoreboard.breakTicks - 1) > 0) { theScoreboard.breakTicks -= 1; }
            else { theScoreboard.breakTicks = 0; }
            updateBoard();
        }
        private void cmdBreakDec1Min_Click(object sender, RoutedEventArgs e)
        {
            if ((theScoreboard.breakTicks - 60) > 0) { theScoreboard.breakTicks -= 60; }
            else { theScoreboard.breakTicks = 0; }
            updateBoard();
        }
        private void cmdPower_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Really close the application?","Confirm Quit",MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes) 
            { Application.Current.Shutdown(); }
            
        }
        private void cmdHorn_Click(object sender, RoutedEventArgs e)
        {
            soundEvent("buzzer");
        }
        private void cboCOM_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isInit == false)
            {
                Settings.Default.COMPort = cboCOM.Items[cboCOM.SelectedIndex].ToString();
                Settings.Default.Save();
            }
        }
        private void wpfMain_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (serial.IsOpen) { serial.Close(); serial.Dispose(); }
        }
        private void cmdIncBlueScore_Click(object sender, RoutedEventArgs e)
        {
            soundEvent("bluescore");
            theScoreboard.blueScore += 1;
            updateBoard();
            
        }
        private void cmdIncRedScore_Click(object sender, RoutedEventArgs e)
        {
            soundEvent("redscore");
            theScoreboard.redScore += 1;
            updateBoard();
        }
        private void cmdDecBlueScore_Click(object sender, RoutedEventArgs e)
        {
            if ((theScoreboard.blueScore-1) >= 0) { theScoreboard.blueScore -= 1; updateBoard(); }
        }
        private void cmdDecRedScore_Click(object sender, RoutedEventArgs e)
        {
            if ((theScoreboard.redScore - 1) >= 0) { theScoreboard.redScore -= 1; updateBoard(); }
        }
        private void cmdCancelRefCheck_Click(object sender, RoutedEventArgs e)
        {
            bdrRefCheck.Visibility = Visibility.Hidden;
            theScoreboard.refTicks = 0;
            updateBoard();
        }
        private void cmdNull_Click(object sender, RoutedEventArgs e)
        {
            soundEvent("buzzer");
            theScoreboard.state = Scoreboard.eGameState.PointBreak;
        }
        private void cmdSetGame31_Click(object sender, RoutedEventArgs e)
        {
            theScoreboard.ticks = 31;
        }
        private void cmdSetGame1min_Click(object sender, RoutedEventArgs e)
        {
            theScoreboard.ticks = 61;
        }
        private void cmdSetBreak31_Click(object sender, RoutedEventArgs e)
        {
            theScoreboard.breakTicks = 31;
        }
        private void cmdSetBreak1min_Click(object sender, RoutedEventArgs e)
        {
            theScoreboard.breakTicks = 61;
        }
    }
}
