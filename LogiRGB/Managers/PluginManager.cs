using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PluginContracts;

namespace LogiRGB.Managers {
	public class PluginManager {

		[ImportMany]
		public IEnumerable<IPlugin> Plugins {
			get; set;
		}

		private static string PluginPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LogiRGB", "Plugins");

		public PluginManager() {
			DirectoryCatalog directoryCatalog = new DirectoryCatalog(PluginPath);

			//An aggregate catalog that combines multiple catalogs 
			var catalog = new AggregateCatalog(directoryCatalog);

			// Create the CompositionContainer with all parts in the catalog (links Exports and Imports) 
			var container = new CompositionContainer(catalog);

			//Fill the imports of this object 
			container.ComposeParts(this);
		}
	}
}
