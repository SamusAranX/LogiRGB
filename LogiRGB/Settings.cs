using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

using MColor = System.Windows.Media.Color;
using DColor = System.Drawing.Color;

namespace LogiRGB {
	public class Settings {

		public class ColorInfo {
			public byte[] Color { get; set; }
			public byte[] CustomColor { get; set; }
			public bool UsesCustomColor { get; set; }

			public ColorInfo() {}

			public ColorInfo(DColor color) {
				this.Color = color.ToByteArray();
			}
		}
		public Dictionary<string, ColorInfo> HashesAndColors;

		//public Dictionary<string, byte[]> HashesAndColors;
		public byte[] FallbackColor;
		public bool AutostartEnabled;
		//public string[] ActivePluginGUIDs;
		public List<string> ActivePluginGUIDs;

		private static string _settingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LogiRGB", "settings.json");

		/// <summary>
		/// Instantiate a new Settings object. This will also instantiate all properties for convenience.
		/// </summary>
		public Settings() {
			//HashesAndColors = new Dictionary<string, byte[]>();
			HashesAndColors = new Dictionary<string, ColorInfo>();
			FallbackColor = new byte[] { 0, 127, 255 };
			AutostartEnabled = false;
			ActivePluginGUIDs = new List<string>(new string[] { "C8CF0EAB-2BB0-4BCB-8122-560281515295" }); // The Logitech plugin's GUID. Cheap, I know
		}

		public static Settings LoadSettings() {
			if (!File.Exists(_settingsPath)) {
				Debug.WriteLine("There are no saved settings yet. Returning a new object.");
				
				return new Settings();
			}

			var serializer = new JavaScriptSerializer();

			using (StreamReader inputFile = new StreamReader(_settingsPath)) {
				var json = inputFile.ReadToEnd();
				try {
					return serializer.Deserialize<Settings>(json);
				} catch (Exception ex) {
					if (ex is ArgumentException || ex is ArgumentNullException) {
						// Return new Settings object is JSON input is invalid or corrupt
						return new Settings();
					}

					// Throw InvalidOperationExceptions
					throw;
				}
			}
		}

		public bool SaveSettings() {
			var serializer = new JavaScriptSerializer();
			
			Directory.CreateDirectory(Path.GetDirectoryName(_settingsPath));

			try {
				using (StreamWriter outputFile = new StreamWriter(_settingsPath, false)) {
					var json = serializer.Serialize(this);
					outputFile.Write(json);

					return true;
				}
			} catch (Exception ex) {
				Debug.WriteLine(ex.Message);
				return false;
			}
		}

	}
}
