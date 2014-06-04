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
using Microsoft.Kinect;
using System.ComponentModel;
using ROM_Demo.Framework;

namespace ROM_Demo {
	public partial class MainWindow : Window {
		#region Fields
		KinectSensor kinect;

		// Color Fields
		readonly int bytesPerPixel = (PixelFormats.Bgr32.BitsPerPixel + 7) / 8;
		ColorFrameReader colorReader;
		byte[] intermediaryArray;
		WriteableBitmap bitmap;

		// Body Fields
		BodyFrameReader bodyReader;
		Body[] bodies;
		CoordinateMapper coordinateMapper;
		DrawingGroup drawingGroup;
		int colorSpaceWidth;
		int colorSpaceHeight;
		
		// Brushes
		readonly Brush trackedJointBrush = new SolidColorBrush(Color.FromArgb(150, 0, 255, 0));
		readonly Brush inferredJointBrush = new SolidColorBrush(Color.FromArgb(150, 255, 255, 0));
		readonly Pen trackedBonePen = new Pen(new SolidColorBrush(Color.FromArgb(150, 0, 255, 0)), 35);
		readonly Pen inferredBonePen = new Pen(new SolidColorBrush(Color.FromArgb(150, 255, 255, 0)), 35);
		#endregion

		public MainWindow() {
			InitializeComponent();

			InitializeKinect();
		}

		void InitializeKinect() {
			kinect = KinectSensor.Default;

			if (kinect == null) {
				MessageBox.Show("Kinect not found. Please reconnect device and try again.");
				this.Close();
			}

			kinect.Open();

			InitializeColorReading();
			InitializeBodyReading();
		}

		void InitializeColorReading() {
			var frameDescription = kinect.ColorFrameSource.FrameDescription;

			colorSpaceWidth = frameDescription.Width;
			colorSpaceHeight = frameDescription.Height;

			colorReader = kinect.ColorFrameSource.OpenReader();
			intermediaryArray = new byte[frameDescription.Width * frameDescription.Height * bytesPerPixel];
			bitmap = new WriteableBitmap(frameDescription.Width, frameDescription.Height, 96, 96, PixelFormats.Bgr32, null);
			ColorStreamImage.Source = bitmap;
		}

		void InitializeBodyReading() {
			coordinateMapper = kinect.CoordinateMapper;
			drawingGroup = new DrawingGroup();
			SkeletonStreamImage.Source = new DrawingImage(drawingGroup);

			var bodyFrameSource = kinect.BodyFrameSource;

			bodies = new Body[bodyFrameSource.BodyCount];
			bodyReader = kinect.BodyFrameSource.OpenReader();
		}

		private void MainWindow_Loaded(object sender, RoutedEventArgs e) {
			if (colorReader != null)
				colorReader.FrameArrived += ColorFrame_Arrived;
			if (bodyReader != null)
				bodyReader.FrameArrived += BodyFrame_Arrived;
		}

		private void MainWindow_Closing(object sender, CancelEventArgs e) {
			if (colorReader != null) {
				colorReader.Dispose();
				colorReader = null;
			}
			if (bodyReader != null) {
				bodyReader.Dispose();
				bodyReader = null;
			}
			if (kinect != null) {
				kinect.Close();
				kinect = null;
			}
		}

		void ColorFrame_Arrived(object sender, ColorFrameArrivedEventArgs e) {
			var frameReference = e.FrameReference;
			
			if (frameReference == null)
				return;

			var frame = frameReference.AcquireFrame();

			if (frame == null)
				return;
			
			using (frame) {
				var frameDescription = frame.FrameDescription;

				if (frameDescription.Width == bitmap.PixelWidth && frameDescription.Height == bitmap.PixelHeight) {
					if (frame.RawColorImageFormat == ColorImageFormat.Bgra) {
						frame.CopyRawFrameDataToArray(intermediaryArray);
					}
					else {
						frame.CopyConvertedFrameDataToArray(intermediaryArray, ColorImageFormat.Bgra);
					}

					bitmap.WritePixels(new Int32Rect(0, 0, frameDescription.Width, frameDescription.Height), intermediaryArray, (int)(frameDescription.Width * bytesPerPixel), 0);
				}
			}
		}

