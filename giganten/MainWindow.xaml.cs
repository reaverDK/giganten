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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace giganten
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
		}

		private void MenuItem_Click_Exit(object sender, RoutedEventArgs e)
		{
			Application.Current.Shutdown();
		}

		private void MenuItem_Click_Open(object sender, RoutedEventArgs e)
		{
			Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
			dlg.FileName = "Document"; //default file name
			dlg.DefaultExt = ".txt"; //default file extension
			dlg.Filter = "Text Documents (.txt)|*.txt"; //filter files by extension

			Nullable<bool> result = dlg.ShowDialog();
			if(result == true)
			{
				string filename = dlg.FileName;
			}
		}
	}
}
