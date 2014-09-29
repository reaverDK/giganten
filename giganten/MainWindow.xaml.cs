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

namespace giganten
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		string filename;
		DataHandler datahandler = null;
		int yearSelected = 2014;
		string salesPerson1 = null;
		string salesPerson2 = null; 

		public MainWindow(DataHandler data)
		{
			InitializeComponent();
			datahandler = data;
			
			//double[] mylist = new double[]{45,67,12,132,156,4,154,556,6,6,46,54,4,65,4,254,6,46,54};
			//DrawLines(mylist, Line1Sales1);
			//DrawLines(mylist, Line2Sales2);
			
			loadComboBoxes();
			drawGraphs();
		}

		private void MenuItem_Click_Exit(object sender, RoutedEventArgs e)
		{
			Application.Current.Shutdown();
		}

		private void MenuItem_Click_Open(object sender, RoutedEventArgs e)
		{
			if (dialogbox()) {
				StatusBox.Content = "Status: Indlæser fil";
				//bool success = datahandler.AddFile(filename);
				//StatusBox.Content = success ? "Status: Filen blev indlæst" : "Status: Fejl under filindlæsning";
			}
		}

		public bool dialogbox()
		{
			Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
			dlg.FileName = "Document"; //default file name
			dlg.DefaultExt = ".txt"; //default file extension
			dlg.Filter = "Text Documents (.txt)|*.txt"; //filter files by extension

			Nullable<bool> result = dlg.ShowDialog();
			if (result == true)
			{
				filename = dlg.FileName;
				return true;
			}
			return false;
		}

		private void MenuItem_Click_Export(object sender, RoutedEventArgs e)
		{
			pdfCreater();
		}

		private void MenuItem_Click_About(object sender, RoutedEventArgs e)
		{
			AboutWindow aboutwin = new AboutWindow();
			aboutwin.Show();
		}

		private void MenuItem_Click_Contact(object sender, RoutedEventArgs e)
		{
			ContactWindow contactWin = new ContactWindow();
			contactWin.Show();
		}

		public void pdfCreater()
		{
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

		private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (combobox_Person1 != null && combobox_Person2 != null)
			{
				if (combobox_Afd.SelectedIndex == 0)
				{
					combobox_Person1.IsEnabled = false;
					combobox_Person2.IsEnabled = false;
				}
				else
				{
					combobox_Person1.IsEnabled = true;
					combobox_Person2.IsEnabled = true;
				}
			}
			StatusBox.Content = "Status: Combobox selection changed !";
		}

		private void Select_All_Checked(object sender, RoutedEventArgs e)
		{
			if (Select_All.IsChecked == true)
			{
				this.SA_Aftaler.IsChecked = true;
				this.Omsætning.IsChecked = true;
				this.Indtjening.IsChecked = true;
				this.Abonnement.IsChecked = true;
				this.Tilbehør.IsChecked = true;
				this.Vægbeslag_Vs_TV.IsChecked = true;
				this.RTG_Mobil.IsChecked = true;
				this.RTG_Ipad.IsChecked = true;
				this.Tryghedsaftaler.IsChecked = true;
				this.TDC_TV.IsChecked = true;
			}
			drawGraphs();
		}

		private void Select_All_Unchecked(object sender, RoutedEventArgs e)
		{
			if (Select_All.IsChecked == false)
			{
				this.SA_Aftaler.IsChecked = false;
				this.Omsætning.IsChecked = false;
				this.Indtjening.IsChecked = false;
				this.Abonnement.IsChecked = false;
				this.Tilbehør.IsChecked = false;
				this.Vægbeslag_Vs_TV.IsChecked = false;
				this.RTG_Mobil.IsChecked = false;
				this.RTG_Ipad.IsChecked = false;
				this.Tryghedsaftaler.IsChecked = false;
				this.TDC_TV.IsChecked = false;
			}
			drawGraphs();
		}

		private void MenuItem_Click_Reset(object sender, RoutedEventArgs e)
		{
			ResetWindow resetWindow = new ResetWindow();
			resetWindow.Show();
		}

		public void DrawLines(double[] lineList, Polyline line){
			double scaleGraph = 200/lineList.Max();

			for (int i = 0; i < lineList.Length; i++)
			{
				line.Points.Add(new Point(600*((double)i/(double)lineList.Length),(lineList[i]*scaleGraph)));
			}			
		}

		private void drawGraphs()
		{
			graph_Person1.Background= Brushes.LightBlue;
			graph_Person2.Background = Brushes.LightGray;

			YearInfo year = datahandler.GetYear(yearSelected);
			if (salesPerson1 != null)
			{
				for (int i = 0; i < 12; i++)
				{
					if (year[i] != null)
					{

						Salesman sm = year[i].GetSalesman(salesPerson1);
						/*	if (Omsætning.IsChecked==true)
							{
								//draw graph function
							}
							if (SA_Aftaler.IsChecked == true)
							{
								//draw graph function
							}
							if (Indtjening.IsChecked == true)
							{
								//draw graph function
							}
							if (Abonnement.IsChecked == true)
							{
								//draw graph function
							}
							if (Tilbehør.IsChecked == true)
							{
								//draw graph function
							}
							if (Vægbeslag_Vs_TV.IsChecked == true)
							{
								//draw graph function
							}
							if (RTG_Mobil.IsChecked == true)
							{
								//draw graph function
							}
							if (RTG_Ipad.IsChecked == true)
							{
								//draw graph function
							}
							if (Tryghedsaftaler.IsChecked == true)
							{
								//draw graph function
							}
							if (TDC_TV.IsChecked == true)
							{
								//draw graph function
							}*/
					}
				}
			}
		}

		private void loadComboBoxes()
		{
			YearInfo year = datahandler.GetYear(yearSelected);
			string[] salesMen = null;
			combobox_Person1.ItemsSource = salesMen;
			combobox_Person2.ItemsSource = salesMen;
			salesMen = year.GetSalesmen();
			/*for (int i = 0; i < salesMen.Length; i++)
			{
				combobox_Person1.Items.Add(salesMen[i]);
				combobox_Person2.Items.Add(salesMen[i]);
			}*/
		}

		private void combobox_Person1_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			try {
				salesPerson1 = (string)combobox_Person1.SelectedItem;
				drawGraphs();
			}
			catch (Exception ex) {
				
			}
		}

		private void combobox_Person2_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			try {
				salesPerson2 = (string)combobox_Person2.SelectedValue;
				drawGraphs();
			}
			catch (Exception ex) {

			}
		}
	}
}
