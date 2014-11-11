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

namespace giganten {
	class ElGraph {

		public String PersonA {
			get { return CanvasGroupA.Person; }
			set {
				CanvasGroupA.Person = value;
				UpdateGraph(CanvasGroupA);
			}
		}

		public String PersonB {
			get { return CanvasGroupB.Person; }
			set {
				CanvasGroupB.Person = value;
				UpdateGraph(CanvasGroupB);
			}
		}

		CanvasGroup CanvasGroupA;
		CanvasGroup CanvasGroupB;

		DataHandler datahandler;
		Dictionary<string, string[]> Groups;
		CheckBox[] CheckBoxes;
		CheckBox CheckBoxOms;
		CheckBox CheckBoxInd;

		Random random = new Random();
		internal const double MarginHorisontal = 30;
		internal const double MarginTop = 10;

		public ElGraph(
			DataHandler data, 
			Canvas a, Canvas b, 
			Dictionary<string, string[]> groups, 
			CheckBox[] checkboxes,
			CheckBox checkboxoms,
			CheckBox checkboxind
			) 
		{
			datahandler = data;
			Groups = groups;
			CheckBoxes = checkboxes;
			CheckBoxOms = checkboxoms;
			CheckBoxInd = checkboxind;
			CanvasGroupA = new CanvasGroup(a);
			CanvasGroupB = new CanvasGroup(b);

			if (Groups.Count != CheckBoxes.Length)
				throw new ArgumentException("There need to be similar numbers of KGM groups and checkboxes");

			Polyline[] LinesA = new Polyline[Groups.Count];
			Polyline[] LinesB = new Polyline[Groups.Count];

			for (int i = 0; i < Groups.Count; i++) {
				LinesA[i] = new Polyline();
				LinesB[i] = new Polyline();

				LinesA[i].StrokeThickness = 3;
				LinesB[i].StrokeThickness = 3;

				DoubleCollection dcol = new DoubleCollection(new double[] { random.Next(7), random.Next(4) });
				LinesA[i].StrokeDashArray = dcol;
				LinesB[i].StrokeDashArray = dcol;

				var properties = typeof(Brushes).GetProperties();
				var count = properties.Count();
				var colour = properties
				 .Select(x => new { Property = x, Index = random.Next(count) })
				 .OrderBy(x => x.Index).First();
				LinesA[i].Stroke = (SolidColorBrush)colour.Property.GetValue(colour, null);
				LinesB[i].Stroke = (SolidColorBrush)colour.Property.GetValue(colour, null);
			}

			CanvasGroupA.InitKGMLines(LinesA);
			CanvasGroupB.InitKGMLines(LinesB);
		}

		public void UpdateSize(double width, double height) {
			CanvasGroupA.UpdateSize(width, height);
			CanvasGroupB.UpdateSize(width, height);

			UpdateGraph();
		}

		public void UpdateGraph() {
			UpdateGraph(CanvasGroupA);
			UpdateGraph(CanvasGroupB);
		}

		private void UpdateGraph(CanvasGroup canvasgroup) {
			foreach (Polyline line in canvasgroup.Lines) {
				line.Points.Clear();
			}
			canvasgroup.LineOms.Points.Clear();
			canvasgroup.LineInd.Points.Clear();

			if (canvasgroup.Person == null || canvasgroup.Person == "" || canvasgroup.Person == "<Ingen sælger valgt>")
				return;

			YearInfo year = datahandler.GetYear(2014);

			double[] omsætning = CalculateOmsætning(canvasgroup.Person, year);
			double[] indtjening = CalculateIndtjening(canvasgroup.Person, year);
			Dictionary<String, double[]> kgmdata = CalculateKGMData(canvasgroup.Person, year);

			double maxkroner = CalculateMaxKroner(omsætning);

			canvasgroup.Krone100.Content = maxkroner;
			canvasgroup.Krone75.Content = maxkroner * 0.75;
			canvasgroup.Krone50.Content = maxkroner * 0.5;
			canvasgroup.Krone25.Content = maxkroner * 0.25;

			double maxPerc = 0.01;
			foreach (KeyValuePair<String, double[]> pair in kgmdata) {
				var tempmax = pair.Value.Max();
				if (tempmax > maxPerc)
					maxPerc = tempmax;
			}

			if (CheckBoxOms.IsChecked == true)
				DrawLines(omsætning, canvasgroup.LineOms, maxkroner, canvasgroup.Canvas);

			if (CheckBoxInd.IsChecked == true)
				DrawLines(indtjening, canvasgroup.LineInd, maxkroner, canvasgroup.Canvas);

			int n = 0;
			foreach (KeyValuePair<String, string[]> pair in Groups) {
				if (kgmdata.ContainsKey(pair.Key)) {
					Polyline line = canvasgroup.Lines[n];
					DrawLines(kgmdata[pair.Key], line, maxPerc, canvasgroup.Canvas);
				}
				n++;
			}
		}

