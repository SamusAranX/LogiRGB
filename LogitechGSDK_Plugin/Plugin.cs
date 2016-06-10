using System;
using System.ComponentModel.Composition;
using System.Drawing;
using PluginContracts;

namespace Plugin {
	[Export(typeof(IPlugin))]
	public class Plugin : IPlugin {
		public Guid GUID { get { return new Guid("F8E24D27-9D2F-4775-A6CE-16B7FE69A841"); } }

		public string Author {
			get {
				return "Peter Wunder";
			}
		}

		public string Description {
			get {
				return "This plugin provides color changing capability for all Logitech devices supported by their LED Illumination SDK.\nBasically, if your device has RGB LEDs inside of it, chances are this will support it.";
			}
		}

		public string Name {
			get {
				return "Logitech LED Illumination SDK";
			}
		}

		public string UpdateURL {
			get {
				throw new NotImplementedException();
			}
		}

		public string UpdateWebSite {
			get {
				throw new NotImplementedException();
			}
		}

		public Version version {
			get {
				return new Version(1, 0);
			}
		}

		public string Website {
			get {
				return "http://peterwunder.de";
			}
		}

		public bool Init() {
			throw new NotImplementedException();
		}

		public bool SetColor(Color c) {
			throw new NotImplementedException();
		}

		public bool Shutdown() {
			throw new NotImplementedException();
		}
	}
}