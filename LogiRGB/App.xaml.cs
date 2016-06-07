using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Hardcodet.Wpf.TaskbarNotification;

using DColor = System.Drawing.Color;
using DSize = System.Drawing.Size;

namespace LogiRGB {
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application {

		public ColorManager colorManager;
		public FocusWatcher focusWatcher;

		private TaskbarIcon taskbarIcon;
		public Settings settings;

		private void Application_Startup(object sender, StartupEventArgs e) {
			taskbarIcon = (TaskbarIcon)FindResource("taskbarIcon");

			settings = Settings.LoadSettings();

			colorManager = new ColorManager(settings.FallbackColor.ToColor());
			colorManager.ColorChanged += ColorManager_ColorChanged;

			focusWatcher = new FocusWatcher();
			focusWatcher.FocusChanged += FocusWatcher_FocusChanged;
			focusWatcher.StartWatching();

			SettingsWindow settingsWindow = new SettingsWindow();
			settingsWindow.Show();

			//if(!IsAdministrator()) {
			//	MessageBox.Show("Without administrator privileges, LogiRGB won't be able to read some applications' data.\nYou don't have to grant it these privileges, but without them, some applications will not trigger a color change.", "LogiRGB", MessageBoxButton.OK, MessageBoxImage.Information);
			//}
		}

		private void ColorManager_ColorChanged(object sender, ColorChangedEventArgs e) {
			Debug.WriteLine("Color changed: " + e.NewColor.ToString());
		}

		private void FocusWatcher_FocusChanged(object sender, FocusChangedEventArgs e) {
			using (FileStream fs = File.OpenRead(e.Filename))
			using (SHA1 sha1 = SHA1.Create()) {
				var checksum = sha1.ComputeHash(fs);
				var strChecksum = BitConverter.ToString(checksum).Replace("-", string.Empty).ToLowerInvariant();

				if (settings.HashesAndColors.ContainsKey(strChecksum)) {
					var colorBytes = settings.HashesAndColors[strChecksum];
					var colorNum = (int)Math.Floor(colorBytes.Length / 3.0); // The 3 is a float because Visual Studio is stupid

					DColor[] colors = new DColor[colorNum];
					for (int i = 0; i < colorNum; i++) {
						colors[i] = ((byte[])colorBytes.Skip(i * 3).Take(3)).ToColor();
					}

					Debug.WriteLine("Dictionary hit! " + settings.HashesAndColors[strChecksum].ToColor().ToString());

					colorManager.SetColor(settings.HashesAndColors[strChecksum].ToColor(), colors);
				} else {
					var iconBitmap = Helpers.GetEXEIconBitmap(e.Filename);
					if (iconBitmap.Size.Width > 128) {
						iconBitmap = iconBitmap.Resize(new DSize(128, 128));
						Debug.WriteLine("Icon is bigger than 128x128, resizing.");
					}

					var colors = Helpers.AnalyzeImage(iconBitmap, 32);

					byte[] colorBytes = new byte[] { };
					foreach (var c in colors) {
						colorBytes = colorBytes.Concat(new byte[] { c.R, c.G, c.B }).ToArray();
					}
					settings.HashesAndColors[strChecksum] = colorBytes;

					Debug.WriteLine($"New colors for {e.Filename}: " + string.Join(", ", colors.Select(c => c.ToString())));

					colorManager.SetColor(colors[0], colors.Skip(1).ToArray());
				}
			}
		}



		private void Application_Exit(object sender, ExitEventArgs e) {
			colorManager.ColorChanged -= ColorManager_ColorChanged;

			focusWatcher.StopWatching();
			focusWatcher.FocusChanged -= FocusWatcher_FocusChanged;
		}
	}
}
