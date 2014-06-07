using Microsoft.Kinect;
using ROM_Demo.AngleMeasurementModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Collections.ObjectModel;
using ROM_Demo.Framework;
using System.Globalization;

namespace ROM_Demo {
	/// <summary>
	/// Interaction logic for SelectionWindow.xaml
	/// </summary>
	public partial class SelectionWindow : Window {
		#region Constants
		/// <summary>
		/// The number of bytes a single color pixel will take up.
		/// This is mainly used with color stream data.
		/// </summary>
		readonly int BYTES_PER_PIXEL = (PixelFormats.Bgr32.BitsPerPixel + 7) / 8;
		#endregion

		#region Fields
		/// <summary>
		/// The primary sensor we will be retrieving sensor data from.
		/// </summary>
		private KinectSensor _kinect;

		/// <summary>
		/// Handles all data releated to the color stream.
		/// </summary>
		private ColorFrameReader _colorReader;
		/// <summary>
		/// The array that will temporarily hold color data when we receive it, before sending it to the bitmap.
		/// </summary>
		private byte[] _intermediaryArray;
		/// <summary>
		/// The source for the image controls that we will write color data to.
		/// </summary>
		private WriteableBitmap _imageBitmap;
		/// <summary>
		/// Width of a frame in color space.
		/// </summary>
		private int _colorFrameWidth;
		/// <summary>
		/// Height of a frame in color space.
		/// </summary>
		private int _colorFrameHeight;

		/// <summary>
		/// Handles all data related to the body/skeleton stream.
		/// </summary>
		private BodyFrameReader _bodyReader;
		/// <summary>
		/// Holds the group of bodies refreshed each body frame.
		/// </summary>
		private Body[] _bodies;
		/// <summary>
		/// A local copy of the coordinate mapper for converting joint positions.
		/// </summary>
		private CoordinateMapper _coordinateMapper;
		/// <summary>
		/// The drawing group needed to create drawing contexts, where we will draw body data.
		/// </summary>
		private DrawingGroup _drawingGroup;

		private AngleMeasurementWindow _measurementWindow;
		private ObservableCollection<AngleMeasurementModel> _measurementModels = new ObservableCollection<AngleMeasurementModel>();
		#endregion

		#region Properties
		public ObservableCollection<AngleMeasurementModel> MeasurementModels {
			get { return _measurementModels; }
		}
		#endregion

		public SelectionWindow() {
			InitializeComponent();

			// Initialize the sensor and all needed streams
			InitializeKinect();

			LoadMeasurementModels();

			InitializeLayout();
		}

		/// <summary>
		/// Initialization for loading the Kinect sensor.
		/// Also call the initializations for the color and body/skeleton streams.
		/// </summary>
		private void InitializeKinect() {
			// Set our KinectSensor object to the default sensor
			_kinect = KinectSensor.Default;

			// Check to make sure the sensor we obtained is valid
			if (_kinect == null) {
				MessageBox.Show("Error obtaining Kinect sensor data. Please reconnect device and try again.");
				this.Close();
			}

			// Open the Kinect for reading
			_kinect.Open();

			// Initialize sensor-specific readers
			InitializeColorReading();
			InitializeBodyReading();
		}

		/// <summary>
		/// Initialization for the color stream.
		/// </summary>
		private void InitializeColorReading() {
			// Cache the frame description for measurement references
			FrameDescription fd = _kinect.ColorFrameSource.FrameDescription;

			// Set the color frame width and height for usage later
			_colorFrameWidth = fd.Width;
			_colorFrameHeight = fd.Height;

			// Open the color reader from the frame source
			_colorReader = _kinect.ColorFrameSource.OpenReader();
			// Allocate the space needed for the intermediary array to hold color pixels
			_intermediaryArray = new byte[fd.Width * fd.Height * BYTES_PER_PIXEL];
			// Initialize the bitmap we will be writing pixel data to
			// DPI standard is 96 ppi
			_imageBitmap = new WriteableBitmap(fd.Width, fd.Height, 96, 96, PixelFormats.Bgr32, null);
			PreviewImage.Source = _imageBitmap;
		}

		/// <summary>
		/// Initialization for the body/skeleton stream.
		/// </summary>
		private void InitializeBodyReading() {
			// Set up our local copy of the coordinate mapper
			_coordinateMapper = _kinect.CoordinateMapper;
			_drawingGroup = new DrawingGroup();
			DrawingImage.Source = new DrawingImage(_drawingGroup);

			// Cache the body frame source
			BodyFrameSource bfs = _kinect.BodyFrameSource;
			// Allocate space needed to hold bodies
			_bodies = new Body[bfs.BodyCount];
			_bodyReader = bfs.OpenReader();
		}

