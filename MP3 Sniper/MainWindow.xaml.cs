using System;
using System.Diagnostics;
using System.Reflection;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using NAudio.Wave;
using System.Windows.Forms;

namespace MP3_Sniper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<MP3FileInfo> fileInfos;

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Verion number from assembly
            string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();

            System.Windows.Controls.MenuItem ver = new System.Windows.Controls.MenuItem();
            System.Windows.Controls.MenuItem newExistMenuItem = (System.Windows.Controls.MenuItem)this.FileMenu.Items[2];
            ver.Header = "v" + version;
            ver.IsEnabled = false;
            newExistMenuItem.Items.Add(ver);

            //Check for a new version.
            if (await UpdateCheck.CheckForUpdate("http://textuploader.com/df0s2/raw") == 1)
                {
                    // An update is available, but user has chosen not to update.
                    ver.Header = "Update Available!";
                    ver.Click += Ver_Click;
                    ver.IsEnabled = true;
                }

            fileInfos = new List<MP3FileInfo>();
        }

        private void GroupBox_DragEnter(object sender, System.Windows.DragEventArgs e)
        {
            if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop, false) == true)
            {
                e.Effects = System.Windows.DragDropEffects.All;
            }
        }

        private void GroupBox_Drop(object sender, System.Windows.DragEventArgs e)
        {
            // Get files dropped.
            string[] droppedFiles = (string[])e.Data.GetData(System.Windows.DataFormats.FileDrop);
            int counter = fileInfos.Count;

            foreach (string path in droppedFiles)
            {
                string extension = Path.GetExtension(path);

                // If file is an mp3, set up collection item. 
                if (extension == ".mp3")
                {
                    MP3FileInfo info = new MP3FileInfo();

                    info.ID = counter;
                    info.Name = Path.GetFileNameWithoutExtension(path);
                    info.Path = path;
                    info.Status = "Ready";

                    fileInfos.Add(info);
                }

                counter++;
            }

            // Set the grid datasource to the new collection of files.
            FileDataGrid.ItemsSource = fileInfos;
            FileDataGrid.Items.Refresh();
        }

        private void FrameSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                FrameTextBox.Text = e.NewValue.ToString();
            }
            catch
            {

            }
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {

            // Validation
            if (!Directory.Exists(OutputPathTextBox.Text))
            {
                System.Windows.MessageBox.Show("Output file path required!", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Disable controls.
            FrameSlider.IsEnabled = false;
            FrameScrollBar.IsEnabled = false;
            OutputPathTextBox.IsEnabled = false;
            StartButton.IsEnabled = false;

            // Loop through each file info and create new snippet.
            foreach (MP3FileInfo info in fileInfos)
            {
                using (Mp3FileReader reader = new Mp3FileReader(info.Path))
                {
                    int count = 1;
                    Mp3Frame mp3Frame = reader.ReadNextFrame();
                    System.IO.FileStream _fs = new System.IO.FileStream(OutputPathTextBox.Text + "\\" + info.Name + FileExtensionTextBox.Text + ".mp3", System.IO.FileMode.Create, System.IO.FileAccess.Write);

                    while (mp3Frame != null)
                    {
                        // Sample the chosen amount of frames. 
                        if (count > Int32.Parse(FrameTextBox.Text))
                            break;

                        _fs.Write(mp3Frame.RawData, 0, mp3Frame.RawData.Length);
                        count = count + 1;
                        mp3Frame = reader.ReadNextFrame();
                    }

                    info.Status = "Done";

                    _fs.Close();
                }
                
            }

            // Re-enable controls.
            FrameSlider.IsEnabled = true;
            FrameScrollBar.IsEnabled = true;
            OutputPathTextBox.IsEnabled = true;
            StartButton.IsEnabled = true;

            FileDataGrid.Items.Refresh();

            System.Windows.MessageBox.Show("Done!");

            Process.Start(OutputPathTextBox.Text);
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            fileInfos = new List<MP3FileInfo>();
            FileDataGrid.ItemsSource = null;
            FileDataGrid.Items.Refresh();
        }

        private void PathSelectorButton_Click(object sender, RoutedEventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    OutputPathTextBox.Text = fbd.SelectedPath;
                }
            }
        }

        private void FrameScrollBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (e.NewValue < e.OldValue)
            {
                FrameSlider.Value += 1;
            }
            else
            {
                FrameSlider.Value -= 1;
            }
        }

        private void FileDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void FrameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private async void Ver_Click(object sender, RoutedEventArgs e)
        {
            await UpdateCheck.CheckForUpdate("http://textuploader.com/df0s2/raw");
        }

        private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void GitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://github.com/rex706/");
        }
    }
}
