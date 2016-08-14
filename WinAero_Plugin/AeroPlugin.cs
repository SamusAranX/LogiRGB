using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Runtime.InteropServices;
using PluginContracts;

namespace WinAero_Plugin {

	[Export(typeof(IPlugin))]
	[ExportMetadata("Name", "Windows Aero Plugin")]
	[ExportMetadata("Author", "Peter Wunder")]
	[ExportMetadata("Version", "1.0")]
	[ExportMetadata("GUID", "B45CC307-02B2-4AE7-B415-DAE17EB1CB7B")]
	[ExportMetadata("Description", "This plugin enables LogiRGB to change your system's Aero color.\nWindows 7+ and enabled Aero themes required.")]
	[ExportMetadata("UpdateURL", "https://apps.peterwunder.de/logirgb/pluginAeroUpdate.json")]
	[ExportMetadata("Website", "https://peterwunder.de")]
	public class AeroPlugin : IPlugin {
		#region Windows Aero APIs
		public struct DWM_COLORIZATION_PARAMS {
			public uint clrColor;
			public uint clrAfterGlow;
			public uint nIntensity;
			public uint clrAfterGlowBalance;
			public uint clrBlurBalance;
			public uint clrGlassReflectionIntensity;
			public bool fOpaque;
		}

		[DllImport("dwmapi.dll", EntryPoint = "#127", PreserveSig = false)]
		private static extern void DwmGetColorizationParameters(out DWM_COLORIZATION_PARAMS parameters);

		[DllImport("dwmapi.dll", EntryPoint = "#131", PreserveSig = false)]
		private static extern void DwmSetColorizationParameters(ref DWM_COLORIZATION_PARAMS parameters, bool unknown);

		[DllImport("dwmapi.dll", PreserveSig = false)]
		public static extern bool DwmIsCompositionEnabled();

		// Helper method to convert from a Win32 BGRA-format color to a .NET color.
		private static Color BgraToColor(uint color) {
			return Color.FromArgb(int.Parse(color.ToString("X"), NumberStyles.HexNumber));
		}

		// Helper method to convert from a .NET color to a Win32 BGRA-format color.
		private static uint ColorToBgra(Color color) {
			return (uint)(color.B | (color.G << 8) | (color.R << 16) | (color.A << 24));
		}
		#endregion

		#region IPlugin Methods
		private DWM_COLORIZATION_PARAMS RestoreParameters;

		public bool Initialize() {
			Debug.WriteLine(this.GetType().Name + ": Initialize");

			DwmGetColorizationParameters(out RestoreParameters);

			return true;
		}

		public bool SetColor(Color c) {
			Debug.WriteLine(this.GetType().Name + ": SetColor");

			DWM_COLORIZATION_PARAMS colParams;
			DwmGetColorizationParameters(out colParams);
			colParams.clrColor = ColorToBgra(c);

			DwmSetColorizationParameters(ref colParams, false);

			return true;
		}

		public void Shutdown() {
			Debug.WriteLine(this.GetType().Name + ": Shutdown");

			DwmSetColorizationParameters(ref RestoreParameters, false);
		}
		#endregion

	}
}
