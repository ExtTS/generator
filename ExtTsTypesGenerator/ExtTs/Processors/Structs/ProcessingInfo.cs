using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtTs.Processors {
	public struct ProcessingInfo {
		public int StagesCount;
		public int StageIndex;
		public string StageName;
		public string InfoText;
		public double Progress;
		public ProcessingInfo (
			int stagesCount = 3, 
			int stageIndex = 0, 
			string stageName = "Initializing",
			string infoText = "...",
			double progress = 0.0
		) {
			this.StagesCount = stagesCount;
			this.StageIndex = stageIndex;
			this.StageName = stageName;
			this.InfoText = infoText;
			this.Progress = progress;
		}
	}
}
