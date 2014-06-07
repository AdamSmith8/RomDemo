using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ROM_Demo.AngleMeasurementModels {
	class LeftElbowModel : AngleMeasurementModel {
		public LeftElbowModel(int bitmapWidth, int bitmapHeight)
			: base(bitmapWidth, bitmapHeight) {

			this.TestName = "Left Elbow";

			this.JointsToMeasure.Add(JointType.WristLeft);
			this.JointsToMeasure.Add(JointType.ElbowLeft);
			this.JointsToMeasure.Add(JointType.ShoulderLeft);
		}
	}
}
