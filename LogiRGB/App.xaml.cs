using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Input;

using Hardcodet.Wpf.TaskbarNotification;
using LogiRGB.Managers;
using PluginContracts;

using DColor = System.Drawing.Color;
using DSize = System.Drawing.Size;

namespace LogiRGB {
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application {

		public ColorManager colorManager;
		public FocusWatcher focusWatcher;
		public PluginManager pluginManager;

		private TaskbarIcon taskbarIcon;
		public Settings settings;

		private void Application_Startup(object sender, StartupEventArgs e) {
			taskbarIcon = (TaskbarIcon)FindResource("taskbarIcon");

			pluginManager = new PluginManager();

			settings = Settings.LoadSettings();
			Debug.WriteLine(string.Join(", ", settings.ActivePluginGUIDs));

			colorManager = new ColorManager(Helpers.ByteArrayToColor(settings.FallbackColor));
			colorManager.InitializeSDKs();
			//colorManager.ColorChanged += ColorManager_ColorChanged;

			focusWatcher = new FocusWatcher();
			focusWatcher.FocusChanged += FocusWatcher_FocusChanged;
			focusWatcher.StartWatching();

			var containsInvalidGUIDs = settings.ActivePluginGUIDs.Select(ap => Tuple.Create(ap, pluginManager.Plugins.Select(p => p.Metadata.GUID).Contains(ap)));
			if (pluginManager.Plugins.Count() == 0) {
				// Checking if there are plugins loaded - if not, exit.
				taskbarIcon.ShowBalloonTip("LogiRGB", "No plugins found.\nExiting.", BalloonIcon.Warning);
				App.Current.Shutdown();
			} else if (containsInvalidGUIDs.All(x => x.Item2)) {
				// With this insanely longuerythatdeleted LINQ q hcheckingave, wewhether there are GUIDs in ActivePluginGUIDs that are also not in the actual plugin list
				// That way we know if there are any plugins  been're  deleted between restarts of LogiRGB	
				// False means that the plugin has been 

				// Get a list of GUIDs of plugins that still exist
				var newActivePluginGUIDs = settings.ActivePluginGUIDs.Where(g => containsInvalidGUIDs.Single(t => t.Item1 == g).Item2);

				settings.ActivePluginGUIDs = newActivePluginGUIDs.ToArray();
			}

			//if(!IsAdministrator()) {
			//	MessageBox.Show("Without administrator privileges, LogiRGB won't be able to read some applications' data.\nYou don't have to grant it these privileges, but without them, some applications will not trigger a color change.", "LogiRGB", MessageBoxButton.OK, MessageBoxImage.Information);
			//}
		}

		private void About_Click(object sender, RoutedEventArgs e) {
			Debug.WriteLine("Display About window");
		}

		private void Exit_Click(object sender, RoutedEventArgs e) {
			Current.Shutdown();
		}

		//private void ColorManager_ColorChanged(object sender, ColorChangedEventArgs e) {
		//	Debug.WriteLine("Color changed: " + e.NewColor.ToString());
		//}

		private void FocusWatcher_FocusChanged(object sender, FocusChangedEventArgs e) {
			Debug.WriteLine(e.Filename);

			if (e.Filename == Assembly.GetExecutingAssembly().Location || e.Filename.EndsWith("LogiRGB.vshost.exe"))
				return; // Exclude LogiRGB

			using (FileStream fs = File.OpenRead(e.Filename))
			using (SHA1 sha1 = SHA1.Create()) {
				var checksum = sha1.ComputeHash(fs);
				var strChecksum = BitConverter.ToString(checksum).Replace("-", string.Empty).ToLowerInvariant();

				if (settings.HashesAndColors.ContainsKey(strChecksum)) {
					//
					// Dictionary entry found, using data from that
					//
					var colorBytes = settings.HashesAndColors[strChecksum];
					var color = Helpers.ByteArrayToColor(colorBytes);

					Debug.WriteLine("Dictionary hit! " + color.ToString());
					
					colorManager.SetColor(color);
				} else {
					//
					// No dictionary entry found, generating color and creating entry
					//

					var iconBitmap = Helpers.GetEXEIconBitmap(e.Filename);

					if (iconBitmap.Size.Width > 128) {
						iconBitmap = iconBitmap.Resize(new DSize(128, 128));
						Debug.WriteLine("Icon is bigger than 128x128, resizing.");
					}

					var colors = ColorManager.AnalyzeImage(iconBitmap, 32);
					DColor newColor;
					if (colors.Length == 0) {
						newColor = Helpers.ByteArrayToColor(settings.FallbackColor);
					} else {
						newColor = colors[0];
					}
					
					settings.HashesAndColors[strChecksum] = newColor.ToByteArray();
					colorManager.SetColor(newColor);

					settings.SaveSettings(); // Save settings so our newly generated colors aren't lost
				}
			}
		}

		private void Application_Exit(object sender, ExitEventArgs e) {
			colorManager.Shutdown();

			focusWatcher.StopWatching();
			focusWatcher.FocusChanged -= FocusWatcher_FocusChanged;
		}
	}

	class ShowWindowCommand : ICommand {
		public void Execute(object parameter) {
			Debug.WriteLine("Double Click");

			var openWindows = Application.Current.Windows.OfType<SettingsWindow>();
			if (openWindows.Count() != 1) {
				SettingsWindow settingsWindow = new SettingsWindow();
				settingsWindow.Show();
			} else {
				var settingsWindow = openWindows.First();
				//settingsWindow.Focus();
				settingsWindow.Activate();
			}
		}

		public bool CanExecute(object parameter) {
			return true;
		}

		public event EventHandler CanExecuteChanged;
	}
}
