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

using OxyPlot;
using OxyPlot.Series;

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
		List<List<double>> lineList1 = new List<List<double>>();
		List<List<double>> lineList2 = new List<List<double>>();
		List<Polyline> lines1 = new List<Polyline>();
		List<Polyline> lines2 = new List<Polyline>();
		Random random = new Random();
		DateTime lastredraw = DateTime.Now;

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
				Polyline line1 = new Polyline();
				Polyline line2 = new Polyline();
				lines1.Add(line1);
				lines2.Add(line2);
				line1.StrokeThickness = 3;
				line2.StrokeThickness = 3;

				//Create a random dashline
				DoubleCollection dcol = new DoubleCollection(new double[] { random.Next(7), random.Next(4) });
				line1.StrokeDashArray = dcol;
				line2.StrokeDashArray = dcol;

				//line1.Stroke = Brushes.Red;
				//line2.Stroke = Brushes.Red;
				//Create a random color
				var properties = typeof(Brushes).GetProperties();
				var count = properties.Count();

				var colour = properties
				 .Select(x => new { Property = x, Index = random.Next(count) })
				 .OrderBy(x => x.Index)
				  .First();
				line1.Stroke = (SolidColorBrush)colour.Property.GetValue(colour, null);
				line2.Stroke = (SolidColorBrush)colour.Property.GetValue(colour, null);
				
				CheckBox cb = new CheckBox();
				cb.Content = group.Key;
				cb.Height = 25;
				CheckBoxPanel.Children.Add(cb);
				checkBoxList.Add(cb);
				cb.Checked += Checkbox_Changed;
				cb.Unchecked += Checkbox_Changed;
			}
			SizeChanged += MainWindow_SizeChanged;

			drawGraphs();

			
		}

		void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e) {
			drawGraphs();
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
			/*PdfDocument pdfdocument = new PdfDocument();
			PdfPage page = pdfdocument.AddPage();
			XGraphics gfx = XGraphics.FromPdfPage(page);
			XFont font = new XFont("Verdana", 11, XFontStyle.Bold);
			gfx.DrawString("My Graph", font, XBrushes.Black, new XRect(0, 0, page.Width, page.Height), XStringFormats.TopCenter);
			const string pdfFileName = "Mygraph.pdf";
			pdfdocument.Save(pdfFileName);*/

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
				DefineCharts(document, lineList1);
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
				DefineCharts(document, lineList2);
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

			//Series series = chart.SeriesCollection.AddSeries();
			//series.ChartType = ChartType.Column2D;
			//series.Add(new double[]{1, 17, 45, 5, 3, 20, 11, 23, 8, 19});
			//series.HasDataLabel = true;
			//series = chart.SeriesCollection.AddSeries();
			//series.ChartType = ChartType.Line;
			//series.Add(new double[]{41, 7, 5, 45, 13, 10, 21, 13, 18, 9});
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
			drawGraphs();
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
			drawGraphs();
		}

		private void MenuItem_Click_Reset(object sender, RoutedEventArgs e) {
			ResetWindow resetWindow = new ResetWindow();
			resetWindow.Show();
		}

		public void DrawLines(double[] lineList, Polyline line, Canvas canvas) {
			double height = canvas.Height;
			double width = canvas.Width;
			double scaleGraph = height / lineList.Max();

			for (int i = 0; i < lineList.Length; i++) {
				line.Points.Add(new System.Windows.Point(width * ((double)i / (double)lineList.Length), (lineList[i] * scaleGraph)));
			}
		}

		public void DrawLines(double[] lineList, Polyline line, double max, Canvas canvas) {
			double height = canvas.Height;
			double width = canvas.Width;
			double scaleGraph = height / max;

			for (int i = 0; i < lineList.Length; i++) {
				line.Points.Add(new System.Windows.Point(width * ((double)i / (double)lineList.Length), (lineList[i] * scaleGraph)));
			}
		}

		private void drawGraphs() {
			DateTime now = DateTime.Now;
			TimeSpan sincelast = now - lastredraw;
			if (sincelast.TotalMilliseconds < 50)
				return;
			graph_Person1.Height = canvasgrid1.ActualHeight;
			graph_Person1.Width = canvasgrid1.ActualWidth;
			TransformGroup g = new TransformGroup();
			g.Children.Add(new TranslateTransform(0, -graph_Person1.Height));
			g.Children.Add(new ScaleTransform(1, -1));
			graph_Person1.RenderTransform = g;
			graph_Person1.Background = Brushes.LightBlue;
			graph_Person1.Children.Clear();

			graph_Person2.Height = canvasgrid2.ActualHeight;
			graph_Person2.Width = canvasgrid2.ActualWidth;
			g = new TransformGroup();
			g.Children.Add(new TranslateTransform(0, -graph_Person2.Height));
			g.Children.Add(new ScaleTransform(1, -1));
			graph_Person2.RenderTransform = g;
			graph_Person2.Background = Brushes.LightGreen;
			graph_Person2.Children.Clear();

			if (salesPerson1 != null) {
				drawGraphFor(salesPerson1, graph_Person1, lineList1, lines1);
			}
			if (salesPerson2 != null) {
				drawGraphFor(salesPerson2, graph_Person2, lineList2, lines2);
			}
			lastredraw = DateTime.Now;
		}

		//PlotModel Model = null;

		private void drawGraphFor(string salesperson, Canvas canvas, List<List<double>> list, List<Polyline> lines)
		{
			//Oxy plot test stuff
			/*var tmp = new PlotModel { Title = "Hello", Subtitle = "World" };
			var series1 = new LineSeries { Title = "yuihu", MarkerType = MarkerType.Circle, MarkerSize = 3 };
			series1.Points.Add(new DataPoint(1, 10));
			series1.Points.Add(new DataPoint(1, 20));
			series1.Points.Add(new DataPoint(1, 40));
			tmp.Series.Add(series1);
			this.Model = tmp;*/

			YearInfo year = datahandler.GetYear(yearSelected);

			double[] omsætning = new double[12];
			double[] indtjening = new double[12];
			Dictionary<String, double[]> kgmgroups = new Dictionary<string, double[]>();

			// Get the data
			for (int i = 0; i < 12; i++)
			{
				if (Omsætning.IsChecked == true)
				{
					if (year[i] != null)
					{
						Salesman sm = year[i].GetSalesman(salesperson);
						if (sm != null)
						{
							omsætning[i] = sm.Omsaetning;
						}
						else
							omsætning[i] = 0;
					}
					else
						omsætning[i] = 0;
				}
				else
					omsætning[i] = 0;

				if (Indtjening.IsChecked == true)
				{
					if (year[i] != null)
					{
						Salesman sm = year[i].GetSalesman(salesperson);
						if (sm != null)
						{
							indtjening[i] = sm.Indtjening;
						}
						else
							indtjening[i] = 0;
					}
					else
						indtjening[i] = 0;
				}
				else
					indtjening[i] = 0;
			}
			foreach (CheckBox cb in checkBoxList)
			{
				if (cb.IsChecked == true)
				{
					String[] kgms = Groups[(string)cb.Content];
					double[] percentages = new double[12];
					for (int i = 0; i < 12; i++)
					{
						if (year[i] != null)
						{
							Salesman sm = year[i].GetSalesman(salesperson);
							if (sm != null)
							{
								percentages[i] = sm.PercentOfTotal(kgms);
							}
							else
								percentages[i] = 0;
						}
						else
							percentages[i] = 0;
					}
					kgmgroups.Add((string)cb.Content, percentages);
				}
			}

			// Actually draw the data

			double maxOms = omsætning.Max();
			double maxInd = indtjening.Max();
			double maxPerc = 0;
			foreach (KeyValuePair<String, double[]> pair in kgmgroups)
			{
				var tempmax = pair.Value.Max();
				if (tempmax > maxPerc)
					maxPerc = tempmax;
			}

			/*if (maxOms <= 0)
				maxOms = 10;
			if (maxInd <= 0)
				maxInd = 10;
			if (maxPerc <= 0)
				maxPerc = 0.1;*/

			Polyline line = new Polyline();
			line.StrokeThickness = 3;
			line.Stroke = Brushes.Blue;
			canvas.Children.Add(line);
			DrawLines(omsætning, line, maxOms, canvas);

			line = new Polyline();
			line.StrokeThickness = 3;
			line.StrokeDashArray = new DoubleCollection(new double[] { 3, 2 });
			line.Stroke = Brushes.Green;
			canvas.Children.Add(line);
			DrawLines(indtjening, line, maxOms, canvas);

			int n = 0;
			list.Clear();
			foreach (KeyValuePair<String, double[]> pair in kgmgroups)
			{
				if (n >= list.Count)
				{
					list.Add(new List<double>());
				}
				list[n].AddRange(pair.Value);
				line = lines[n];
				line.Points.Clear();
				canvas.Children.Add(line);
				DrawLines(pair.Value, line, maxPerc, canvas);
				n++;
			}
		}

		private void combobox_Person1_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			try {
				salesPerson1 = (string)combobox_Person1.SelectedItem;
				if (salesPerson1 == "<Ingen sælger valgt>")
					salesPerson1 = null;
				drawGraphs();
			}
			catch (Exception ex) {

			}
		}

		private void combobox_Person2_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			try {
				salesPerson2 = (string)combobox_Person2.SelectedValue;
				if (salesPerson2 == "<Ingen sælger valgt>")
					salesPerson2 = null;
				drawGraphs();
			}
			catch (Exception ex) {

			}
		}

		private void Checkbox_Changed(object sender, RoutedEventArgs e) {
			drawGraphs();
		}
	}
}
