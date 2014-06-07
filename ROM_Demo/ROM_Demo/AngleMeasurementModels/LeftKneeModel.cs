using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ROM_Demo.AngleMeasurementModels {
	class LeftKneeModel : AngleMeasurementModel {
		public LeftKneeModel(int bitmapWidth, int bitmapHeight)
			: base(bitmapWidth, bitmapHeight) {

			this.TestName = "Left Knee";

			this.JointsToMeasure.Add(JointType.HipLeft);
			this.JointsToMeasure.Add(JointType.KneeLeft);
			this.JointsToMeasure.Add(JointType.AnkleLeft);
		}
	}
}
