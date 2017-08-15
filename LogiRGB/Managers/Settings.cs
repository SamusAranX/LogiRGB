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
		/// <summary>
		/// This class represents the app's settings.
		/// They get saved and loaded from a JSON file in the user's AppData folder.
		/// </summary>

		public class ColorInfo {
			/// <summary>
			/// A wrapper class 
			/// </summary>

			public byte[] Color { get; set; }
			public byte[] CustomColor { get; set; }
			public bool UsesCustomColor { get; set; }

			public ColorInfo() {}

			/// <summary>
			/// Instantiate a new ColorInfo object with a Color.
			/// </summary>
			/// <param name="color">The System.Drawing.Color object to wrap in the ColorInfo object.</param>
			public ColorInfo(DColor color) {
				this.Color = color.ToByteArray();
			}
		}

		public Dictionary<string, ColorInfo> HashesAndColors;
		
		public byte[] FallbackColor;
		public bool AutostartEnabled;
		//public List<string> ActivePluginGUIDs;

		private static string _settingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LogiRGB", "settings.json");

		/// <summary>
		/// Instantiate a new Settings object. This will also instantiate all properties for convenience.
		/// </summary>
		public Settings() {
			//HashesAndColors = new Dictionary<string, byte[]>();
			HashesAndColors = new Dictionary<string, ColorInfo>();
			FallbackColor = new byte[] { 0, 127, 255 };
			AutostartEnabled = false;
			//ActivePluginGUIDs = new List<string>(new string[] { "C8CF0EAB-2BB0-4BCB-8122-560281515295" }); // The Logitech plugin's GUID. Cheap, I know
		}

		/// <summary>
		/// Parses the settings file and returns it as a Settings object.
		/// </summary>
		/// <returns>The user's settings for LogiRGB.</returns>
		public static Settings LoadSettings() {
			if (!File.Exists(_settingsPath)) {
				Debug.WriteLine("Settings: There are no saved settings yet. Returning a new object.");
				
				return new Settings();
			}

			var serializer = new JavaScriptSerializer();

			using (StreamReader inputFile = new StreamReader(_settingsPath)) {
				var json = inputFile.ReadToEnd();
				try {
					return serializer.Deserialize<Settings>(json);
				} catch (Exception ex) {
					if (ex is ArgumentException || ex is ArgumentNullException) {
						Debug.WriteLine("Settings: Loaded data is corrupt, constructing new Settings object");
						// Return new Settings object is JSON input is invalid or corrupt
						return new Settings();
					}

					// Throw InvalidOperationExceptions
					throw;
				}
			}
		}

		/// <summary>
		/// Saves the Settings object to disk.
		/// </summary>
		/// <returns>True if the settings were saved successfully, False if not.</returns>
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
			}
			return false;
		}

	}
}
