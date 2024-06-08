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
using System.Windows.Shapes;

namespace Chess_UI
{
    /// <summary>
    /// Interaction logic for StartMenuWindow.xaml
    /// </summary>
    public partial class StartMenuWindow : Window
    {
        
        public StartMenuWindow()
        {
            InitializeComponent();
            SetupBackgroundMusic();
            
        }

        private void SetupBackgroundMusic()
        {
            App.MediaPlayer.Open(new Uri("pack://application:,,,/Music/737934__josefpres__piano-loops-157-octave-down-short-loop-120-bpm.wav"));
            App.MediaPlayer.MediaEnded += (s, e) =>
            {
                App.MediaPlayer.Position = TimeSpan.Zero;
                App.MediaPlayer.Play();
            };
            App.MediaPlayer.Volume = 0.5;
            App.MediaPlayer.Play();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            App.MediaPlayer.Stop(); // Stop the music when the StartMenuWindow is closed
        }

        private void BtnStartGame_Click(object sender, RoutedEventArgs e)
        {


            MainWindow mainGame = new MainWindow();
            mainGame.Show();
            this.Close();



        }

        private void BtnSoundOptions_Click(object sender, RoutedEventArgs e)
        {

            SoundOptionsWindow soundOptions = new SoundOptionsWindow();
            soundOptions.ShowDialog();  // Use ShowDialog to keep focus on options window
        }

        private void BtnGameInfo_Click(object sender, RoutedEventArgs e)
        {
            GameInfoWindow gameInfo = new GameInfoWindow();
            gameInfo.ShowDialog();  // Use ShowDialog to keep focus on game info window
        }

        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}