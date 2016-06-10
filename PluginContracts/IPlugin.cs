using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace PluginContracts {
	public interface IPlugin {
		Guid GUID { get; }

		string Name { get; }
		string Description { get; }
		Version version { get; }
		string Author { get; }
		string Website { get; }

		string UpdateURL { get; }
		string UpdateWebSite { get; }

		bool Init();
		bool Shutdown();
		
		bool SetColor(Color c);
	}
}