		private void LoadMeasurementModels() {
			_measurementModels.Add(new RightElbowModel(_colorFrameWidth, _colorFrameHeight));
			_measurementModels.Add(new LeftElbowModel(_colorFrameWidth, _colorFrameHeight));
			_measurementModels.Add(new RightKneeModel(_colorFrameWidth, _colorFrameHeight));
			_measurementModels.Add(new LeftKneeModel(_colorFrameWidth, _colorFrameHeight));
			_measurementModels.Add(new LowerBackModel(_colorFrameWidth, _colorFrameHeight));
			_measurementModels.Add(new UpperNeckModel(_colorFrameWidth, _colorFrameHeight));
		}

		private void InitializeLayout() {
			//TODO5: remove the following message- AM I A FREAKING PRO?!?!?!?!?!?!?
			for (int i = 0; i < _measurementModels.Count; i++) {
				TestSelectionGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

				var button = new Button();
				var buttonBinding = new Binding("TestName");
				buttonBinding.Source = _measurementModels[i];
				button.SetBinding(Button.ContentProperty, buttonBinding);
				button.SetValue(Grid.RowProperty, i);
				button.Margin = new Thickness(0, 5, 0, 0);
				button.FontSize = 18.0;
				button.Click += LaunchMeasurementModel;

				var textBox = new TextBox();
				var textBoxBinding = new Binding("RecordingResult");
				textBoxBinding.Source = _measurementModels[i];
				textBox.SetBinding(TextBox.TextProperty, textBoxBinding);
				textBox.SetValue(Grid.RowProperty, i);
				textBox.SetValue(Grid.ColumnProperty, 1);
				textBox.Margin = new Thickness(5, 5, 0, 0);
				textBox.FontSize = 18.0;

				TestSelectionGrid.Children.Add(button);
				TestSelectionGrid.Children.Add(textBox);
			}
		}

		void LaunchMeasurementModel(object sender, RoutedEventArgs e) {
			var button = sender as Button;
			int index = -1;
			if (button == null)
				return;

			for (int i = 0; i < _measurementModels.Count; i++) {
				if (button.Content.ToString() == _measurementModels[i].TestName) {
					index = i;
					break;
				}
			}

			_measurementWindow = new AngleMeasurementWindow(_measurementModels[index], index);
			_measurementWindow.ShowDialog();
			_measurementWindow = null;
		}

		/// <summary>
		/// Raised when the window has been loaded.
		/// </summary>
		private void SelectionWindow_Loaded(object sender, RoutedEventArgs e) {
			// If the readers were initialized successfully, then subscribe to the frame arrived events
			if (_colorReader != null)
				_colorReader.FrameArrived += ColorFrame_Arrived;
			if (_bodyReader != null)
				_bodyReader.FrameArrived += BodyFrame_Arrived;
		}

		/// <summary>
		/// Cleanup used resources when the window closes.
		/// </summary>
		private void SelectionWindow_Closing(object sender, CancelEventArgs e) {
			if (_colorReader != null) {
				_colorReader.Dispose();
				_colorReader = null;
			}
			if (_bodyReader != null) {
				_bodyReader.Dispose();
				_bodyReader = null;
			}
			if (_kinect != null) {
				_kinect.Close();
				_kinect = null;
			}
		}

		/// <summary>
		/// Raised when we have received a color frame for usage.
		/// </summary>
		private void ColorFrame_Arrived(object sender, ColorFrameArrivedEventArgs e) {
			ColorFrameReference cfr = e.FrameReference;

			if (cfr == null)
				return;

			var frame = cfr.AcquireFrame();

			if (frame == null)
				return;

			using (frame) {
				FrameDescription fd = frame.FrameDescription;

				if (fd.Width == _imageBitmap.PixelWidth && fd.Height == _imageBitmap.PixelHeight) {
					if (frame.RawColorImageFormat == ColorImageFormat.Bgra) {
						frame.CopyRawFrameDataToArray(_intermediaryArray);
					}
					else {
						frame.CopyConvertedFrameDataToArray(_intermediaryArray, ColorImageFormat.Bgra);
					}

					if (_measurementWindow != null && _measurementWindow.IsActive) {
						_measurementModels[_measurementWindow.ModelIndex].UpdateColorFrame(fd.Width, fd.Height, _intermediaryArray, (int)(fd.Width * BYTES_PER_PIXEL));
					}
					else {
						_imageBitmap.WritePixels(new Int32Rect(0, 0, fd.Width, fd.Height), _intermediaryArray, (int)(fd.Width * BYTES_PER_PIXEL), 0);
					}
				}
			}
		}

