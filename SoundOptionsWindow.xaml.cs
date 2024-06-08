using System.Windows;

namespace Chess_UI
{
    public partial class SoundOptionsWindow : Window
    {
        public SoundOptionsWindow()
        {
            InitializeComponent();
            sldVolume.Value = App.Volume * 100; // Initialize slider with current volume
            chkMute.IsChecked = App.IsMuted; // Initialize checkbox with mute status
        }

        private void ChkMute_Checked(object sender, RoutedEventArgs e)
        {
            App.IsMuted = true;
        }

        private void ChkMute_Unchecked(object sender, RoutedEventArgs e)
        {
            App.IsMuted = false;
        }

        private void SldVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (App.MediaPlayer != null)
            {
                App.Volume = sldVolume.Value / 100;
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
