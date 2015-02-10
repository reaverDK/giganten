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
using System.Drawing;
using System.Diagnostics;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.IO;
using Nea;

namespace giganten
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public ObservableCollection<String> SalesmenCollection { get; private set; }
		Dictionary<string, string[]> Groups;
		List<CheckBox> checkBoxList = new List<CheckBox>();

		List<string> allSalesMen = new List<string>();

		Dictionary<string, string[]> departments;
		string filename;
		string depfilename;
		DataHandler datahandler = null;
		//int yearSelected = 2014;
		string salesPerson1 = null;
		string salesPerson2 = null;
		string brugernavn = null;
		ElGraph graph;
		Color[] colors;

		public MainWindow(DataHandler data, Dictionary<string, string[]> groups) {
			Groups = groups;
			SalesmenCollection = new ObservableCollection<string>();
			SalesmenCollection.Add("<INGEN SÆLGER VALGT>");
			datahandler = data;
			YearInfo year = datahandler.GetYear(datahandler.FirstAvailableYear);
			string[] salesMen = year.GetSalesmen();
			salesMen = salesMen.OrderBy(x => x).ToArray();
			foreach (string s in salesMen) {
				allSalesMen.Add(s);
			}

			InitializeComponent();

			LoadDepFiles();

			colors = GetColors(Groups.Count);

			int i = 0;
			foreach (KeyValuePair<String, String[]> group in Groups) {
				AddGroupCheckboxEntry(group.Key, colors[i]);
				i++;
			}

			SizeChanged += MainWindow_SizeChanged;
			TextMay.Content = "Maj\n" + year.Year;
			TextJune.Content = "Juni\n" + year.Year;
			TextJuly.Content = "Juli\n" + year.Year;
			TextAug.Content = "Aug\n" + year.Year;
			TextSep.Content = "Sep\n" + year.Year;
			TextOct.Content = "Okt\n" + year.Year;
			TextNov.Content = "Nov\n" + year.Year;
			TextDec.Content = "Dec\n" + year.Year;
			TextJan.Content = "Jan\n" + (year.Year + 1);
			TextFeb.Content = "Feb\n" + (year.Year + 1);
			TextMarch.Content = "Marts\n" + (year.Year + 1);
			TextApril.Content = "April\n" + (year.Year + 1);

			graph = new ElGraph(
				datahandler, 
				graph_Person1,
				graph_Person2, 
				Groups, 
				checkBoxList.ToArray(),
				Omsætning,
				Indtjening,
				TvVsVæg,
				colors);

			graph.UpdateSize(canvasgrid1.ActualWidth, canvasgrid1.ActualHeight);
		}

		private void AddGroupCheckboxEntry(string groupname, Color color) {
			CheckBox cb = new CheckBox();
			Grid grid = new Grid();
			Canvas canv = new Canvas();
			canv.Background = new SolidColorBrush(Color.FromRgb(255, 255, 255)); //(225, 255, 190)
			canv.Height = 25;
			canv.Width = 23;
			canv.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
			Polyline line = new Polyline();
			line.Points.Add(new Point(3, 3));
			line.Points.Add(new Point(3, 20));
			line.Points.Add(new Point(20, 20));
			line.Points.Add(new Point(20, 3));
			line.Points.Add(new Point(3, 3));
			line.Stroke = new SolidColorBrush(color);
			line.StrokeThickness = 4;
			line.StrokeDashArray = new DoubleCollection(new double[] { 2, 2 });
			canv.Children.Add(line);

			cb.Content = groupname;
			cb.Height = 25;
			grid.Children.Add(cb);
			grid.Children.Add(canv);
			CheckBoxPanel.Children.Add(grid);
			checkBoxList.Add(cb);
			cb.Checked += Checkbox_Changed;
			cb.Unchecked += Checkbox_Changed;
		}

		void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e) {
			graph.UpdateSize(canvasgrid1.ActualWidth, canvasgrid1.ActualHeight);
		}

		private void MenuItem_Click_Exit(object sender, RoutedEventArgs e) {
			Application.Current.Shutdown();
		}

		private void MenuItem_Click_Open(object sender, RoutedEventArgs e) {
			if (dialogbox()) {
				StatusBox.Content = "Status: Indlæser fil";
				//bool success = datahandler.AddFile(filename);
				//StatusBox.Content = success ? "Status: Filen blev indlæst" : "Status: Fejl under filindlæsning";
			}
		}

		public bool dialogbox() {
			Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
			dlg.FileName = "Document"; //default file name
			dlg.DefaultExt = ".txt"; //default file extension
			dlg.Filter = "Text Documents (.txt)|*.txt"; //filter files by extension

			Nullable<bool> result = dlg.ShowDialog();
			if (result == true) {
				filename = dlg.FileName;
				return true;
			}
			return false;
		}

		private void MenuItem_Click_Export(object sender, RoutedEventArgs e) {
			PDF.CreatePdf(salesPerson1, salesPerson2, datahandler, Groups);
		}

		private void MenuItem_Click_About(object sender, RoutedEventArgs e) {
			AboutWindow aboutwin = new AboutWindow();
			aboutwin.Show();
		}

		private void MenuItem_Click_Contact(object sender, RoutedEventArgs e) {
			ContactWindow contactWin = new ContactWindow();
			contactWin.Show();
		}

		private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			
			if (combobox_Person1 != null && combobox_Person2 != null) {
				if (combobox_Afd.SelectedIndex == 0) {
					combobox_Person1.IsEnabled = false;
					combobox_Person2.IsEnabled = false;
				}
				else if (combobox_Afd.Items[combobox_Afd.SelectedIndex] == "ALLE AFDELINGER")
				{
					combobox_Person1.IsEnabled = true;
					combobox_Person2.IsEnabled = true;
					string[] sm = datahandler.GetYear(datahandler.FirstAvailableYear).GetSalesmen();

					SalesmenCollection.Clear();
					SalesmenCollection.Add("<INGEN SÆLGER VALGT>");

					foreach (string s in sm)
					{
						SalesmenCollection.Add(s);
					}
				}
				else {
					combobox_Person1.IsEnabled = true;
					combobox_Person2.IsEnabled = true;
					foreach (string afdeling in departments.Keys)
					{
						if (afdeling == (string)combobox_Afd.Items[combobox_Afd.SelectedIndex])
						{
							SalesmenCollection.Clear();
							SalesmenCollection.Add("<INGEN SÆLGER VALGT>");

							for (int i = 0; i < departments[afdeling].Length; i++)
							{
								SalesmenCollection.Add(departments[afdeling][i]);
							}
						}
					}				
				}
			}
			StatusBox.Content = "Status: Combobox selection changed !";
		}

		private void Select_All_Checked(object sender, RoutedEventArgs e) {
			if (Select_All.IsChecked == true) {
				this.Omsætning.IsChecked = true;
				this.Indtjening.IsChecked = true;
				this.TvVsVæg.IsChecked = true;
				foreach (CheckBox cb in checkBoxList) {
					cb.IsChecked = true;
				}
			}
			System.Threading.Thread.Sleep(50);
			graph.UpdateGraph();
		}

		private void Select_All_Unchecked(object sender, RoutedEventArgs e) {
			if (Select_All.IsChecked == false) {
				this.Omsætning.IsChecked = false;
				this.Indtjening.IsChecked = false;
				this.TvVsVæg.IsChecked = false;
				foreach (CheckBox cb in checkBoxList) {
					cb.IsChecked = false;
				}
			}
			System.Threading.Thread.Sleep(50);
			graph.UpdateGraph();
		}

		private void MenuItem_Click_Reset(object sender, RoutedEventArgs e) {
			ResetWindow resetWindow = new ResetWindow();
			resetWindow.Show();
		}

		private void combobox_Person1_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			try {
				salesPerson1 = (string)combobox_Person1.SelectedItem;
				if (salesPerson1 == "<INGEN SÆLGER VALGT>")
					salesPerson1 = null;
				graph.PersonA = salesPerson1;
			}
			catch (Exception ex) { }
		}

		private void combobox_Person2_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			try {
				salesPerson2 = (string)combobox_Person2.SelectedValue;
				if (salesPerson2 == "<INGEN SÆLGER VALGT>")
					salesPerson2 = null;
				graph.PersonB = salesPerson2;
			}
			catch (Exception ex) { }
		}

		private void Checkbox_Changed(object sender, RoutedEventArgs e) {
			graph.UpdateGraph();
		}

		private static Color[] GetColors(int number) {
			Color[] colors = new Color[number];
			Random random = new Random();

			for (int i = 0; i < number; i++) {
				int tries = 0;
				do {
					tries++;
					colors[i] = new Color();
					colors[i].A = 255;
					colors[i].R = (byte)random.Next(256);
					colors[i].G = (byte)random.Next(256);
					colors[i].B = (byte)random.Next(256);

					if (colors[i].R + colors[i].G + colors[i].B > 600)
						continue;
					bool cont = false;

					int dif = Math.Abs(colors[i].R - 225);
					dif += Math.Abs(colors[i].G - 255);
					dif += Math.Abs(colors[i].B - 190);
					if (dif < 1000.0 / (double)number)
						continue;

					for (int j = 0; j < i; j++) {
						dif = Math.Abs(colors[i].R - colors[j].R);
						dif += Math.Abs(colors[i].G - colors[j].G);
						dif += Math.Abs(colors[i].B - colors[j].B);
						if (dif < 1000.0 / (double)number) {
							cont = true;
							break;
						}
					}
					if (cont)
						continue;
					break;
				} while (tries < 10);
			}

			return colors;
		}

		private void LoadDepFiles()
		{
			//string[] filePaths = null;
			string file = null;
			departments = new Dictionary<string, string[]>();
			//config data
			try
			{
				file = "Personale liste Know How.txt";
				NeaReader r = new NeaReader(new StreamReader(file));
				while (r.Peek() != -1)
				{
					char next = (char)r.Peek();
					while (char.IsWhiteSpace(next))
					{
						r.ReadLine();
						next = (char)r.Peek();
					}
					
					string departmentname = "ERROR";
					List<string> strings = new List<string>();

					while (r.Peek() != -1)
					{
						next = (char)r.Peek();
						if (!char.IsWhiteSpace(next))
						{
							if (departmentname != "ERROR")
							{
								departments.Add(departmentname, strings.ToArray());
								strings.Clear();
							}
							departmentname = r.ReadWord().ToUpper();
							r.ReadLine();
						}
						else
						{
							r.SkipWhiteSpace();
							r.ReadUntil('\t'); //løn nr
							r.SkipWhiteSpace();
							r.ReadUntil('\t'); // efternavn
							r.SkipWhiteSpace();
							r.ReadUntil('\t'); // fornavn
							r.SkipWhiteSpace();
							r.ReadUntil('\t'); // afdeling
							if (r.Peek() == -1)
								break;
							r.SkipWhiteSpace();
							brugernavn = r.ReadWord(); // brugernavn
							strings.Add(brugernavn.ToUpper());

							r.ReadLine();
						}
					}
					departments.Add(departmentname, strings.ToArray());
				}
				r.Close();
			}
			catch (FileNotFoundException fnf)
			{
				StreamWriter w = new StreamWriter("Personale liste Know How.txt");
				w.Write("Fill this with data");
				w.Close();
			}
			catch (Exception e)
			{

			}

			foreach (string afdeling in departments.Keys)
			{
				combobox_Afd.Items.Add(afdeling);
			}
			combobox_Afd.Items.Add("ALLE AFDELINGER");
		}
	}
}
