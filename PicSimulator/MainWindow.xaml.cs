using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using PicSimulatorLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace PicSimulator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private Settings settings;
        private string appName = "PicSimulator";

        private InstructionSet iSet = new InstructionSet();
        public IEnumerable<Instruction> Instructions => iSet.Instructions.Values;

        public event PropertyChangedEventHandler PropertyChanged;
        private CommonOpenFileDialog ofd = null;

        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            LoadSettings();
            if (!string.IsNullOrEmpty(settings.MicrochipPath))
            {
                MCU.MicrochipProcPath = settings.MicrochipPath;
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
                iSet.SetInstructions(HexFileDecoder.GetInstructions(ofd.FileName));
                OnPropertyChanged("Instructions");
                listCode.UpdateLayout();

            }
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
    }
}