		private static double[] CalculateOmsætning(string person, YearInfo year) {
			double[] omsætning = new double[12];

			for (int i = 0; i < 12; i++) {
				if (year[i] != null) {
					Salesman sm = year[i].GetSalesman(person);
					if (sm != null) {
						omsætning[i] = sm.Omsaetning;
					}
					else
						omsætning[i] = 0;
				}
				else
					omsætning[i] = 0;
			}

			return omsætning;
		}

		private static double[] CalculateIndtjening(string person, YearInfo year) {
			double[] indtjening = new double[12];

			for (int i = 0; i < 12; i++) {
				if (year[i] != null) {
					Salesman sm = year[i].GetSalesman(person);
					if (sm != null) {
						indtjening[i] = sm.Indtjening;
					}
					else
						indtjening[i] = 0;
				}
				else
					indtjening[i] = 0;
			}

			return indtjening;
		}

		private Dictionary<string, double[]> CalculateKGMData(string person, YearInfo year) {
			Dictionary<String, double[]> kgmdata = new Dictionary<string, double[]>();

			foreach (CheckBox cb in CheckBoxes) {
				if (cb.IsChecked == true) {
					String[] kgms = Groups[(string)cb.Content];
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
					kgmdata.Add((string)cb.Content, percentages);
				}
			}

			return kgmdata;
		}

		private static double CalculateMaxKroner(double[] kronelist) {
			double listmax = kronelist.Max();

			double nicenumber = 10000;

			if (listmax < nicenumber)
				return nicenumber;

			while (listmax > nicenumber * 10)
				nicenumber *= 10;

			if (listmax < nicenumber * 1.5)
				return nicenumber * 1.5;

			for (int i = 2; i <= 10; i++) {
				if (listmax < nicenumber * i)
					return nicenumber * i;
			}

			return listmax;
		}

		private static void DrawLines(double[] lineList, Polyline line, double max, Canvas canvas) {
			double height = canvas.Height;
			double width = canvas.Width;
			double graphheight = height - MarginTop;
			double graphwidth = width - (2 * MarginHorisontal);

			for (int i = 0; i < lineList.Length; i++) {
				double offset = ((double)i / (double)(lineList.Length - 1));
				line.Points.Add(
					new Point(
						MarginHorisontal + graphwidth * offset, // x coordinate
						((lineList[i] / max) * graphheight) // y coordinate
						));
			}
		}
	}

	class CanvasGroup {
		internal string Person;
		internal Canvas Canvas;
		internal Polyline[] Lines;
		internal Polyline LineOms;
		internal Polyline LineInd;

		internal Polyline Hori100;
		internal Polyline Hori75;
		internal Polyline Hori50;
		internal Polyline Hori25;
		internal Polyline Hori0;

		internal Polyline VertLeft;
		internal Polyline VertRight;

		internal Label Krone100;
		internal Label Krone75;
		internal Label Krone50;
		internal Label Krone25;

