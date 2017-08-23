using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LogiRGB.Managers.SDK_Wrappers;

namespace LogiRGB.Managers.Plugins {
	[Export(typeof(IPlugin))]
	[ExportMetadata("Name", "Logitech LED Illumination Plugin")]
	[ExportMetadata("Author", "Peter Wunder")]
	[ExportMetadata("Version", "1.0")]
	[ExportMetadata("GUID", "C8CF0EAB-2BB0-4BCB-8122-560281515295")]
	[ExportMetadata("Description", "This plugin provides color changing capability for all Logitech devices supported by their LED Illumination SDK.\nBasically, if your device has RGB LEDs inside of it, chances are this will support it.")]
	[ExportMetadata("UpdateURL", "https://apps.peterwunder.de/logirgb/pluginLogiUpdate.json")]
	[ExportMetadata("Website", "https://peterwunder.de")]
	[Obsolete("This is only here for historical purposes. Use the ColorManager instead.", true)]
	public class LogitechPlugin : IPlugin {
		public bool Initialize() {
			Debug.WriteLine(this.GetType().Name + ": Initialize");

			if (!LogitechGSDK.LogiLedInit())
				return false; // Don't return the result of the following calls. If this one fails, get the heck outta here.

			// Only target devices that support RGB lighting
			LogitechGSDK.LogiLedSetTargetDevice(LogitechGSDK.LOGI_DEVICETYPE_PERKEY_RGB | LogitechGSDK.LOGI_DEVICETYPE_RGB);
			LogitechGSDK.LogiLedSaveCurrentLighting();

			return true;
		}

		// This will always return false after doing Init() -> ... -> Shutdown() -> Init() again -> SetColor()
		// So basically, after disabling and re-enabling this plugin in the settings
		public bool SetColor(Color c) {
			var col = c;

			var r = (int)Math.Min(col.R / 2.55, 100);
			var g = (int)Math.Min(col.G / 2.55, 100);
			var b = (int)Math.Min(col.B / 2.55, 100);

			Debug.WriteLine(this.GetType().Name + ": SetColor " + col.ToString());

			return LogitechGSDK.LogiLedSetLighting(r, g, b);
		}

		public void Shutdown() {
			Debug.WriteLine(this.GetType().Name + ": Shutdown");

			LogitechGSDK.LogiLedRestoreLighting();
			LogitechGSDK.LogiLedShutdown();
		}
	}
}
