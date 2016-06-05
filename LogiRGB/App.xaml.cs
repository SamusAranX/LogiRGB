using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows;
using Hardcodet.Wpf.TaskbarNotification;

namespace LogiRGB {
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application {

		public FocusWatcher focusWatcher;
		private TaskbarIcon taskbarIcon;
		public Settings settings;

		private void Application_Startup(object sender, StartupEventArgs e) {
			taskbarIcon = (TaskbarIcon)FindResource("taskbarIcon");

			focusWatcher = new FocusWatcher();
			focusWatcher.FocusChanged += FocusWatcher_FocusChanged;
			focusWatcher.StartWatching();

			//if(!IsAdministrator()) {
			//	MessageBox.Show("Without administrator privileges, LogiRGB won't be able to read some applications' data.\nYou don't have to grant it these privileges, but without them, some applications will not trigger a color change.", "LogiRGB", MessageBoxButton.OK, MessageBoxImage.Information);
			//}
		}

		private void FocusWatcher_FocusChanged(object sender, FocusChangedEventArgs e) {

			using (SHA1CryptoServiceProvider sha1 = new SHA1CryptoServiceProvider()) {
				Convert.ToBase64String(sha1.ComputeHash(byteArray));
			}
		}

		private void Application_Exit(object sender, ExitEventArgs e) {
			//LogitechGSDK.LogiLedRestoreLighting();
			//LogitechGSDK.LogiLedShutdown();
		}
	}
}
