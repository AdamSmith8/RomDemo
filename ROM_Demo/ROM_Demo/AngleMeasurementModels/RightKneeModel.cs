using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ROM_Demo.AngleMeasurementModels {
	class RightKneeModel : AngleMeasurementModel {
		public RightKneeModel(int bitmapWidth, int bitmapHeight)
			: base(bitmapWidth, bitmapHeight) {

			this.TestName = "Right Knee";

			this.JointsToMeasure.Add(JointType.HipRight);
			this.JointsToMeasure.Add(JointType.KneeRight);
			this.JointsToMeasure.Add(JointType.AnkleRight);
		}
	}
}
