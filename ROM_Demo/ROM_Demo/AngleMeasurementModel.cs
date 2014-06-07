using Microsoft.Kinect;
using ROM_Demo.Framework;
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

		private DrawingGroup _drawingGroup;

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
			_drawingGroup = new DrawingGroup();
			_bodyImageSource = new DrawingImage(_drawingGroup);
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

		public void UpdateBodyFrame(Body body, CoordinateMapper coordinateMapper) {
			using (var dc = _drawingGroup.Open()) {
				dc.DrawRectangle(Brushes.Transparent, null, new Rect(0, 0, 1920, 1080));

				// Torso
				BodyDrawing.DrawBone(body.Joints[JointType.Head], body.Joints[JointType.Head], coordinateMapper, dc, 20);
				BodyDrawing.DrawBone(body.Joints[JointType.Neck], body.Joints[JointType.SpineShoulder], coordinateMapper, dc, 20);
				BodyDrawing.DrawBone(body.Joints[JointType.SpineShoulder], body.Joints[JointType.SpineMid], coordinateMapper, dc, 20);
				BodyDrawing.DrawBone(body.Joints[JointType.SpineMid], body.Joints[JointType.SpineBase], coordinateMapper, dc, 20);
				BodyDrawing.DrawBone(body.Joints[JointType.SpineShoulder], body.Joints[JointType.ShoulderRight], coordinateMapper, dc, 20);
				BodyDrawing.DrawBone(body.Joints[JointType.SpineShoulder], body.Joints[JointType.ShoulderLeft], coordinateMapper, dc, 20);
				BodyDrawing.DrawBone(body.Joints[JointType.SpineBase], body.Joints[JointType.HipRight], coordinateMapper, dc, 20);
				BodyDrawing.DrawBone(body.Joints[JointType.SpineBase], body.Joints[JointType.HipLeft], coordinateMapper, dc, 20);

				// Left Arm
				BodyDrawing.DrawBone(body.Joints[JointType.ShoulderLeft], body.Joints[JointType.ElbowLeft], coordinateMapper, dc, 20);
				BodyDrawing.DrawBone(body.Joints[JointType.ElbowLeft], body.Joints[JointType.WristLeft], coordinateMapper, dc, 20);
				BodyDrawing.DrawBone(body.Joints[JointType.WristLeft], body.Joints[JointType.HandLeft], coordinateMapper, dc, 20);
				BodyDrawing.DrawBone(body.Joints[JointType.HandLeft], body.Joints[JointType.HandTipLeft], coordinateMapper, dc, 20);
				BodyDrawing.DrawBone(body.Joints[JointType.WristLeft], body.Joints[JointType.ThumbLeft], coordinateMapper, dc, 20);

				// Right Arm
				BodyDrawing.DrawBone(body.Joints[JointType.ShoulderRight], body.Joints[JointType.ElbowRight], coordinateMapper, dc, 20);
				BodyDrawing.DrawBone(body.Joints[JointType.ElbowRight], body.Joints[JointType.WristRight], coordinateMapper, dc, 20);
				BodyDrawing.DrawBone(body.Joints[JointType.WristRight], body.Joints[JointType.HandRight], coordinateMapper, dc, 20);
				BodyDrawing.DrawBone(body.Joints[JointType.HandRight], body.Joints[JointType.HandTipRight], coordinateMapper, dc, 20);
				BodyDrawing.DrawBone(body.Joints[JointType.WristRight], body.Joints[JointType.ThumbRight], coordinateMapper, dc, 20);

				// Left Leg
				BodyDrawing.DrawBone(body.Joints[JointType.HipLeft], body.Joints[JointType.KneeLeft], coordinateMapper, dc, 20);
				BodyDrawing.DrawBone(body.Joints[JointType.KneeLeft], body.Joints[JointType.AnkleLeft], coordinateMapper, dc, 20);
				BodyDrawing.DrawBone(body.Joints[JointType.AnkleLeft], body.Joints[JointType.FootLeft], coordinateMapper, dc, 20);

				// Right Leg
				BodyDrawing.DrawBone(body.Joints[JointType.HipRight], body.Joints[JointType.KneeRight], coordinateMapper, dc, 20);
				BodyDrawing.DrawBone(body.Joints[JointType.KneeRight], body.Joints[JointType.AnkleRight], coordinateMapper, dc, 20);
				BodyDrawing.DrawBone(body.Joints[JointType.AnkleRight], body.Joints[JointType.FootRight], coordinateMapper, dc, 20);

				for (int i = 0; i < JointsToMeasure.Count; i++) {
					if (i == 0) {
						continue;
					}
					BodyDrawing.DrawBone(body.Joints[JointsToMeasure[i]], body.Joints[JointsToMeasure[i - 1]], coordinateMapper, dc, 255);
				}
			}

			foreach (var jointType in JointsToMeasure) {
				if (body.Joints[jointType].TrackingState == TrackingState.NotTracked) {
					return;
				}
			}
			if (JointsToMeasure.Count == 3) {
				var p1 = coordinateMapper.MapCameraPointToColorSpace(body.Joints[JointsToMeasure[0]].Position);
				var p2 = coordinateMapper.MapCameraPointToColorSpace(body.Joints[JointsToMeasure[1]].Position);
				var p3 = coordinateMapper.MapCameraPointToColorSpace(body.Joints[JointsToMeasure[2]].Position);

				UpdateAngle(p1, p2, p3);
			}
		}

		protected virtual void UpdateAngle(ColorSpacePoint joint1, ColorSpacePoint joint2, ColorSpacePoint joint3) {
			// Negate the Y component since the screen space starts at the top
			var v1 = new Vec2f(joint1.X - joint2.X, -(joint1.Y - joint2.Y));
			var v2 = new Vec2f(joint3.X - joint2.X, -(joint3.Y - joint2.Y));

			v1.Normalize();
			v2.Normalize();

			int angle = (int)Math.Round(Math.Acos(v1.Dot(v2)) * (180 / Math.PI));
			angle -= 180;
			angle = -angle;

			RecordingResult = (float)angle;
		}
	}
}
