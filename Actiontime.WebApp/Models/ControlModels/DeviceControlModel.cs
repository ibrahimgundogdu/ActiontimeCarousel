using Actiontime.Data.Entities;
using Actiontime.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Actiontime.WebApp
{
	public class DeviceControlModel
	{
		public OurLocation Location { get; set; }
		public List<LocationPartial> LocationPartials { get; set; }
		public List<Qrreader> QRReaders { get; set; }
		public Qrreader QRReader { get; set; }

		public int PartId { get; set; }
		public IEnumerable<SelectListItem> Items { get; set; }
	}
}
