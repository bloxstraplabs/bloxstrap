using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bloxstrap.Models
{
	public class ReShadeShaderConfig
	{
		// it's assumed that the BaseFolder has a "Textures" folder and a "Shaders" folder
		// the files listed in ExcludedFiles are relative to the BaseFolder

		public string Name { get; set; } = null!;
		public string DownloadLocation { get; set; } = null!;
		public string BaseFolder { get; set; } = "/";
		public List<string> ExcludedFiles { get; set; } = new List<string>();

		public override string ToString() => Name;
	}
}