		void BodyFrame_Arrived(object sender, BodyFrameArrivedEventArgs e) {
			var frameReference = e.FrameReference;

			if (frameReference == null)
				return;

			var frame = frameReference.AcquireFrame();

			if (frame == null)
				return;

			using (frame) {
				frame.GetAndRefreshBodyData(bodies);

				using (var dc = drawingGroup.Open()) {
					dc.DrawRectangle(Brushes.Transparent, null, new Rect(0.0, 0.0, colorSpaceWidth, colorSpaceHeight));

					foreach (var body in bodies) {
						if (body.IsTracked) {
							var rShoulder = body.Joints[JointType.ShoulderRight];
							var rElbow = body.Joints[JointType.ElbowRight];
							var rWrist = body.Joints[JointType.HandRight];

							DrawBone(rShoulder, rElbow, dc);
							DrawBone(rElbow, rWrist, dc);

							UpdateAngle(rShoulder, rElbow, rWrist);
						}
					}
				}
			}
		}

		void DrawBone(Joint joint1, Joint joint2, DrawingContext dc) {
			if (joint1.TrackingState == TrackingState.NotTracked || joint2.TrackingState == TrackingState.NotTracked) {
				return;
			}

			var p1 = coordinateMapper.MapCameraPointToColorSpace(joint1.Position);
			var p2 = coordinateMapper.MapCameraPointToColorSpace(joint2.Position);

			if (joint1.TrackingState == TrackingState.Inferred || joint2.TrackingState == TrackingState.Inferred) {
				dc.DrawLine(inferredBonePen, new Point(p1.X, p1.Y), new Point(p2.X, p2.Y));
			}
			else {
				dc.DrawLine(trackedBonePen, new Point(p1.X, p1.Y), new Point(p2.X, p2.Y));
			}

			if (joint1.TrackingState == TrackingState.Tracked) {
				dc.DrawEllipse(trackedJointBrush, null, new Point(p1.X, p1.Y), 18.0, 18.0);
			}
			else {
				dc.DrawEllipse(inferredJointBrush, null, new Point(p1.X, p1.Y), 18.0, 18.0);
			}

			if (joint2.TrackingState == TrackingState.Tracked) {
				dc.DrawEllipse(trackedJointBrush, null, new Point(p2.X, p2.Y), 18.0, 18.0);
			}
			else {
				dc.DrawEllipse(inferredJointBrush, null, new Point(p2.X, p2.Y), 18.0, 18.0);
			}
		}

		void UpdateAngle(Joint rShoulder, Joint rElbow, Joint rWrist) {
			if(rShoulder.TrackingState == TrackingState.NotTracked ||
				rElbow.TrackingState == TrackingState.NotTracked ||
				rWrist.TrackingState == TrackingState.NotTracked) {

				return;
			}

			var shoulderPoint = coordinateMapper.MapCameraPointToColorSpace(rShoulder.Position);
			var elbowPoint = coordinateMapper.MapCameraPointToColorSpace(rElbow.Position);
			var wristPoint = coordinateMapper.MapCameraPointToColorSpace(rWrist.Position);

			// Negate the Y component since the screen space starts at the top
			var v1 = new Vec2f(shoulderPoint.X - elbowPoint.X, -(shoulderPoint.Y - elbowPoint.Y));
			var v2 = new Vec2f(wristPoint.X - elbowPoint.X, -(wristPoint.Y - elbowPoint.Y));

			v1.Normalize();
			v2.Normalize();

			int angle = (int)Math.Round(Math.Acos(v1.Dot(v2)) * (180 / Math.PI));
			angle -= 180;
			angle = -angle;

			AngleTextBlock.Text = angle.ToString();
		}
	}
}
