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
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Threading;

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
        }

        private string[] args = Environment.GetCommandLineArgs();
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (AutoExtractExists() && HasValidArchive())
            {
                CreateTrayIcon();
                string ArchivePath = args.Last();
                string OutputPath = CreateValidOutputPath(ArchivePath);
                RunExtract(ArchivePath, OutputPath);
            }
            else
            {
                new AboutWindow().Show();
            }
        }


        #region Auto Extract Helpers
        private bool AutoExtractExists()
        {
            return args.Contains("/SoupAutoExtract");
        }

        private bool HasValidArchive()
        {
            string ArchivePath = args.Last();
            if (!File.Exists(ArchivePath))
            {
                return false;
            }
            return MessageBox.Show("Try Extract " + Path.GetFileNameWithoutExtension(ArchivePath) + " to " + Path.GetDirectoryName(ArchivePath) + "?", "Soup", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
        }

        private string CreateValidOutputPath(string ArchivePath)
        {
            string ExtractPath = Path.Combine(Path.GetDirectoryName(ArchivePath), Path.GetFileNameWithoutExtension(ArchivePath));
            int ExtractPathCopy = 0;
            while (Directory.Exists(ExtractPath + (ExtractPathCopy == 0 ? "" : "-" + ExtractPathCopy.ToString())))
            {
                ExtractPathCopy++;
            }
            return ExtractPath + (ExtractPathCopy == 0 ? "" : "-" + ExtractPathCopy.ToString());
        }

        private async void RunExtract(string ArchivePath, string OutputPath)
        {
            try
            {
                using (Stream stream = File.OpenRead(ArchivePath))
                using (IReader SharpCompressReader = ReaderFactory.Open(stream))
                {
                    ProgressBar.Maximum = (int)stream.Length;
                    while (SharpCompressReader.MoveToNextEntry())
                    {
                        if (!SharpCompressReader.Entry.IsDirectory)
                        {
                            int EntryTotal = (int)SharpCompressReader.Entry.CompressedSize;
                            ProgressBar.Value += EntryTotal / 2;
                            await Task.Delay(2000);
                            await Task.Run(() =>
                            {
                                SharpCompressReader.WriteEntryToDirectory(OutputPath, new ExtractionOptions()
                                {
                                    ExtractFullPath = true,
                                    Overwrite = true
                                });
                            });
                            ProgressBar.Value += EntryTotal / 2;
                            await Task.Delay(2000);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Status = "Error";
                new AboutWindow().Show();
                MessageBox.Show(ex.Message, "Soup", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            await Task.Delay(1000);
            Status = "Finished";
            CreateNotification("Extract Complete", "Click to Open");
        }
        #endregion
        #region Tray Icon
        private System.Windows.Forms.NotifyIcon TrayIcon = new System.Windows.Forms.NotifyIcon();
        public string Status
        {
            set
            {
                TrayIcon.Text = value;
            }
        }
        private void CreateTrayIcon()
        {
            TrayIcon.Icon = System.Drawing.Icon.ExtractAssociatedIcon(System.Reflection.Assembly.GetExecutingAssembly().Location);
            Status = "Idle";
            TrayIcon.Click += (object o, EventArgs e) => {
                this.Focus();
            };
        }
        private void CreateNotification(string Title, string Details)
        {
            TrayIcon.ShowBalloonTip(3000, Title, Details, System.Windows.Forms.ToolTipIcon.None);
        }
        private void CreateNotificationClickEvent(string OutputPath)
        {
            if (CreatedClickEvent)
            {
                return;
            }
            CreatedClickEvent = true;
            TrayIcon.BalloonTipClicked += (object sender, EventArgs e) =>
            {
                Process.Start(OutputPath);
            };
        }
        private bool CreatedClickEvent = false;
        #endregion

        public System.Windows.Controls.ProgressBar ProgressBar => ProgressBar;

        #region Window Titlebar
        public event PropertyChangedEventHandler PropertyChanged;
        private void Notify(params string[] PropertyNames)
        {
            foreach (string PropertyName in PropertyNames)
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }

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
    }
}
