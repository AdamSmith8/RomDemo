using Microsoft.Kinect;
using ROM_Demo.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ROM_Demo.AngleMeasurementModels {
	class UpperNeckModel : AngleMeasurementModel {
		public UpperNeckModel(int bitmapWidth, int bitmapHeight)
			: base(bitmapWidth, bitmapHeight) {
			
			TestName = "Upper Neck";

			JointsToMeasure.Add(JointType.Head);
			JointsToMeasure.Add(JointType.Neck);
			JointsToMeasure.Add(JointType.SpineShoulder);
		}

		protected override void UpdateAngle(ColorSpacePoint joint1, ColorSpacePoint joint2, ColorSpacePoint joint3) {
			// Negate the Y component since the screen space starts at the top
			var v1 = new Vec2f(joint1.X - joint2.X, -(joint1.Y - joint2.Y));
			var v2 = new Vec2f(0, -1);

			v1.Normalize();
			v2.Normalize();

			int angle = (int)Math.Round(Math.Acos(v1.Dot(v2)) * (180 / Math.PI));
			angle -= 180;
			angle = -angle;

			RecordingResult = (float)angle;
			
			//base.UpdateAngle(joint1, joint2, joint3);
		}
	}
}
