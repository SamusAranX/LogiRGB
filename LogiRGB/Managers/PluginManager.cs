using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
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

			var files = Directory.EnumerateFiles(PluginPath, "*.dll", SearchOption.TopDirectoryOnly);
			var goodPlugins = new AggregateCatalog();

			foreach (var file in files) {
				try {
					var catalog = new AssemblyCatalog(file);

					// Load assembly and check plugin count
					if (catalog.Parts.ToList().Count > 0)
						goodPlugins.Catalogs.Add(catalog);
					else
						Debug.WriteLine($"{file} doesn't contain any plugins");
				} catch (BadImageFormatException) {
					// Assembly is not a .NET assembly
					Debug.WriteLine($"{file} is not a .NET assembly");
				} catch (ReflectionTypeLoadException) {
					// Assembly is either not a plugin or uses a wrong version of the plugin interface
					Debug.WriteLine($"{file} is not a valid plugin");
				}
			}

			// Create the CompositionContainer with all parts in the catalog (links Exports and Imports)
			var container = new CompositionContainer(goodPlugins);

			//Fill the imports of this object 
			container.ComposeParts(this);
			
			Debug.WriteLine("Plugin count: " + Plugins.Count().ToString());
		}
	}
}
