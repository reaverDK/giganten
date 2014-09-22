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
using System.Windows.Shapes;

namespace giganten {
	/// <summary>
	/// Interaction logic for StartUpWindow.xaml
	/// </summary>
	public partial class StartUpWindow : Window {
		public StartUpWindow() {
			InitializeComponent();
		}

		private void Grid_MouseDown(object sender, MouseButtonEventArgs e) {
			if (e.ChangedButton == MouseButton.Left)
				this.DragMove();
		}

		private void Exit_Click(object sender, RoutedEventArgs e) {
			this.Close();
		}
	}
}
