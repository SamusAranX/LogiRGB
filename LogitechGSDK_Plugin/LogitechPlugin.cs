using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Drawing;
using PluginContracts;

namespace LogitechGSDK_Plugin {
	//string Name { get; }
	//string Description { get; }
	//string Version { get; }
	//string Author { get; }
	//string Website { get; }
	[Export(typeof(IPlugin))]
	[ExportMetadata("Name", "Logitech RGB LED Plugin")]
	[ExportMetadata("Version", "1.0")]
	[ExportMetadata("Author", "Peter Wunder")]
	[ExportMetadata("GUID", "c8cf0eab - 2bb0 - 4bcb - 8122 - 560281515295")]
	public class LogitechPlugin : IPlugin {

		public string Description {
			get {
				return "This plugin provides color changing capability for all Logitech devices supported by their LED Illumination SDK.\nBasically, if your device has RGB LEDs inside of it, chances are this will support it.";
			}
		}

		public string UpdateURL {
			get {
				return "https://apps.peterwunder.de/logirgb/pluginLogiUpdate.json";
			}
		}


		public string Website {
			get {
				return "http://peterwunder.de";
			}
		}

		public bool Initialize() {
			if (!LogitechGSDK.LogiLedInit())
				return false; // Don't return the result of the following calls. If this one fails, get the heck outta here.

			LogitechGSDK.LogiLedSetTargetDevice(LogitechGSDK.LOGI_DEVICETYPE_ALL);
			LogitechGSDK.LogiLedSaveCurrentLighting();

			return true;
		}

		public bool SetColor(Color c) {
			var r = (int)Math.Min(c.R / 2.55, 100);
			var g = (int)Math.Min(c.G / 2.55, 100);
			var b = (int)Math.Min(c.B / 2.55, 100);

			Debug.WriteLine(this.GetType().Name + ": SetColor");

			if (!LogitechGSDK.LogiLedSetLighting(r, g, b))
				return false;

			return true;
		}

		public void Shutdown() {
			LogitechGSDK.LogiLedRestoreLighting();
			LogitechGSDK.LogiLedShutdown();
		}
	}
}