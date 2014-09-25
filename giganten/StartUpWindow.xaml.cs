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
		DataHandler dataHandler = null;

		public StartUpWindow() {
			InitializeComponent();
		}

		private void LoadDefaultFiles() {
			String[] filePaths = null;
			string file = null;
			try {
				filePaths = Directory.GetFiles(@"\rankingdata\", "*.csv");
			}
			catch (Exception e) {
				
			}
			if(filePaths != null)
				if (filePaths.Length == 1) {
					file = filePaths[0];
				}

			if (file == null) {
				SetText(StatusText, "Ingen fil fundet.\nVælg venligst en at indlæse.");
				Dispatcher.BeginInvoke(new Action(() => {
					LoadButton.IsEnabled = true;
				}));
			}
			else {
				LoadingProgressBar.IsIndeterminate = true;
				SetText(StatusText, "Indlæser filen: " + file);
				dataHandler = new DataHandler();
				Thread thread = new Thread(() => { dataHandler.LoadFile(file, this); });
				thread.Start();
			}
		}

		delegate void SetTextCallback(TextBlock control, string text);

		private void SetText(TextBlock control, string text) {
			if (control.Dispatcher.CheckAccess()) {
				control.Text = text;
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

		private void Window_Loaded(object sender, RoutedEventArgs e) {
			StatusText.Text = "Ser efter filer";
			Thread thread = new Thread(LoadDefaultFiles);
			thread.Start();
		}

		private void Exit_Click(object sender, RoutedEventArgs e) {
			this.Close();
		}

		private void LoadButton_Click(object sender, RoutedEventArgs e) {
			string file = dialogbox();
			if (file != null) {
				LoadingProgressBar.IsIndeterminate = true;
				SetText(StatusText, "Indlæser filen: " + file);
				dataHandler = new DataHandler();
				Thread thread = new Thread(() => { dataHandler.LoadFile(file,this); });
				thread.Start();
			}
		}

		public string dialogbox() {
			Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
			dlg.DefaultExt = ".csv"; //default file extension
			dlg.Filter = "Ranking file (.csv)|*.csv"; //filter files by extension

			Nullable<bool> result = dlg.ShowDialog();
			if (result == true) {
				return dlg.FileName;
			}
			return null;
		}

		private void CloseButton_Click(object sender, RoutedEventArgs e) {
			this.Close();
		}

		public void FinishedLoading(bool success) {
			if (success) {
				MainWindow main = new MainWindow(dataHandler);
				main.Show();
				this.Close();
			}
			else {
				LoadingProgressBar.IsIndeterminate = false;
				StatusText.Text = "Formåede ikke at loade filen.\nVælg venligst en ny.";
				dataHandler = null;
			}
		}
	}
}