		public CanvasGroup(Canvas canvas) {
			Canvas = canvas;

			Hori100 = new Polyline();
			Hori75 = new Polyline();
			Hori50 = new Polyline();
			Hori25 = new Polyline();
			Hori0 = new Polyline();

			VertLeft = new Polyline();
			VertRight = new Polyline();

			Hori100.Stroke = Brushes.Black;
			Hori75.Stroke = Brushes.Black;
			Hori50.Stroke = Brushes.Black;
			Hori25.Stroke = Brushes.Black;
			Hori0.Stroke = Brushes.Black;

			VertLeft.Stroke = Brushes.Black;
			VertRight.Stroke = Brushes.Black;

			VertLeft.StrokeThickness = 2;
			VertRight.StrokeThickness = 2;

			Krone100 = new Label();
			Krone75 = new Label();
			Krone50 = new Label();
			Krone25 = new Label();

			Krone100.RenderTransform = new ScaleTransform(1, -1, 0, 6);
			Krone75.RenderTransform = new ScaleTransform(1, -1, 0, 6);
			Krone50.RenderTransform = new ScaleTransform(1, -1, 0, 6);
			Krone25.RenderTransform = new ScaleTransform(1, -1, 0, 6);

			Krone100.Content = "100000";
			Krone75.Content = "75000";
			Krone50.Content = "50000";
			Krone25.Content = "25000";


			LineOms = new Polyline();
			LineInd = new Polyline();

			LineOms.StrokeThickness = 3;
			LineOms.Stroke = Brushes.Blue;
			LineInd.StrokeThickness = 3;
			LineInd.Stroke = Brushes.Green;


			Canvas.Children.Add(Hori100);
			Canvas.Children.Add(Hori75);
			Canvas.Children.Add(Hori50);
			Canvas.Children.Add(Hori25);
			Canvas.Children.Add(Hori0);

			Canvas.Children.Add(VertLeft);
			Canvas.Children.Add(VertRight);

			Canvas.Children.Add(Krone100);
			Canvas.Children.Add(Krone75);
			Canvas.Children.Add(Krone50);
			Canvas.Children.Add(Krone25);

			Canvas.Children.Add(LineOms);
			Canvas.Children.Add(LineInd);

			Canvas.Background = Brushes.LightBlue;
		}

		internal void InitKGMLines(Polyline[] lines) {
			Lines = lines;
			foreach (Polyline line in Lines)
				Canvas.Children.Add(line);
		}

		internal void UpdateSize(double width, double height) {
			Canvas.Height = height;
			Canvas.Width = width;

			TransformGroup g = new TransformGroup();
			g.Children.Add(new TranslateTransform(0, -Canvas.Height));
			g.Children.Add(new ScaleTransform(1, -1));
			Canvas.RenderTransform = g;

			Hori100.Points.Clear();
			Hori75.Points.Clear();
			Hori50.Points.Clear();
			Hori25.Points.Clear();
			Hori0.Points.Clear();

			VertLeft.Points.Clear();
			VertRight.Points.Clear();

			Hori100.Points.Add(new Point(ElGraph.MarginHorisontal - 5, height - ElGraph.MarginTop));
			Hori100.Points.Add(new Point(width - ElGraph.MarginHorisontal + 5, height - ElGraph.MarginTop));
			Hori75.Points.Add(new Point(ElGraph.MarginHorisontal - 5, (height - ElGraph.MarginTop) * 0.75));
			Hori75.Points.Add(new Point(width - ElGraph.MarginHorisontal + 5, (height - ElGraph.MarginTop) * 0.75));
			Hori50.Points.Add(new Point(ElGraph.MarginHorisontal - 5, (height - ElGraph.MarginTop) * 0.5));
			Hori50.Points.Add(new Point(width - ElGraph.MarginHorisontal + 5, (height - ElGraph.MarginTop) * 0.5));
			Hori25.Points.Add(new Point(ElGraph.MarginHorisontal - 5, (height - ElGraph.MarginTop) * 0.25));
			Hori25.Points.Add(new Point(width - ElGraph.MarginHorisontal + 5, (height - ElGraph.MarginTop) * 0.25));
			Hori0.Points.Add(new Point(ElGraph.MarginHorisontal - 5, 1));
			Hori0.Points.Add(new Point(width - ElGraph.MarginHorisontal + 5, 1));

			VertLeft.Points.Add(new Point(ElGraph.MarginHorisontal, height - ElGraph.MarginTop + 5));
			VertLeft.Points.Add(new Point(ElGraph.MarginHorisontal, 0));
			VertRight.Points.Add(new Point(width - ElGraph.MarginHorisontal, height - ElGraph.MarginTop + 5));
			VertRight.Points.Add(new Point(width - ElGraph.MarginHorisontal, 0));

			Canvas.SetLeft(Krone100, 0);
			Canvas.SetTop(Krone100, height - ElGraph.MarginTop);
			Krone100.Foreground = Brushes.Black;

			Canvas.SetLeft(Krone75, 0);
			Canvas.SetTop(Krone75, (height - ElGraph.MarginTop) * 0.75);
			Canvas.SetLeft(Krone50, 0);
			Canvas.SetTop(Krone50, (height - ElGraph.MarginTop) * 0.50);
			Canvas.SetLeft(Krone25, 0);
			Canvas.SetTop(Krone25, (height - ElGraph.MarginTop) * 0.25);
		}
	}
}
