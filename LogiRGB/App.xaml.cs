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
using System.Resources;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
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

		private TaskbarIcon taskbarIcon;
		public Settings settings;

		public string ActiveColorHash { get; set; }
		public string ActiveAppName { get; set; }

		private void Application_Startup(object sender, StartupEventArgs e) {
			Debug.WriteLine($"App: Own PID: {Process.GetCurrentProcess().Id}");

			taskbarIcon = (TaskbarIcon)FindResource("taskbarIcon");

			settings = Settings.LoadSettings();

			colorManager = new ColorManager(Helpers.ByteArrayToColor(settings.FallbackColor));
			colorManager.InitializePlugins();
			//colorManager.ColorChanged += ColorManager_ColorChanged;

			focusWatcher = new FocusWatcher();
			focusWatcher.FocusChanged += FocusWatcher_FocusChanged;
			focusWatcher.StartWatching();
		}

		private void Application_LoadCompleted(object sender, System.Windows.Navigation.NavigationEventArgs e) {
			Debug.WriteLine("App: Load completed");
		}

		public void OpenSettingsWindow(int tabIndex = 0) {
			var openWindows = this.Windows.OfType<SettingsWindow>();
			if (openWindows.Count() != 1) {
				SettingsWindow settingsWindow = new SettingsWindow();
				settingsWindow.Show();
			} else {
				var settingsWindow = openWindows.First();
				settingsWindow.SettingsTabs.SelectedIndex = tabIndex;
				settingsWindow.Activate();
			}
		}

		private void Settings_Click(object sender, RoutedEventArgs e) {
			Debug.WriteLine("App: Display Settings window");
			OpenSettingsWindow(1); // 1 = general settings
		}

		private void About_Click(object sender, RoutedEventArgs e) {
			Debug.WriteLine("App: Display About window");
			OpenSettingsWindow(2); // 2 = about
		}

		private void Exit_Click(object sender, RoutedEventArgs e) {
			Current.Shutdown();
		}

		private async void FocusWatcher_FocusChanged(object sender, FocusChangedEventArgs e) {
			await Task.Run(() => {
				using (FileStream fs = new FileStream(e.Filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 16 * 1024 * 1024))
				using (var md5 = new MD5CryptoServiceProvider()) {
					var checksum = md5.ComputeHash(fs);
					var strChecksum = BitConverter.ToString(checksum).Replace("-", string.Empty).ToLowerInvariant();
					this.ActiveColorHash = strChecksum;
					this.ActiveAppName = e.Filename;

					lock (settings) {
						if (settings.HashesAndColors.ContainsKey(strChecksum)) {
							//
							// Dictionary entry found, using data from that
							//

							var colorInfo = settings.HashesAndColors[strChecksum];
							var color = Helpers.ByteArrayToColor(colorInfo.UsesCustomColor ? colorInfo.CustomColor : colorInfo.Color);

							Debug.WriteLine("App: Dictionary hit! " + color.ToString());

							colorManager.SetColor(color);
						} else {
							//
							// No dictionary entry found, generating new color and creating entry
							//

							var iconBitmap = Helpers.GetEXEIconBitmap(e.Filename);

							if (iconBitmap.Size.Width > 128) {
								iconBitmap = iconBitmap.Resize(new DSize(128, 128));
								Debug.WriteLine("App: Icon is bigger than 128x128, resizing.");
							}

							var colors = ColorManager.AnalyzeImage(iconBitmap, 32);
							DColor newColor;
							if (colors.Length == 0) {
								newColor = Helpers.ByteArrayToColor(settings.FallbackColor);
							} else {
								newColor = colors[0];
							}

							settings.HashesAndColors[strChecksum] = new Settings.ColorInfo(newColor);
							colorManager.SetColor(newColor);

							settings.SaveSettings(); // Save settings so our newly generated colors aren't lost
						}
					}
				}
			});
		}

		private void Application_Exit(object sender, ExitEventArgs e) {
			settings.SaveSettings();

			colorManager.Shutdown();

			focusWatcher.StopWatching();
			focusWatcher.FocusChanged -= FocusWatcher_FocusChanged;
		}
	}

	class ShowWindowCommand : ICommand {
#pragma warning disable 0067
		// This disables the warning that this EventHandler isn't being used
		public event EventHandler CanExecuteChanged;
#pragma warning restore 0067

		public void Execute(object parameter) {
			Debug.WriteLine("App: Double Click");

			((App)App.Current).OpenSettingsWindow(1); // 1 = color preview
		}

		public bool CanExecute(object parameter) {
			return true;
		}
	}
}
