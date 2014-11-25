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
			List<String> names = new List<string>();
			YearInfo year = datahandler.GetYear(datahandler.FirstAvailableYear);

			/***** Calculate series data ******/

			foreach (KeyValuePair<string, string[]> pair in groups) {
				String[] kgms = pair.Value;
				double[] percentages = new double[12];
				names.Add(pair.Key);
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

			double[] oms = new double[12];
			for (int i = 0; i < 12; i++) {
				if (year[i] != null) {
					Salesman sm = year[i].GetSalesman(person);
					if (sm != null) {
						oms[i] = sm.Omsaetning;
					}
					else
						oms[i] = 0;
				}
				else
					oms[i] = 0;
			}

			double[] ind = new double[12];
			for (int i = 0; i < 12; i++) {
				if (year[i] != null) {
					Salesman sm = year[i].GetSalesman(person);
					if (sm != null) {
						ind[i] = sm.Indtjening;
					}
					else
						ind[i] = 0;
				}
				else
					ind[i] = 0;
			}

			/***** Create chart for omsætning/indtjening series ******/

			MigraDoc.DocumentObjectModel.Paragraph paragraph = document.LastSection.AddParagraph("Sælger Diagram", "Heading1");
			Chart chart = new Chart();
			chart.Left = 0;
			chart.Width = Unit.FromCentimeter(22);
			chart.Height = Unit.FromCentimeter(15);

			MigraDoc.DocumentObjectModel.Shapes.Charts.Series series = chart.SeriesCollection.AddSeries();
			series.ChartType = ChartType.Line;
			series.LineFormat.Width = 10;
			series.Add(oms);
			series.SetNull();
			series.Name = "Omsætning";

			series = chart.SeriesCollection.AddSeries();
			series.ChartType = ChartType.Line;
			series.LineFormat.Width = 10;
			series.Add(ind);
			series.SetNull();
			series.Name = "Indtjening";

			XSeries xseries = chart.XValues.AddXSeries();
			xseries.Add(new string[] { "Maj", "Juni", "Juli", "Aug", "Sep", "Okt", "Nov", "Dec", "Jan", "Feb", "Marts", "April" });
			chart.XAxis.MajorTickMark = TickMarkType.Inside;
			chart.XAxis.Title.Caption = "MÅNEDER";

			chart.YAxis.MajorTickMark = TickMarkType.Outside;
			chart.YAxis.MajorTickMark = TickMarkType.Outside;
			chart.YAxis.HasMajorGridlines = true;

			chart.PlotArea.LineFormat.Color = MigraDoc.DocumentObjectModel.Colors.DarkGray;
			chart.PlotArea.LineFormat.Width = 1;
			chart.LeftArea.AddLegend();
			document.LastSection.Add(chart);

			/***** Create chart for KGM series ******/

			paragraph = document.LastSection.AddParagraph("Sælger Diagram", "Heading1");
			chart = new Chart();
			chart.Left = 0;
			chart.Width = Unit.FromCentimeter(22);
			chart.Height = Unit.FromCentimeter(15);

			for (int i = 0; i < list.Count; i++) {
				series = chart.SeriesCollection.AddSeries();
				series.ChartType = ChartType.Line;
				series.LineFormat.Width = 10;
				series.Add(list[i]);
				series.SetNull();
				series.Name = names[i];
			}

			xseries = chart.XValues.AddXSeries();
			xseries.Add(new string[] { "Maj", "Juni", "Juli", "Aug", "Sep", "Okt", "Nov", "Dec", "Jan", "Feb", "Marts", "April" });
			chart.XAxis.MajorTickMark = TickMarkType.Inside;
			chart.XAxis.Title.Caption = "MÅNEDER";

			chart.YAxis.TickLabels.Format = "#0%";
			chart.YAxis.MajorTickMark = TickMarkType.Outside;
			chart.YAxis.MajorTickMark = TickMarkType.Outside;
			chart.YAxis.HasMajorGridlines = true;

			chart.PlotArea.LineFormat.Color = MigraDoc.DocumentObjectModel.Colors.DarkGray;
			chart.PlotArea.LineFormat.Width = 1;
			chart.LeftArea.AddLegend();
			document.LastSection.Add(chart);
		}
	}
}
