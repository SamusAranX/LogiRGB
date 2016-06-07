using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace LogiRGB {
	public class Settings {

		private static string _settingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LogiRGB", "settings.json");

		public Dictionary<string, byte[]> HashesAndColors;
		public byte[] FallbackColor;

		/// <summary>
		/// Instantiate a new Settings object. This will also instantiate all properties for convenience.
		/// </summary>
		public Settings() {
			HashesAndColors = new Dictionary<string, byte[]>();
			FallbackColor = new byte[] { 0, 127, 255 };
		}

		public static Settings LoadSettings() {
			if (!File.Exists(_settingsPath)) {
				Debug.WriteLine("There are no saved settings yet. Returning a new object.");
				
				return new Settings();
			}

			var serializer = new JavaScriptSerializer();

			using (StreamReader inputFile = new StreamReader(_settingsPath)) {
				var json = inputFile.ReadToEnd();
				return serializer.Deserialize<Settings>(json);
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
