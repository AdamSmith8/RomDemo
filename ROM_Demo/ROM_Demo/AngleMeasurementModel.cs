using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ROM_Demo {
	public class AngleMeasurementModel : INotifyPropertyChanged {
		#region Fields
		private string _testName = string.Empty;
		private float _recordingResult = 0f;
		private WriteableBitmap _colorImageBitmap;
		private ImageSource _bodyImageSource;

		protected List<JointType> JointsToMeasure = new List<JointType>();
		#endregion

		#region Properties
		public string TestName {
			get { return _testName; }
			set {
				if (_testName != value) {
					_testName = value;
					OnPropertyChanged("TestName");
				}
			}
		}
		public float RecordingResult {
			get { return _recordingResult; }
			set {
				if (_recordingResult != value) {
					_recordingResult = value;
					OnPropertyChanged("RecordingResult");
				}
			}
		}
		public ImageSource ColorImageSource {
			get { return _colorImageBitmap; }
		}
		public ImageSource BodyImageSource {
			get { return _bodyImageSource; }
		}
		#endregion

		#region Events
		/// <summary>
		/// The event raised when a user control property has been modified.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;
		#endregion

		protected AngleMeasurementModel(int bitmapWidth, int bitmapHeight) {
			_colorImageBitmap = new WriteableBitmap(bitmapWidth, bitmapHeight, 96, 96, PixelFormats.Bgr32, null);
		}

		protected void OnPropertyChanged(string name) {
			if (PropertyChanged != null) {
				PropertyChanged.Invoke(this, new PropertyChangedEventArgs(name));
			}
		}

		/// <summary>
		/// Writes pixels from the color frame to the contained bitmap.
		/// Bitmap is accessed through the ColorImageSource property.
		/// </summary>
		/// <param name="width">The width from the frame description.</param>
		/// <param name="height">The height from the frame description.</param>
		/// <param name="pixels">The byte array that holds color pixel data that we will write.</param>
		/// <param name="stride">Equal to the width in the frame description multiplied by the number of bytes per pixel.</param>
		public void UpdateColorFrame(int width, int height, byte[] pixels, int stride) {
			_colorImageBitmap.WritePixels(new Int32Rect(0, 0, width, height), pixels, stride, 0);
		}
	}
}
