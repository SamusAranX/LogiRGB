using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Windows;

namespace LogiRGB {
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application {
		WinApi.WinEventDelegate dele = null;

		public static bool IsAdministrator() {
			var identity = WindowsIdentity.GetCurrent();
			var principal = new WindowsPrincipal(identity);
			return principal.IsInRole(WindowsBuiltInRole.Administrator);
		}

		private void Application_Startup(object sender, StartupEventArgs e) {
			//if(!IsAdministrator()) {
			//	MessageBox.Show("Without administrator privileges, LogiRGB won't be able to read some applications' data.\nYou don't have to grant it these privileges, but without them, some applications will not trigger a color change.", "LogiRGB", MessageBoxButton.OK, MessageBoxImage.Information);
			//}
		}

		private void Application_Exit(object sender, ExitEventArgs e) {
			LogitechGSDK.LogiLedRestoreLighting();
			LogitechGSDK.LogiLedShutdown();
		}
	}
}
