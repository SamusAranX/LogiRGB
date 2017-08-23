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

			this.taskbarIcon = (TaskbarIcon)FindResource("taskbarIcon");

			this.settings = Settings.LoadSettings();

			this.colorManager = new ColorManager(Helpers.ByteArrayToColor(this.settings.FallbackColor));
			this.colorManager.Initialize();
			//colorManager.ColorChanged += ColorManager_ColorChanged;

			this.focusWatcher = new FocusWatcher();
			this.focusWatcher.FocusChanged += FocusWatcher_FocusChanged;
			this.focusWatcher.StartWatching();
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
			OpenSettingsWindow(0); // 0 = general settings
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
				var sw = new Stopwatch();
				sw.Start();

				using (FileStream fs = new FileStream(e.Filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 16 * 1024 * 1024))
				using (var md5 = new MD5CryptoServiceProvider()) {
					var checksum = md5.ComputeHash(fs);
					var strChecksum = BitConverter.ToString(checksum).Replace("-", string.Empty).ToLowerInvariant();
					this.ActiveColorHash = strChecksum;
					this.ActiveAppName = e.Filename;

					lock (this.settings) {
						if (this.settings.HashesAndColors.ContainsKey(strChecksum)) {
							//
							// Dictionary entry found, using data from that
							//

							var colorInfo = this.settings.HashesAndColors[strChecksum];
							var color = Helpers.ByteArrayToColor(colorInfo.UsesCustomColor ? colorInfo.CustomColor : colorInfo.Color);

							Debug.WriteLine("App: Dictionary hit! " + color.ToString());

							Application.Current.Dispatcher.Invoke(() => {
								this.colorManager.SetColor(color);
							});
						} else {
							//
							// No dictionary entry found, generating new color and creating entry
							//

							var iconBitmap = Helpers.GetEXEIconBitmap(e.Filename, e.WindowHandle);

							if (iconBitmap.Size.Width > 128 && iconBitmap.Size.Height > 128) {
								//iconBitmap = iconBitmap.ResizeQuickly(new DSize(128, 128));
								Debug.WriteLine("App: Icon is bigger than 128×128, resizing.");
							}

							var colors = Helpers.AnalyzeImage(iconBitmap, 32);
							DColor newColor;
							if (colors.Length == 0) {
								newColor = Helpers.ByteArrayToColor(this.settings.FallbackColor);
							} else {
								// Make color more intense and save it that way
								newColor = colors[0].MoreIntenseColor();
							}

							this.settings.HashesAndColors[strChecksum] = new Settings.ColorInfo(newColor);

							Application.Current.Dispatcher.Invoke(() => {
								this.colorManager.SetColor(newColor);
							});

							this.settings.SaveSettings(); // Save settings so our newly generated colors aren't lost
						}
					}
				}

				sw.Stop();
				Debug.WriteLine($"FocusChanged took {sw.Elapsed.TotalSeconds.ToString("0.###")} seconds");
			});
		}

		private void Application_Exit(object sender, ExitEventArgs e) {
			this.settings.SaveSettings();

			this.colorManager.Shutdown();

			this.focusWatcher.StopWatching();
			this.focusWatcher.FocusChanged -= FocusWatcher_FocusChanged;
		}
	}

	class ShowWindowCommand : ICommand {
#pragma warning disable 0067
		// This disables the warning that this EventHandler isn't being used
		public event EventHandler CanExecuteChanged;
#pragma warning restore 0067

		public void Execute(object parameter) {
			Debug.WriteLine("App: Double Click");

			((App)App.Current).OpenSettingsWindow(0); // 0 = general settings
		}

		public bool CanExecute(object parameter) {
			return true;
		}
	}
}
