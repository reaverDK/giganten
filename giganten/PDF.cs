using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

using PdfSharp.Drawing;
using PdfSharp.Pdf;
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;
using MigraDoc.RtfRendering;
using MigraDoc.DocumentObjectModel.Shapes.Charts;
using System.Diagnostics;

namespace giganten {
	class PDF {
		public static void CreatePdf(string person1, string person2, DataHandler datahandler, Dictionary<string, string[]> groups) {
			Document document = new Document();
			document.UseCmykColor = true;

			MigraDoc.DocumentObjectModel.Section section = document.AddSection();
			section.PageSetup.Orientation = MigraDoc.DocumentObjectModel.Orientation.Landscape;
			MigraDoc.DocumentObjectModel.Paragraph paragraph = section.AddParagraph();
			paragraph.Format.Font.Color = MigraDoc.DocumentObjectModel.Color.FromCmyk(100, 20, 30, 50);

			if (person1 == "<Ingen sælger valgt>")
				person1 = null;

			if (person2 == "<Ingen sælger valgt>")
				person2 = null;

			if (person1 != null) {
				paragraph.AddFormattedText(person1, TextFormat.Bold);
				paragraph.AddLineBreak();
				paragraph.AddLineBreak();

				DefineCharts(document, person1, groups, datahandler);
			}

			if (person2 != null) {
				MigraDoc.DocumentObjectModel.Section newSection = document.AddSection();
				newSection.PageSetup.Orientation = MigraDoc.DocumentObjectModel.Orientation.Landscape;
				MigraDoc.DocumentObjectModel.Paragraph newParagraph = newSection.AddParagraph();

				newParagraph.AddFormattedText(person2, TextFormat.Bold);
				newParagraph.AddLineBreak();
				newParagraph.AddLineBreak();

				DefineCharts(document, person2, groups, datahandler);
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

		private static void DefineCharts(Document document, string person, Dictionary<string, string[]> groups, DataHandler datahandler) {
			List<double[]> list = new List<double[]>();

			foreach (KeyValuePair<string, string[]> pair in groups) {
				YearInfo year = datahandler.GetYear(datahandler.FirstAvailableYear);
				String[] kgms = pair.Value;
				double[] percentages = new double[12];
				for (int i = 0; i < 12; i++) {
					if (year[i] != null) {
						Salesman sm = year[i].GetSalesman(person);
						if (sm != null) {
							percentages[i] = sm.PercentOfTotal(kgms);
						}
						else
							percentages[i] = 0;
					}
					else
						percentages[i] = 0;
				}
				list.Add(percentages);
			}

			MigraDoc.DocumentObjectModel.Paragraph paragraph = document.LastSection.AddParagraph("Sælger Diagram", "Heading1");
			Chart chart = new Chart();
			chart.Left = 0;
			chart.Width = Unit.FromCentimeter(22);
			chart.Height = Unit.FromCentimeter(15);

			for (int i = 0; i < list.Count; i++) {
				MigraDoc.DocumentObjectModel.Shapes.Charts.Series series = chart.SeriesCollection.AddSeries();
				series.ChartType = ChartType.Line;
				series.Add(list[i]);
				series.SetNull();
			}

			XSeries xseries = chart.XValues.AddXSeries();
			xseries.Add(new string[] { "Jan", "Feb", "Marts", "April", "Maj", "Juni", "Juli", "Aug", "Sep", "Okt", "Nov", "Dec" });
			chart.XAxis.MajorTickMark = TickMarkType.Inside;
			chart.XAxis.Title.Caption = "X-Axis";

			chart.YAxis.MajorTickMark = TickMarkType.Outside;
			chart.YAxis.MajorTickMark = TickMarkType.Outside;
			chart.YAxis.HasMajorGridlines = true;

			chart.PlotArea.LineFormat.Color = MigraDoc.DocumentObjectModel.Colors.DarkGray;
			chart.PlotArea.LineFormat.Width = 3;
			document.LastSection.Add(chart);
		}
	}
}
