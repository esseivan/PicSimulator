using Microsoft.Win32;
using PicSimulatorLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
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
using System.Windows.Shapes;

namespace PicSimulator
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window, INotifyPropertyChanged
	{
		public Dictionary<long, Instruction> code { get; set; }

		public IEnumerable<Instruction> instructions => code?.Values;

		public MainWindow()
		{
			InitializeComponent();
			DataContext = this;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public void OnPropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
				code = HexFileDecoder.GetInstructions(ofd.FileName);
				OnPropertyChanged("instructions");
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
	}
}
