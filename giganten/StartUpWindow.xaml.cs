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
using System.Windows.Shapes;
using System.IO;
using System.Threading;

namespace giganten {
	/// <summary>
	/// Interaction logic for StartUpWindow.xaml
	/// </summary>
	public partial class StartUpWindow : Window {
		String fileToLoad = null;

		public StartUpWindow() {
			InitializeComponent();
		}

		private void LoadDefaultFiles() {
			String[] filePaths = null;
			try {
				filePaths = Directory.GetFiles(@"\rankingdata\", "*.csv");
			}
			catch (Exception e) {
				
			}
			if(filePaths != null)
				if (filePaths.Length == 1) {
					fileToLoad = filePaths[0];
				}
			
			if (fileToLoad == null) {
				SetText(StatusLabel, "Ingen fil fundet");
			}
			else {
				SetText(StatusLabel, "Loader filen: " + fileToLoad);
			}
		}

		delegate void SetTextCallback(Label control, string text);

		private void SetText(Label control, string text) {
			if (control.Dispatcher.CheckAccess()) {
				control.Content = text;
			}
			else {
				SetTextCallback d = new SetTextCallback(SetText);
				control.Dispatcher.Invoke(d, new object[] { control, text });
			}
		}

		private void Grid_MouseDown(object sender, MouseButtonEventArgs e) {
			if (e.ChangedButton == MouseButton.Left)
				this.DragMove();
		}

		private void Exit_Click(object sender, RoutedEventArgs e) {
			this.Close();
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			MainWindow main = new MainWindow();
			main.Show();
			this.Close();
		}

		private void Window_Loaded(object sender, RoutedEventArgs e) {
			StatusLabel.Content = "Ser efter filer";
			Thread thread = new Thread(LoadDefaultFiles);
			thread.Start();
		}
	}
}
