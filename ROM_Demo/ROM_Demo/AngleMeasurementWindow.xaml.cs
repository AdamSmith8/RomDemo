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

namespace ROM_Demo {
	/// <summary>
	/// Interaction logic for AngleMeasurementWindow.xaml
	/// </summary>
	public partial class AngleMeasurementWindow : Window {
		#region Fields
		public readonly int ModelIndex = -1;
		#endregion

		public AngleMeasurementWindow(AngleMeasurementModel model, int modelIndex) {
			InitializeComponent();

			this.DataContext = model;
			this.ModelIndex = modelIndex;
		}
	}
}
