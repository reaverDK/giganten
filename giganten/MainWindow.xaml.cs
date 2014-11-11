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

using PdfSharp.Drawing;
using PdfSharp.Pdf;
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;
using MigraDoc.RtfRendering;
using System.Collections.ObjectModel;
using MigraDoc.DocumentObjectModel.Shapes.Charts;


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

		string filename;
		DataHandler datahandler = null;
		int yearSelected = 2014;
		string salesPerson1 = null;
		string salesPerson2 = null;
		ElGraph graph;

		public MainWindow(DataHandler data, Dictionary<string, string[]> groups) {
			Groups = groups;
			SalesmenCollection = new ObservableCollection<string>();
			SalesmenCollection.Add("<Ingen sælger valgt>");
			datahandler = data;
			YearInfo year = datahandler.GetYear(yearSelected);
			string[] salesMen = year.GetSalesmen();
			salesMen = salesMen.OrderBy(x => x).ToArray();
			foreach (string s in salesMen) {
				SalesmenCollection.Add(s);
			}

			InitializeComponent();

			foreach (KeyValuePair<String, String[]> group in Groups) {
				CheckBox cb = new CheckBox();
				cb.Content = group.Key;
				cb.Height = 25;
				CheckBoxPanel.Children.Add(cb);
				checkBoxList.Add(cb);
				cb.Checked += Checkbox_Changed;
				cb.Unchecked += Checkbox_Changed;
			}
			SizeChanged += MainWindow_SizeChanged;

			graph = new ElGraph(
				datahandler, 
				graph_Person1,
				graph_Person2, 
				Groups, 
				checkBoxList.ToArray(),
				Omsætning,
				Indtjening);

			graph.UpdateSize(canvasgrid1.ActualWidth, canvasgrid1.ActualHeight);
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
			pdfCreater();
		}

		private void MenuItem_Click_About(object sender, RoutedEventArgs e) {
			AboutWindow aboutwin = new AboutWindow();
			aboutwin.Show();
		}

		private void MenuItem_Click_Contact(object sender, RoutedEventArgs e) {
			ContactWindow contactWin = new ContactWindow();
			contactWin.Show();
		}

		public void pdfCreater() {
			Document document = new Document();
			document.UseCmykColor = true;

			MigraDoc.DocumentObjectModel.Section section = document.AddSection();
			section.PageSetup.Orientation = MigraDoc.DocumentObjectModel.Orientation.Landscape;
			MigraDoc.DocumentObjectModel.Paragraph paragraph = section.AddParagraph();
			paragraph.Format.Font.Color = MigraDoc.DocumentObjectModel.Color.FromCmyk(100, 20, 30, 50);
			
			salesPerson1 = (string)combobox_Person1.SelectedItem;
			if (salesPerson1 == "<Ingen sælger valgt>")
			{
				salesPerson1 = null;
			}

			if (salesPerson1 != null)
			{
				paragraph.AddFormattedText(combobox_Person1.Text, TextFormat.Bold);
				paragraph.AddLineBreak();
				paragraph.AddLineBreak();
				//DefineCharts(document, lineList1);
			}

			salesPerson2 = (string)combobox_Person2.SelectedItem;
			if (salesPerson2 == "<Ingen sælger valgt>")
			{
				salesPerson2 = null;
			}

			if (salesPerson2 != null)
			{
				MigraDoc.DocumentObjectModel.Section newSection = document.AddSection();
				newSection.PageSetup.Orientation = MigraDoc.DocumentObjectModel.Orientation.Landscape;
				MigraDoc.DocumentObjectModel.Paragraph newParagraph = newSection.AddParagraph();
				newParagraph.AddFormattedText(combobox_Person2.Text, TextFormat.Bold);
				newParagraph.AddLineBreak();
				newParagraph.AddLineBreak();
				//DefineCharts(document, lineList2);
			}

			const bool unicode = false;
			const PdfFontEmbedding embedding = PdfFontEmbedding.Always;
			PdfDocumentRenderer pdfRenderer = new PdfDocumentRenderer(unicode, embedding);
			pdfRenderer.Document = document;
			pdfRenderer.RenderDocument();

			string time = DateTime.Today.ToShortDateString();
			string myfile = time + ".pdf";
			pdfRenderer.PdfDocument.Save(myfile);
			Process.Start(myfile);
		}

		public void DefineCharts(Document document, List<List<double>> list)
		{	
			MigraDoc.DocumentObjectModel.Paragraph paragraph = document.LastSection.AddParagraph("Sælger Diagram", "Heading1");
			Chart chart = new Chart();
			chart.Left = 0;
			chart.Width = Unit.FromCentimeter(22);
			chart.Height = Unit.FromCentimeter(15);

			for (int i = 0; i < list.Count; i++)
			{
				MigraDoc.DocumentObjectModel.Shapes.Charts.Series series = chart.SeriesCollection.AddSeries();
				series.ChartType = ChartType.Line;
				series.Add(list[i].ToArray());
				series.SetNull();
			}

			XSeries xseries = chart.XValues.AddXSeries();
			xseries.Add(new string[]{"Jan","Feb","Marts","April","Maj","Juni","Juli","Aug","Sep","Okt","Nov","Dec"});
			chart.XAxis.MajorTickMark = TickMarkType.Inside;
			chart.XAxis.Title.Caption = "X-Axis";

			chart.YAxis.MajorTickMark = TickMarkType.Outside;
			chart.YAxis.MajorTickMark = TickMarkType.Outside;
			chart.YAxis.HasMajorGridlines = true;

			chart.PlotArea.LineFormat.Color = MigraDoc.DocumentObjectModel.Colors.DarkGray;
			chart.PlotArea.LineFormat.Width = 3;
			document.LastSection.Add(chart);
		}

		private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			if (combobox_Person1 != null && combobox_Person2 != null) {
				if (combobox_Afd.SelectedIndex == 0) {
					combobox_Person1.IsEnabled = false;
					combobox_Person2.IsEnabled = false;
				}
				else {
					combobox_Person1.IsEnabled = true;
					combobox_Person2.IsEnabled = true;
				}
			}
			StatusBox.Content = "Status: Combobox selection changed !";
		}

		private void Select_All_Checked(object sender, RoutedEventArgs e) {
			if (Select_All.IsChecked == true) {
				this.Omsætning.IsChecked = true;
				this.Indtjening.IsChecked = true;
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
				if (salesPerson1 == "<Ingen sælger valgt>")
					salesPerson1 = null;
				graph.PersonA = salesPerson1;
			}
			catch (Exception ex) { }
		}

		private void combobox_Person2_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			try {
				salesPerson2 = (string)combobox_Person2.SelectedValue;
				if (salesPerson2 == "<Ingen sælger valgt>")
					salesPerson2 = null;
				graph.PersonB = salesPerson2;
			}
			catch (Exception ex) { }
		}

		private void Checkbox_Changed(object sender, RoutedEventArgs e) {
			graph.UpdateGraph();
		}
	}
}
