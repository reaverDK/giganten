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

using PdfSharp.Drawing;
using PdfSharp.Pdf;
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;
using MigraDoc.RtfRendering;
using System.Collections.ObjectModel;

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

		DateTime lastredraw = DateTime.Now;

		public MainWindow(DataHandler data, Dictionary<string, string[]> groups) {
			Groups = groups;
			SalesmenCollection = new ObservableCollection<string>();
			SalesmenCollection.Add("<Ingen sælger valgt>");
			datahandler = data;
			YearInfo year = datahandler.GetYear(yearSelected);
			string[] salesMen = year.GetSalesmen();
			foreach (string s in salesMen) {
				SalesmenCollection.Add(s);
			}

			InitializeComponent();

			foreach (KeyValuePair<String, String[]> group in Groups) {
				CheckBox cb = new CheckBox();
				cb.Content = group.Key;
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
			MigraDoc.DocumentObjectModel.Paragraph paragraph = section.AddParagraph();
			paragraph.Format.Font.Color = MigraDoc.DocumentObjectModel.Color.FromCmyk(100, 20, 30, 50);
			paragraph.AddFormattedText("Hello World!", TextFormat.Bold);

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
				line.Points.Add(new Point(width * ((double)i / (double)lineList.Length), (lineList[i] * scaleGraph)));
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
			graph_Person2.Height = canvasgrid2.ActualHeight;
			graph_Person2.Width = canvasgrid2.ActualWidth;
			g = new TransformGroup();
			g.Children.Add(new TranslateTransform(0, -graph_Person2.Height));
			g.Children.Add(new ScaleTransform(1, -1));
			graph_Person2.RenderTransform = g;
			graph_Person2.Background = Brushes.LightGreen;

			if (salesPerson1 != null) {
				drawGraphFor(salesPerson1, graph_Person1);
			}
			if (salesPerson2 != null) {
				drawGraphFor(salesPerson2, graph_Person2);
			}
			lastredraw = DateTime.Now;
		}

		private void drawGraphFor(string salesperson, Canvas canvas) {
			canvas.Children.Clear();
			YearInfo year = datahandler.GetYear(yearSelected);
			if (Omsætning.IsChecked == true) {
				double[] omsæts = new double[12];
				for (int i = 0; i < 12; i++) {
					if (year[i] != null) {
						Salesman sm = year[i].GetSalesman(salesperson);
						if (sm != null) {
							omsæts[i] = sm.Omsaetning;
						}
						else
							omsæts[i] = 0;
					}
					else
						omsæts[i] = 0;
				}
				Polyline line = new Polyline();
				line.StrokeThickness = 2;
				//line.StrokeDashArray = new DoubleCollection(new double[] { 5, 3 });
				line.Stroke = Brushes.Red;
				canvas.Children.Add(line);
				DrawLines(omsæts, line, canvas);
			}
			if (Indtjening.IsChecked == true) {
				double[] indtj = new double[12];
				for (int i = 0; i < 12; i++) {
					if (year[i] != null) {
						Salesman sm = year[i].GetSalesman(salesperson);
						if (sm != null) {
							indtj[i] = sm.Indtjening;
						}
						else
							indtj[i] = 0;
					}
					else
						indtj[i] = 0;
				}
				Polyline line = new Polyline();
				line.StrokeThickness = 2;
				line.StrokeDashArray = new DoubleCollection(new double[] { 3, 2 });
				line.Stroke = Brushes.Green;
				canvas.Children.Add(line);
				DrawLines(indtj, line, canvas);
			}
			foreach (CheckBox cb in checkBoxList) {
				if (cb.IsChecked == true) {
					String[] kgms = Groups[(string)cb.Content];
					double[] percentages = new double[12];
					for (int i = 0; i < 12; i++) {
						if (year[i] != null) {
							Salesman sm = year[i].GetSalesman(salesperson);
							if (sm != null) {
								percentages[i] = sm.PercentOfTotal(kgms);
							}
							else
								percentages[i] = 0;
						}
						else
							percentages[i] = 0;
					}
					Polyline line = new Polyline();
					line.StrokeThickness = 2;
					line.StrokeDashArray = new DoubleCollection(new double[] { 5, 3 });
					line.Stroke = Brushes.Blue;
					canvas.Children.Add(line);
					DrawLines(percentages, line, canvas);
						
				}
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
