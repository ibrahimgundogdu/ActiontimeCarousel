using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Actiontime.Models.ResultModel
{
	public partial class Result
	{
		public int Success { get; set; } = 0;
		public string Message { get; set; } = string.Empty;

	}

	public partial class AppResult
	{
		public bool Success { get; set; } = false;
		public string Message { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;

	}

	public partial class ConfirmResult : Result
	{
		public int Process { get; set; } = 2002;
		public string QRCode { get; set; }
		public string ConfirmNumber { get; set; }
		public string SerialNumber { get; set; }
		public string Title { get; set; }
		public int LEDTemplate { get; set; }
	}

	public partial class StartResult : Result
	{
		public int Process { get; set; } = 2002;
		public string ConfirmNumber { get; set; }
		public string Title { get; set; }
		public int LEDTemplate { get; set; }
		public int Duration { get; set; }
		public string SerialNumber { get; set; }
	}

	//CompleteResult
	public partial class CompleteResult : Result
	{
		public int Process { get; set; } = 2002;
		public string ConfirmNumber { get; set; }
		public string Title { get; set; }
		public int LEDTemplate { get; set; }
		public string SerialNumber { get; set; }
	}




	public partial class ReaderResult
	{
		public int Process { get; set; } = 1002;
		public int DurationTime { get; set; } = 180;
		public int TriggerTime { get; set; } = 1;
		public string SerialNumber { get; set; }

	}

	public partial class DrawerResult
	{
		public int Process { get; set; } = 1002;
		public string SerialNumber { get; set; }

	}


	public partial class WebSocketResult : Result
	{
		public string Process { get; set; }
		public string ProcessTime { get; set; }
		public string ConfirmNumber { get; set; }
        public int LocationId { get; set; }
    }

}


