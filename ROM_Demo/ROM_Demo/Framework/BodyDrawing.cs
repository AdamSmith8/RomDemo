using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace ROM_Demo.Framework {
	class BodyDrawing {
		public static void DrawBone(Joint joint1, Joint joint2, CoordinateMapper coordinateMapper, DrawingContext dc, byte alpha) {
			if (joint1.TrackingState == TrackingState.NotTracked || joint2.TrackingState == TrackingState.NotTracked) {
				return;
			}
			if (joint1.TrackingState == TrackingState.Inferred && joint2.TrackingState == TrackingState.Inferred) {
				return;
			}

			var p1 = coordinateMapper.MapCameraPointToColorSpace(joint1.Position);
			var p2 = coordinateMapper.MapCameraPointToColorSpace(joint2.Position);

			if (p1.X < 0 || p1.Y < 0 || p2.X < 0 || p2.Y < 0) {
				return;
			}

			var trackedJointBrush = new SolidColorBrush(Color.FromArgb(alpha, 0, 255, 0));
			var inferredJointBrush = new SolidColorBrush(Color.FromArgb(alpha, 255, 255, 0));
			var trackedBonePen = new Pen(trackedJointBrush, 10);
			var inferredBonePen = new Pen(inferredJointBrush, 10);

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
	}
}
