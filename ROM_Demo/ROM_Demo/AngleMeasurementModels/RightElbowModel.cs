using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ROM_Demo.AngleMeasurementModels {
	class RightElbowModel : AngleMeasurementModel {
		public RightElbowModel(int bitmapWidth, int bitmapHeight) 
			:base(bitmapWidth, bitmapHeight) {

			this.TestName = "Right Elbow";

			this.JointsToMeasure.Add(JointType.WristRight);
			this.JointsToMeasure.Add(JointType.ElbowRight);
			this.JointsToMeasure.Add(JointType.ShoulderRight);
		}
	}
}
