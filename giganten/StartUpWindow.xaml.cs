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
using Nea;

namespace giganten {
	enum ReadState { FindNextEntry, FindNextKGM }
	/// <summary>
	/// Interaction logic for StartUpWindow.xaml
	/// </summary>
	public partial class StartUpWindow : Window {
		DataHandler dataHandler = null;
		Dictionary<string, string[]> groups = new Dictionary<string, string[]>();
		List<string[]> ratios = new List<string[]>();
		int demoDays = 80;

		public StartUpWindow() {
			InitializeComponent();
		}

		private void LoadDefaultFiles() {
			string[] filePaths = null;
			string file = "config.ini";
			//config data
			try {
				NeaReader r = new NeaReader(new StreamReader(file));
				ReadState state = ReadState.FindNextEntry;
				string group = "ERROR";
				List<string> list = new List<string>();

				while (r.Peek() != -1) {
					NeaReader line = new NeaReader(r.ReadLine());
					string temp;

					line.SkipWhiteSpace();
					if ((char)line.Peek() == '#') // comment
						continue;

					switch (state) {
						case ReadState.FindNextEntry:
							temp = line.ReadWord();
							if (temp == "Group:") {
								line.SkipWhiteSpace();
								group = line.ReadToEnd();
								state = ReadState.FindNextKGM;
							}
							else if (temp == "Ratio:") {
								string[] ratio = new string[2];
								ratio[0] = line.ReadSection('[', ']');
								ratio[1] = line.ReadSection('[', ']');
								ratios.Add(ratio);
							}
							break;
						case ReadState.FindNextKGM:
							line.SkipWhiteSpace();
							if (line.Peek() != -1) {
								list.Add(line.ReadWord());
							}
							else {
								groups.Add(group, list.ToArray());
								list.Clear();
								state = ReadState.FindNextEntry;
							}
							break;
					}
				}
				r.Close();
			}
			catch (FileNotFoundException fnf) {
				StreamWriter w = new StreamWriter("config.ini");
				w.Write("Fill this with data");
				w.Close();
			}
			catch (Exception e) {

			}
			filePaths = null;
			file = null;
			//ranking data
			try {
				filePaths = Directory.GetFiles("rankingdata//", "*.csv");
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
				Dispatcher.BeginInvoke(new Action(() => { LoadingProgressBar.IsIndeterminate = true; }));
				SetText(StatusText, "Indlæser filen:\n" + file);
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
			if (DateTime.Today.DayOfYear > demoDays)
			{
				StatusText.Text = "Demo expired.\nPlease contact the developers.";
			}
			else {
				int dDay = demoDays - DateTime.Today.DayOfYear;
				demoText.Text = "Demo expires in: " + dDay + " days";
				Thread thread = new Thread(LoadDefaultFiles);
				thread.Start();
			}
		}

		private void Exit_Click(object sender, RoutedEventArgs e) {
			this.Close();
		}

		private void LoadButton_Click(object sender, RoutedEventArgs e) {
			string file = dialogbox();
			if (file != null) {
				LoadingProgressBar.IsIndeterminate = true;
				SetText(StatusText, "Indlæser filen:\n" + file);
				LoadButton.IsEnabled = false;
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
				MainWindow main = new MainWindow(dataHandler, groups, ratios);
				main.Show();
				this.Close();
			}
			else {
				LoadingProgressBar.IsIndeterminate = false;
				LoadButton.IsEnabled = true;
				StatusText.Text = "Formåede ikke at loade filen.\nVælg venligst en ny.";
				dataHandler = null;
			}
		}
	}
}
