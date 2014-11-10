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
				UpdateGraph(personA, CanvasA);
			}
		}
		string personA = "";

		public String PersonB {
			get { return personB; }
			set {
				personB = value;
				UpdateGraph(personB, CanvasB);
			}
		}
		string personB = "";

		DataHandler datahandler;
		Canvas CanvasA;
		Canvas CanvasB;

		public ElGraph(DataHandler data, Canvas a, Canvas b) {
			datahandler = data;
			CanvasA = a;
			CanvasB = b;
		}

		private void UpdateGraph(string person, Canvas canvas) {
			
		}
	}
}