		/// <summary>
		/// Raised when we have received a body/skeleton frame for usage.
		/// </summary>
		private void BodyFrame_Arrived(object sender, BodyFrameArrivedEventArgs e) {
			var fr = e.FrameReference;

			if (fr == null)
				return;

			var frame = fr.AcquireFrame();

			if (frame == null)
				return;

			using (frame) {
				frame.GetAndRefreshBodyData(_bodies);

				if (_measurementWindow != null && _measurementWindow.IsActive) {
					var q = _bodies.Where(b => b.IsTracked);
					if (q.Count() > 0) {
						_measurementModels[_measurementWindow.ModelIndex].UpdateBodyFrame(q.First(), _coordinateMapper);
					}
				}
				else {
					using (var dc = _drawingGroup.Open()) {
						dc.DrawRectangle(Brushes.Transparent, null, new Rect(0.0, 0.0, _colorFrameWidth, _colorFrameHeight));

						foreach (var body in _bodies.Where(b => b.IsTracked)) {
							var t = _coordinateMapper.MapCameraPointToColorSpace(body.Joints[JointType.ElbowRight].Position);
							var rightElbowPoint = new Point(t.X, t.Y);
							dc.DrawText(new FormattedText("Right Elbow", CultureInfo.GetCultureInfo("en-us"), System.Windows.FlowDirection.LeftToRight, new Typeface("Segoe UI"), 36, Brushes.Black), rightElbowPoint);


							// Torso
							BodyDrawing.DrawBone(body.Joints[JointType.Head], body.Joints[JointType.Head], _coordinateMapper, dc, 80);
							BodyDrawing.DrawBone(body.Joints[JointType.Neck], body.Joints[JointType.SpineShoulder], _coordinateMapper, dc, 80);
							BodyDrawing.DrawBone(body.Joints[JointType.SpineShoulder], body.Joints[JointType.SpineMid], _coordinateMapper, dc, 80);
							BodyDrawing.DrawBone(body.Joints[JointType.SpineMid], body.Joints[JointType.SpineBase], _coordinateMapper, dc, 80);
							BodyDrawing.DrawBone(body.Joints[JointType.SpineShoulder], body.Joints[JointType.ShoulderRight], _coordinateMapper, dc, 80);
							BodyDrawing.DrawBone(body.Joints[JointType.SpineShoulder], body.Joints[JointType.ShoulderLeft], _coordinateMapper, dc, 80);
							BodyDrawing.DrawBone(body.Joints[JointType.SpineBase], body.Joints[JointType.HipRight], _coordinateMapper, dc, 80);
							BodyDrawing.DrawBone(body.Joints[JointType.SpineBase], body.Joints[JointType.HipLeft], _coordinateMapper, dc, 80);

							// Left Arm
							BodyDrawing.DrawBone(body.Joints[JointType.ShoulderLeft], body.Joints[JointType.ElbowLeft], _coordinateMapper, dc, 80);
							BodyDrawing.DrawBone(body.Joints[JointType.ElbowLeft], body.Joints[JointType.WristLeft], _coordinateMapper, dc, 80);
							BodyDrawing.DrawBone(body.Joints[JointType.WristLeft], body.Joints[JointType.HandLeft], _coordinateMapper, dc, 80);
							BodyDrawing.DrawBone(body.Joints[JointType.HandLeft], body.Joints[JointType.HandTipLeft], _coordinateMapper, dc, 80);
							BodyDrawing.DrawBone(body.Joints[JointType.WristLeft], body.Joints[JointType.ThumbLeft], _coordinateMapper, dc, 80);

							// Right Arm
							BodyDrawing.DrawBone(body.Joints[JointType.ShoulderRight], body.Joints[JointType.ElbowRight], _coordinateMapper, dc, 80);
							BodyDrawing.DrawBone(body.Joints[JointType.ElbowRight], body.Joints[JointType.WristRight], _coordinateMapper, dc, 80);
							BodyDrawing.DrawBone(body.Joints[JointType.WristRight], body.Joints[JointType.HandRight], _coordinateMapper, dc, 80);
							BodyDrawing.DrawBone(body.Joints[JointType.HandRight], body.Joints[JointType.HandTipRight], _coordinateMapper, dc, 80);
							BodyDrawing.DrawBone(body.Joints[JointType.WristRight], body.Joints[JointType.ThumbRight], _coordinateMapper, dc, 80);

							// Left Leg
							BodyDrawing.DrawBone(body.Joints[JointType.HipLeft], body.Joints[JointType.KneeLeft], _coordinateMapper, dc, 80);
							BodyDrawing.DrawBone(body.Joints[JointType.KneeLeft], body.Joints[JointType.AnkleLeft], _coordinateMapper, dc, 80);
							BodyDrawing.DrawBone(body.Joints[JointType.AnkleLeft], body.Joints[JointType.FootLeft], _coordinateMapper, dc, 80);

							// Right Leg
							BodyDrawing.DrawBone(body.Joints[JointType.HipRight], body.Joints[JointType.KneeRight], _coordinateMapper, dc, 80);
							BodyDrawing.DrawBone(body.Joints[JointType.KneeRight], body.Joints[JointType.AnkleRight], _coordinateMapper, dc, 80);
							BodyDrawing.DrawBone(body.Joints[JointType.AnkleRight], body.Joints[JointType.FootRight], _coordinateMapper, dc, 80);
						}
					}
				}
			}
		}
	}
}
