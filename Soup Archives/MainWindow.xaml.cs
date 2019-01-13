using SharpCompress.Archives;
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

        //private string[] args = Environment.GetCommandLineArgs();
        private string[] args = { "/SoupAutoExtract", @"C:\Users\andre\Desktop\Temp\FlaggyFlags.rar" };
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Paused;
            if (AutoExtractExists() && HasValidArchive())
            {
                CreateTrayIcon();
                this.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Indeterminate;
                string ArchivePath = args.Last();
                string OutputPath = CreateValidOutputPath(ArchivePath);
                RunExtract(ArchivePath, OutputPath);
            }
            else
            {
                new AboutWindow().Show();
                Close();
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

        private double ExtractProgressOffset = 10;
        private double TotalArchiveSize = 1;
        private bool Running = false;
        private async void RunExtract(string ArchivePath, string OutputPath)
        {
            Running = true;
            LaunchSmoothProgressBar();
            MaxValue = 10;
            try
            {
                var Archive = ArchiveFactory.Open(ArchivePath);
                var Reader = Archive.ExtractAllEntries();
                Reader.EntryExtractionProgress += (object sender, ReaderExtractionEventArgs<IEntry> e) =>
                {
                    if (Double.IsInfinity(e.ReaderProgress.PercentageReadExact))
                    {
                        System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)delegate ()
                        {
                            OverviewProgress.IsIndeterminate = true;
                            this.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Indeterminate;
                        });
                    }
                    else
                    {
                        System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)delegate ()
                        {
                            DetailedSecondProgressTitle.Text = e.Item.Key + " " + e.ReaderProgress.BytesTransferred + "/" + e.Item.Size + ": " + e.ReaderProgress.PercentageRead + "%";
                            DetailedProgress.IsIndeterminate = false;
                            this.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Normal;
                            MaxValue = ExtractProgressOffset + (e.ReaderProgress.BytesTransferred / TotalArchiveSize * 80);
                            DetailedProgress.Value = e.ReaderProgress.PercentageRead;
                        });
                    }
                };
                TotalArchiveSize = Archive.Entries.Where(e => !e.IsDirectory).Sum(e => e.Size);
                long CompletedArchiveSize = 0;
                await Task.Run(() =>
                {
                    int i = 1;
                    while (Reader.MoveToNextEntry())
                    {
                        if (!Reader.Entry.IsDirectory)
                        {
                            System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)delegate ()
                            {
                                DetailedProgressTitle.Text = "Working on " + Reader.Entry.Key + " (" + i + " of " + Archive.Entries.Count() + ")";
                            });
                            i++;

                            Reader.WriteEntryToDirectory(OutputPath, new ExtractionOptions()
                            {
                                Overwrite = true,
                                ExtractFullPath = true
                            });

                            CompletedArchiveSize += Reader.Entry.Size;
                            System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)delegate ()
                            {
                                ExtractProgressOffset = 10 + (CompletedArchiveSize / TotalArchiveSize * 80);
                                MaxValue = ExtractProgressOffset;
                            });
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                Running = false;
                Status = "Error";
                this.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Error;
                new AboutWindow().Show();
                MessageBox.Show(ex.Message, "Soup", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            Running = false;
            CommandManager.InvalidateRequerySuggested();
            Status = "Finished";
            CreateNotification("Extract Complete", "Click to Open");
            while (OverviewProgress.Value<100)
            {
                OverviewProgress.Value += 0.4;
                await Task.Delay(20);
            }
        }
        private double MaxValue = 0;
        private async void LaunchSmoothProgressBar()
        {
            while (Running&&OverviewProgress.IsIndeterminate==false)
            {
                if (OverviewProgress.Value<MaxValue)
                {
                    await System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)delegate ()
                    {
                        OverviewProgress.Value += 0.4;
                    });
                    await Task.Delay(50);
                }
                else
                {
                    await Task.Delay(200);
                }
            }
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
            TrayIcon.Visible = true;
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

        #region Window Titlebar
        public event PropertyChangedEventHandler PropertyChanged;
        private void Notify(params string[] PropertyNames)
        {
            foreach (string PropertyName in PropertyNames)
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }

        private void CanExecute(object sender, CanExecuteRoutedEventArgs e) =>
            e.CanExecute = true;

        private void ExitCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !Running;
        }

        private void CloseWindow(object sender, ExecutedRoutedEventArgs e) =>
    Application.Current.Shutdown();

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

        private void OverviewProgress_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            this.TaskbarItemInfo.ProgressValue = e.NewValue / (sender as ProgressBar).Maximum;
        }
    }
}
