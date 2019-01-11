using SharpCompress.Common;
using SharpCompress.Readers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


//Because we imported Windows.Forms .......
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using Path = System.IO.Path;

namespace Soup_Archives
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;

            _notifyIcon = new NotifyIcon()
            {
                Text= "Working ..."
            };
            _notifyIcon.Icon = System.Drawing.Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location);
        }

        private readonly NotifyIcon _notifyIcon;
        private const int Delay = 4000;
        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                CheckForFile();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message, "Soup");
            }

            if (DidExtract)
            {
                Hide();
                _notifyIcon.Text = "Done!";
                await Task.Delay(Delay + 2000);
                Close();
            }
        }

        private bool DidExtract = false;
        private void CheckForFile()
        {
            string[] args = Environment.GetCommandLineArgs();
            if (!args.Contains("tryOpen"))
                return;
            if (args.Count() == 0)
                return;
            string FilePath = args.Last();

            if (!File.Exists(FilePath))
                FilePath = Environment.GetCommandLineArgs()[0].ToString();

            if (!File.Exists(FilePath))
                return;

            if (MessageBox.Show("Try Extract " + Path.GetFileNameWithoutExtension(FilePath) + " to " + Path.GetDirectoryName(FilePath) + "?", "Soup", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                return;

            _notifyIcon.Visible = true;
            string ExtractPath = Path.Combine(Path.GetDirectoryName(FilePath), Path.GetFileNameWithoutExtension(FilePath));
            int ExtractPathCopy = 0;
            while (Directory.Exists(ExtractPath + (ExtractPathCopy == 0 ? "" : "-" + ExtractPathCopy.ToString())))
            {
                ExtractPathCopy++;
            }
            ExtractPath += (ExtractPathCopy == 0 ? "" : "-" + ExtractPathCopy.ToString());

            using (Stream stream = File.OpenRead(FilePath))
            using (var reader = SharpCompress.Readers.ReaderFactory.Open(stream))
            {
                while (reader.MoveToNextEntry())
                {
                    if (!reader.Entry.IsDirectory)
                    {
                        Console.WriteLine(reader.Entry.Key);
                        reader.WriteEntryToDirectory(ExtractPath, new ExtractionOptions()
                        {
                            ExtractFullPath = true,
                            Overwrite = true
                        });
                    }
                }
            }
            DidExtract = true;
            _notifyIcon.BalloonTipClicked += (s, a) =>
            {
                Process.Start(ExtractPath);
            };
            _notifyIcon.ShowBalloonTip(Delay, "Extract Complete", "Click to Open", ToolTipIcon.Info);
        }

        #region Window Titlebar
        public event PropertyChangedEventHandler PropertyChanged;
        private void Notify(params string[] PropertyNames)
        {
            foreach (string PropertyName in PropertyNames)
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }

        private void CloseWindow(object sender, ExecutedRoutedEventArgs e) =>
            Application.Current.Shutdown();

        private void CanExecute(object sender, CanExecuteRoutedEventArgs e) =>
            e.CanExecute = true;

        private void MinimizeWindow(object sender, ExecutedRoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void TitleBar_MouseMove(object sender, MouseEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                if (TitleBar.IsMouseDirectlyOver)
                {
                    DragMove();
                }
            }
        }
        #endregion

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e) =>
            System.Diagnostics.Process.Start(e.Uri.ToString());
    }
}
