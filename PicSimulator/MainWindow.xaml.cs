using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using PicSimulatorLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
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
using Brushes = System.Windows.Media.Brushes;
using Point = System.Windows.Point;

namespace PicSimulator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private string appName = "PicSimulator";
        private Settings settings;

        public IEnumerable<Instruction> Instructions => mcu?.Program?.Instructions.Values;

        public event PropertyChangedEventHandler PropertyChanged;
        private CommonOpenFileDialog ofd = null;

        private Simulator_v2 sim;
        private MCU mcu;
        private ContextMenu movableControlContextMenu;

        public int[] Frequencies = new int[]{
            31000,
            31250,
            62500,
            125000,
            250000,
            500000,
            1000000,
            2000000,
            4000000,
            8000000,
            16000000,
        };

        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            cbMCUs.ItemsSource = Enum.GetValues(typeof(Generator.MCUReference));
            cbMCUs.SelectedIndex = 0;

            cbFrequencies.ItemsSource = Frequencies.Select((x) => x.ToString());
            cbFrequencies.SelectedIndex = 0;

            LoadSettings();
            if (!string.IsNullOrEmpty(settings.MicrochipPath))
            {
                MCU.MicrochipProcPath = settings.MicrochipPath;
            }
        }

        private void GenerateContextMenuMovableControls()
        {
            if (movableControlContextMenu == null)
                movableControlContextMenu = new ContextMenu();
            else
                movableControlContextMenu.Items.Clear();
            MenuItem mi = new MenuItem();
            mi.Header = "Link to : ";

            foreach (var item in mcu.IOs)
            {
                if (item == null)
                    continue;

                MenuItem mi2 = new MenuItem();
                mi2.Click += LinkMenuItem_Click;
                mi2.Header = item.name;
                mi.Items.Add(mi2);
            }

            movableControlContextMenu.Items.Add(mi);


        }

        private void LinkMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem mi = sender as MenuItem;
            IOControl control = movableControlContextMenu.PlacementTarget as IOControl;
            string linkTo = mi.Header.ToString();
            IO io = mcu.IOs.Where((x) => x.name.Equals(linkTo)).FirstOrDefault();
            if (io == null) return;

            control.link = io;
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem mi = sender as MenuItem;
            switch (mi.Header)
            {
                case "Test1":

                    break;

                default:
                    break;
            }
        }

        private void LoadSettings()
        {
            if (!EsseivaN.Tools.SettingManager_Fast.LoadAppName(appName, out settings))
            {
                settings = new Settings();
                SaveSettings();
            }
        }

        private void SaveSettings()
        {
            EsseivaN.Tools.SettingManager_Fast.SaveAppName(appName, settings, false, true);
        }

        private void SelectMicrochipFolder()
        {
            if (ofd == null)
            {
                ofd = new CommonOpenFileDialog()
                {
                    Title = "Select the Microchip processor folder",
                    InitialDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Microchip", "MPLABX"),
                    IsFolderPicker = true,
                };
            }

            if (ofd.ShowDialog() == CommonFileDialogResult.Ok)
            {
                // Search for the default one : pic16f1827.inc
                string path = SearchForFile("pic16f1827.inc", ofd.FileName);
                Console.WriteLine("Found path : '" + path + "'");
            }
        }

        private string SearchForFile(string filename, string rootPath)
        {
            DirectoryInfo di = new DirectoryInfo(rootPath);
            var files = di.GetFiles(filename, SearchOption.AllDirectories);
            if (files.Length == 0)
                return string.Empty;
            if (files.Length == 1)
                return files[0].FullName;

            files.Select((x) => x.FullName).ToList().Sort();
            return files[0].FullName;
        }

        private void OpenFile()
        {
            OpenFileDialog ofd = new OpenFileDialog()
            {
                Title = "Load hex file",
                Filter = "Hex file (*.hex)|*.hex|All files (*.*)|*.*",
                FilterIndex = 0,
            };

            if (ofd.ShowDialog() == true)
            {
                Stop();
                mcu.Decode(ofd.FileName);
                if (!double.TryParse(cbFrequencies.Text, out double val))
                {
                    val = 1000000;
                    cbFrequencies.Text = val.ToString();
                }
                sim?.Dispose();
                sim = new Simulator_v2(mcu, val);
                sim.OnFrequencyChanged += Sim_OnFrequencyChanged;
                //sim.OnPaused += Sim_OnPaused;

                OnPropertyChanged("Instructions");
            }
        }

        private void Sim_OnPaused(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() => listCode.ScrollIntoView(mcu.NextInstruction));
        }

        private void Sim_OnFrequencyChanged(object sender, Simulator_v2.FrequencyChangeEventArgs e)
        {
            Dispatcher.Invoke(() => txtFrequency.Text = $"Frequency is {EsseivaN.Tools.Tools.DecimalToEngineer(e.Frequency, 3, true)}Hz");
        }

        private void MenuItemOpen_Click(object sender, RoutedEventArgs e)
        {
            OpenFile();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void MenuItemLoad_Click(object sender, RoutedEventArgs e)
        {
            SelectMicrochipFolder();
        }

        private void MenuItemQuit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void MenuItemStep_Click(object sender, RoutedEventArgs e)
        {
            if (sim == null || sim.Running) return;

            //sim.Step();
            listCode.ScrollIntoView(mcu.NextInstruction);
        }

        private void MenuItemStartPause_Click(object sender, RoutedEventArgs e)
        {
            StartPause();
        }

        private void MenuItemStop_Click(object sender, RoutedEventArgs e)
        {
            Stop();
        }

        public void StartPause()
        {
            if (sim.Running)
                sim.Stop();
            else
                sim.AutoRun();
        }

        public void Stop()
        {
            if (sim == null) return;
            sim.Stop();
            //sim.Reset();
            listCode.ScrollIntoView(mcu.NextInstruction);
        }

        private void cbFrequencies_TextChanged(object sender, TextChangedEventArgs e)
        {
            Console.WriteLine(cbFrequencies.Text + "Hz");
            if (double.TryParse(cbFrequencies.Text, out double val))
            {
                cbFrequencies.Foreground = Brushes.Black;
                sim?.SetFrequency(val);
            }
            else
                cbFrequencies.Foreground = Brushes.Red;
        }

        private void ButtonBreakPoint_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            Instruction i = btn.DataContext as Instruction;

            i.BreakPointSet = !i.BreakPointSet;
        }



        private IOControl movingControl = null;
        private Point offset;
        private bool isStartPointSet = false;
        private Point startPoint;
        private double startWidth = 0;
        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            IOControl lc = new LedControl
            {
                ContextMenu = movableControlContextMenu
            };

            Canvas.SetLeft(lc, 50);
            Canvas.SetTop(lc, 50);
            lc.MouseDown += Lc_MouseDown;

            canvasIOs.Children.Add(lc);
        }

        private void CanvasIOs_MouseMove(object sender, MouseEventArgs e)
        {
            if (movingControl == null)
                return;

            if (Keyboard.Modifiers == ModifierKeys.None)
            {
                isStartPointSet = false;
                Vector move = e.GetPosition(canvasIOs) - offset;

                Canvas.SetLeft(movingControl, move.X);
                Canvas.SetTop(movingControl, move.Y);
            }
            else if (Keyboard.Modifiers == ModifierKeys.Shift)
            {
                if (!isStartPointSet)
                {
                    isStartPointSet = true;
                    startPoint = e.GetPosition(canvasIOs);
                    startWidth = movingControl.ActualWidth;
                }

                double dist = Math.Max(10, (e.GetPosition(canvasIOs) - startPoint).X + startWidth);
                movingControl.Width = movingControl.Height = dist;
            }
        }

        private void Lc_MouseDown(object sender, MouseButtonEventArgs e)
        {
            isStartPointSet = false;
            movingControl = sender as IOControl;
            offset = e.GetPosition(movingControl);
        }

        private void canvasIOs_MouseUp(object sender, MouseButtonEventArgs e)
        {
            isStartPointSet = false;
            movingControl = null;
        }

        private void Button_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (movingControl != null)
                canvasIOs.Children.Remove(movingControl);
        }

        private void cbMCUs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Enum.TryParse<Generator.MCUReference>(cbMCUs.SelectedItem.ToString(), out var result))
            {
                mcu = Generator.GenerateSimulator(result);
                GenerateContextMenuMovableControls();
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            sim?.Dispose();
        }
    }
}
