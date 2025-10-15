using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Threading;


namespace OpenStartScreen
{
    public partial class MainWindow : Window
    {
        private bool isZoomedOut = false;
        private bool isApps = false;
        DispatcherTimer dispatcherTimer = new DispatcherTimer();
        private bool open = true;
        private ScaleTransform scaleTransform;
        private double targetVerticalOffset;
        private DateTime animationStartTime;
        private TimeSpan animationDuration;
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int SystemParametersInfo(int uAction, int uParam, StringBuilder lpvParam, int fuWinIni);

        WindowInteropHelper wih;
        public MainWindow()
        {
            // start info
            Console.WriteLine("OpenStartScreen | Revised by actium_xyz\n" +
                              "                | Originally Created by kolejker\n" +
                              "                | V1.0\n\n\n\n");
            

            InitializeComponent();

            LoadStartMenuItems();
            LoadPinnedItems();
            this.WindowState = WindowState.Maximized;
            GridsPanel.AllowDrop = true;
            DataContext = new UserCard();

            scaleTransform = new ScaleTransform(1.0, 1.0);
            var translateTransform = new TranslateTransform();
            var transformGroup = new TransformGroup();
            transformGroup.Children.Add(scaleTransform);
            transformGroup.Children.Add(translateTransform);

            GridsPanel.LayoutTransform = transformGroup;
            this.KeyDown += MainWindow_KeyDown;
            this.Deactivated += MainWindow_LostFocus;
            dispatcherTimer.Interval = TimeSpan.FromMilliseconds(20);
            dispatcherTimer.Tick += DispatcherTimer_Tick;
            dispatcherTimer.Start();

            GridsPanel.Drop += GridsPanel_Drop;
            GridsPanel.DragOver += GridsPanel_DragOver;

            WindowsKey.MouseEnter += WindowsKey_MouseEnter;
            WindowsKey.MouseLeave += WindowsKey_MouseLeave;

            this.Activated += MainWindow_Activated;
            CatagoriesScroll.PreviewMouseWheel += ScrollViewer_PreviewMouseWheel;
            wih = new WindowInteropHelper(this);

        }

        private void WindowsKey_MouseLeave(object sender, MouseEventArgs e)
        {
            WindowsKey.Visibility = Visibility.Collapsed;
        }

        private void WindowsKey_MouseEnter(object sender, MouseEventArgs e)
        {
            // Check if the mouse is near the corner where the button should appear
            Point mousePosition = e.GetPosition(this);
            double cornerThreshold = 0; // Adjust as needed

            if (mousePosition.X > WindowsKey.ActualWidth - cornerThreshold &&
                mousePosition.Y > WindowsKey.ActualHeight - cornerThreshold)
            {
                WindowsKey.Visibility = Visibility.Visible;
            }
        }

        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scv = (ScrollViewer)sender;
            scv.ScrollToHorizontalOffset(scv.HorizontalOffset - e.Delta);
            e.Handled = true;
        }

        private void MainWindow_Activated(object? sender, EventArgs e)
        {
            //
        }

        private void MainWindow_LostFocus(object? sender, EventArgs e)
        {
            //open = false;
            this.Hide();
            open = false;
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetForegroundWindow(IntPtr hWnd);
        private void DispatcherTimer_Tick(object? sender, EventArgs e)
        {
            //Console.WriteLine("workojg");
            Process[] pname = Process.GetProcessesByName("OpenLoader");
            if (pname.Length >= 1)
            {
                if (open)
                {
                    if (isApps)
                    {
                        double newOffset = scrollViewer.VerticalOffset - scrollViewer.ViewportHeight;
                        AnimateScrollViewer(scrollViewer, newOffset, TimeSpan.FromSeconds(1));
                    }
                    Console.WriteLine("Hiding...");
                    open = false;
                    this.Hide();
                    Thread.Sleep(200);
                }else
                {
                    Console.WriteLine("Showing...");
                    this.Show();
                    SetForegroundWindow(wih.Handle);
                    open = true;
                    Thread.Sleep(200);
                }
                // already open in bg, unhide it.
                //dispatcherTimer.Stop();
            }
            else
            {
                // Void
            }
            //Thread.Sleep(200);
        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Z)
            {
                ToggleZoom();
            }
        }

