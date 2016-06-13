using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PluginContracts;

namespace LogiRGB.Managers {
	public class PluginManager {

		[ImportMany(typeof(IPlugin))]
		public IEnumerable<Lazy<IPlugin, IPluginMetadata>> Plugins {
			get; set;
		}

		private static string PluginPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LogiRGB", "Plugins");

		public PluginManager() {
			Directory.CreateDirectory(PluginPath);

			// Create the CompositionContainer with all parts in the catalog (links Exports and Imports) 
			var container = new CompositionContainer(new DirectoryCatalog(PluginPath));

			//Fill the imports of this object 
			container.ComposeParts(this);
			
			Debug.WriteLine("Plugin count: " + Plugins.Count().ToString());
		}
	}
}
