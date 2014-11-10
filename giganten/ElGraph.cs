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
			get { return personA; }
			set {
				personA = value;
				UpdateGraph(personA, CanvasA, LinesA, LineOmsA, LineIndA);
			}
		}
		string personA = "";

		public String PersonB {
			get { return personB; }
			set {
				personB = value;
				UpdateGraph(personB, CanvasB, LinesB, LineOmsB, LineIndB);
			}
		}
		string personB = "";

		Canvas CanvasA;
		Canvas CanvasB;
		Polyline[] LinesA;
		Polyline[] LinesB;
		Polyline LineOmsA;
		Polyline LineOmsB;
		Polyline LineIndA;
		Polyline LineIndB;

		DataHandler datahandler;
		Dictionary<string, string[]> Groups;
		CheckBox[] CheckBoxes;
		CheckBox CheckBoxOms;
		CheckBox CheckBoxInd;

		Random random = new Random();

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
			CanvasA = a;
			CanvasB = b;

			if (Groups.Count != CheckBoxes.Length)
				throw new ArgumentException("There need to be similar numbers of KGM groups and checkboxes");

			#region Line initialisation

			LinesA = new Polyline[Groups.Count];
			LinesB = new Polyline[Groups.Count];

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

			LineOmsA = new Polyline();
			LineOmsB = new Polyline();
			LineIndA = new Polyline();
			LineIndB = new Polyline();

			LineOmsA.StrokeThickness = 3;
			LineOmsB.StrokeThickness = 3;
			LineOmsA.Stroke = Brushes.Blue;
			LineOmsB.Stroke = Brushes.Blue;
			LineIndA.StrokeThickness = 3;
			LineIndB.StrokeThickness = 3;
			LineIndA.Stroke = Brushes.Green;
			LineIndB.Stroke = Brushes.Green;

			#endregion

			#region Canvas initialisation

			CanvasA.Children.Add(LineOmsA);
			CanvasA.Children.Add(LineIndA);
			foreach (Polyline line in LinesA)
				CanvasA.Children.Add(line);

			CanvasB.Children.Add(LineOmsB);
			CanvasB.Children.Add(LineIndB);
			foreach (Polyline line in LinesB)
				CanvasB.Children.Add(line);

			CanvasA.Background = Brushes.LightBlue;
			CanvasB.Background = Brushes.LightGreen;

			#endregion
		}

		public void SetSize(double width, double height) {
			CanvasA.Height = height;
			CanvasB.Height = height;
			CanvasA.Width = width;
			CanvasB.Width = width;

			TransformGroup g = new TransformGroup();
			g.Children.Add(new TranslateTransform(0, -CanvasA.Height));
			g.Children.Add(new ScaleTransform(1, -1));
			
			CanvasA.RenderTransform = g;
			CanvasB.RenderTransform = g;

			UpdateGraph();
		}

		public void UpdateGraph() {
			UpdateGraph(personA, CanvasA, LinesA, LineOmsA, LineIndA);
			UpdateGraph(personB, CanvasB, LinesB, LineOmsB, LineIndB);
		}

		private void UpdateGraph(string person, Canvas canvas, Polyline[] lines, Polyline lineoms, Polyline lineind) {
			
			foreach (Polyline line in lines) {
				line.Points.Clear();
			}
			lineoms.Points.Clear();
			lineind.Points.Clear();

			if (person == null || person == "" || person == "<Ingen sælger valgt>")
				return;

			YearInfo year = datahandler.GetYear(2014);

			#region Calculate lines

			double[] omsætning = new double[12];
			double[] indtjening = new double[12];
			Dictionary<String, double[]> kgmgroups = new Dictionary<string, double[]>();

			for (int i = 0; i < 12; i++) {
				if (CheckBoxOms.IsChecked == true) {
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
				else
					omsætning[i] = 0;

				if (CheckBoxInd.IsChecked == true) {
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
				else
					indtjening[i] = 0;
			}
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
					kgmgroups.Add((string)cb.Content, percentages);
				}
			}

			#endregion

			#region Draw background

			#endregion

			#region Draw data lines

			double maxOms = omsætning.Max();
			double maxInd = indtjening.Max();
			double maxPerc = 0;
			foreach (KeyValuePair<String, double[]> pair in kgmgroups) {
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

			DrawLines(omsætning, lineoms, maxOms, canvas);

			DrawLines(indtjening, lineind, maxOms, canvas);

			int n = 0;
			foreach (KeyValuePair<String, string[]> pair in Groups) {
				if (kgmgroups.ContainsKey(pair.Key)) {
					Polyline line = lines[n];
					DrawLines(kgmgroups[pair.Key], line, maxPerc, canvas);
				}
				n++;
			}

			#endregion
		}

		#region Mess

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

		/*private void drawGraphs() {
			DateTime now = DateTime.Now;
			TimeSpan sincelast = now - lastredraw;
			if (sincelast.TotalMilliseconds < 50)
				return;

			if (salesPerson1 != null) {
				drawGraphFor(salesPerson1, graph_Person1, lineList1, lines1);
			}
			if (salesPerson2 != null) {
				drawGraphFor(salesPerson2, graph_Person2, lineList2, lines2);
			}
			lastredraw = DateTime.Now;
		}

		private void drawGraphFor(string salesperson, Canvas canvas, List<List<double>> list, List<Polyline> lines) {
			YearInfo year = datahandler.GetYear(yearSelected);

			double[] omsætning = new double[12];
			double[] indtjening = new double[12];
			Dictionary<String, double[]> kgmgroups = new Dictionary<string, double[]>();

			// Get the data
			for (int i = 0; i < 12; i++) {
				if (Omsætning.IsChecked == true) {
					if (year[i] != null) {
						Salesman sm = year[i].GetSalesman(salesperson);
						if (sm != null) {
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

				if (Indtjening.IsChecked == true) {
					if (year[i] != null) {
						Salesman sm = year[i].GetSalesman(salesperson);
						if (sm != null) {
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
					kgmgroups.Add((string)cb.Content, percentages);
				}
			}

			// Actually draw the data

			double maxOms = omsætning.Max();
			double maxInd = indtjening.Max();
			double maxPerc = 0;
			foreach (KeyValuePair<String, double[]> pair in kgmgroups) {
				var tempmax = pair.Value.Max();
				if (tempmax > maxPerc)
					maxPerc = tempmax;
			}

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
			foreach (KeyValuePair<String, double[]> pair in kgmgroups) {
				if (n >= list.Count) {
					list.Add(new List<double>());
				}
				list[n].AddRange(pair.Value);
				line = lines[n];
				line.Points.Clear();
				canvas.Children.Add(line);
				DrawLines(pair.Value, line, maxPerc, canvas);
				n++;
			}
		}*/

		#endregion
	}
}