        private void ToggleZoom()
        {
            double newScale = isZoomedOut ? 1.0 : 0.3;
            var transformGroup = (TransformGroup)GridsPanel.LayoutTransform;
            var scaleTransform = (ScaleTransform)transformGroup.Children[0];
            var translateTransform = (TranslateTransform)transformGroup.Children[1];

            DoubleAnimation scaleXAnimation = new DoubleAnimation
            {
                To = newScale,
                Duration = TimeSpan.FromSeconds(0.2),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
            };

            DoubleAnimation scaleYAnimation = new DoubleAnimation
            {
                To = newScale,
                Duration = TimeSpan.FromSeconds(0.2),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
            };

            scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleXAnimation);
            scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleYAnimation);

            double translateX = isZoomedOut ? 0 : (GridsPanel.ActualWidth * (1 - newScale)) / 2;
            double translateY = isZoomedOut ? 0 : (GridsPanel.ActualHeight * (1 - newScale)) / 2;

            DoubleAnimation translateXAnimation = new DoubleAnimation
            {
                To = translateX,
                Duration = TimeSpan.FromSeconds(0.2),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
            };

            DoubleAnimation translateYAnimation = new DoubleAnimation
            {
                To = translateY,
                Duration = TimeSpan.FromSeconds(0.2),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
            };

            translateTransform.BeginAnimation(TranslateTransform.XProperty, translateXAnimation);
            translateTransform.BeginAnimation(TranslateTransform.YProperty, translateYAnimation);

            isZoomedOut = !isZoomedOut;
        }


        private const int SPI_GETDESKWALLPAPER = 0x0073;
        private const int MAX_PATH = 260;

        private string GetWallpaperPath()
        {
            var wallpaperPath = new StringBuilder(MAX_PATH);
            SystemParametersInfo(SPI_GETDESKWALLPAPER, MAX_PATH, wallpaperPath, 0);
            return wallpaperPath.ToString();
        }

        private void GoToApps_Click(object sender, RoutedEventArgs e)
        {
            isApps = true;
            double newOffset = scrollViewer.VerticalOffset + scrollViewer.ViewportHeight;
            AnimateScrollViewer(scrollViewer, newOffset, TimeSpan.FromSeconds(1));
        }

        private void GoToStart_Click(object sender, RoutedEventArgs e)
        {
            isApps = false;
            double newOffset = scrollViewer.VerticalOffset - scrollViewer.ViewportHeight;
            AnimateScrollViewer(scrollViewer, newOffset, TimeSpan.FromSeconds(1));
        }

        private void AnimateScrollViewer(ScrollViewer scrollViewer, double toValue, TimeSpan duration)
        {
            targetVerticalOffset = toValue;
            animationDuration = duration;
            animationStartTime = DateTime.Now;

            CompositionTarget.Rendering += OnCompositionTargetRendering;
        }

        private void OnCompositionTargetRendering(object sender, EventArgs e)
        {
            var elapsed = DateTime.Now - animationStartTime;
            if (elapsed >= animationDuration)
            {
                scrollViewer.ScrollToVerticalOffset(targetVerticalOffset);
                CompositionTarget.Rendering -= OnCompositionTargetRendering;
            }
            else
            {
                double progress = elapsed.TotalMilliseconds / animationDuration.TotalMilliseconds;
                double currentOffset = scrollViewer.VerticalOffset;
                double newOffset = currentOffset + (targetVerticalOffset - currentOffset) * progress;
                scrollViewer.ScrollToVerticalOffset(newOffset);
            }
        }
        private const string StartMenuPath = @"C:\ProgramData\Microsoft\Windows\Start Menu\Programs";
        private static string PinnedStartMenuPath = Environment.ExpandEnvironmentVariables(@"%AppData%\Microsoft\Internet Explorer\Quick Launch\User Pinned\StartMenu");




        private T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);
            if (parentObject == null)
                return null;

            if (parentObject is T parent)
                return parent;
            else
                return FindParent<T>(parentObject);
        }

        private bool isFirstGridAdded = false;



        private void LoadStartMenuItems()
        {
            var programFiles = Directory.EnumerateFiles(StartMenuPath, "*.lnk", SearchOption.AllDirectories);
            var rootCategory = new ProgramCategory("Other Programs");

            var categories = new Dictionary<string, ProgramCategory>
            {
                { "Other Programs", rootCategory }
            };

            foreach (var file in programFiles)
            {
                string targetPath = ShortcutResolver.Resolve(file);
                if (!string.IsNullOrEmpty(targetPath) && File.Exists(targetPath))
                {
                    var relativePath = Path.GetRelativePath(StartMenuPath, file);
                    var directory = Path.GetDirectoryName(relativePath);

                    if (string.IsNullOrEmpty(directory) || directory == ".")
                    {
                        rootCategory.Items.Add(file);
                    }
                    else
                    {
                        if (!categories.ContainsKey(directory))
                        {
                            categories[directory] = new ProgramCategory(directory);
                        }
                        categories[directory].Items.Add(file);
                    }
                }
            }

            DisplayCategories(categories);
        }

        private void LoadPinnedItems()
        {
            GridsPanel.Children.Clear();
            _allTiles.Clear();

            var desktopTileInfo = CreateDesktopTileInfo();
            if (desktopTileInfo != null)
            {
                desktopTileInfo.Tile = CreateTile(desktopTileInfo);
                _allTiles.Add(desktopTileInfo);
            }

            var pinnedFiles = Directory.EnumerateFiles(PinnedStartMenuPath, "*.lnk", SearchOption.TopDirectoryOnly);
            foreach (var file in pinnedFiles)
            {
                var tileInfo = CreatePinnedProgramTileInfo(file);
                if (tileInfo != null)
                {
                    tileInfo.Tile = CreateTile(tileInfo);
                    _allTiles.Add(tileInfo);
                }
            }

            ReorganizeTiles();
        }


        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
    }
}