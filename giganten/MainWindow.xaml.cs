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
		DataHandler datahandler = new DataHandler();

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
			if (dialogbox()) {
				StatusBox.Content = "Status: Indlæser fil";
				bool success = datahandler.AddFile(filename);
				StatusBox.Content = success ? "Status: Filen blev indlæst" : "Status: Fejl under filindlæsning";
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

			const string myfile = "helloworld.pdf";
			pdfRenderer.PdfDocument.Save(myfile);
			Process.Start(myfile);
		}
	}
}
