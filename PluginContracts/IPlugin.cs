using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.ComponentModel.Composition;

namespace PluginContracts {
	public interface IPlugin {
		string Description { get; }

		string UpdateURL { get; }
		string Website { get; }

		bool Initialize();
		void Shutdown();
		
		bool SetColor(Color c);
	}

	public interface IPluginMetadata {
		string Name { get; }
		string Version { get; }
		string Author { get; }

		string GUID { get; }
	}


}
