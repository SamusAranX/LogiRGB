using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TsudaKageyu;

namespace LogiRGB {
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window {
		public MainWindow() {
			InitializeComponent();
		}

		public static bool IsAdministrator() {
			var identity = WindowsIdentity.GetCurrent();
			var principal = new WindowsPrincipal(identity);
			return principal.IsInRole(WindowsBuiltInRole.Administrator);
		}

		private void button_Click(object sender, RoutedEventArgs e) {
			Application.Current.Shutdown();
		}

		private void FocusWatcher_FocusChanged(object sender, FocusChangedEventArgs e) {
			Debug.WriteLine("Getting icon.");

			Bitmap iconBitmap = Helpers.GetEXEIconBitmap(e.Filename);
			if (iconBitmap.Size.Width > 128) {
				iconBitmap = iconBitmap.Resize(new System.Drawing.Size(128, 128));
				Debug.WriteLine("Icon is bigger than 128x128, resizing.");
			}

			//iconBitmap = iconBitmap.Resize(new System.Drawing.Size(128, 128));

			var analyzedData = Helpers.AnalyzeImage(iconBitmap, 32);
			var quantBitmap = analyzedData.Item1;
			var colors = analyzedData.Item2;

			image.Source = quantBitmap.ToBitmapSource();

			Debug.WriteLine(string.Join(", ", colors.Select(c => c.ToString())));

			Border[] colorBorders = { color1, color2, color3, color4 };
			foreach (Border b in colorBorders) {
				b.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0, 255, 255, 255));
			}
			for (int i = 0; i < colors.Length; i++) {
				colorBorders[i].Background = new SolidColorBrush(Helpers.ToMediaColor(colors[i]));
			}
		}

		private void Window_Loaded(object sender, RoutedEventArgs e) {
			//if (!IsAdministrator()) {
			//	MessageBox.Show("Without administrator privileges, LogiRGB won't be able to read some applications' data.\nYou don't have to grant it these privileges, but without them, some applications will not trigger a color change.", "LogiRGB", MessageBoxButton.OK, MessageBoxImage.Information);
			//}

			((App)Application.Current).focusWatcher.FocusChanged += FocusWatcher_FocusChanged;

			//if (!LogitechGSDK.LogiLedInit()) {
			//	Debug.WriteLine("LogiLedInit failed");
			//}
			//Thread.Sleep(3000);
			//LogitechGSDK.LogiLedSetTargetDevice(LogitechGSDK.LOGI_DEVICETYPE_RGB);

			//if (!LogitechGSDK.LogiLedInit()) {
			//	MessageBox.Show("Couldn't initialize SDK.", "LogiRGB", MessageBoxButton.OK, MessageBoxImage.Information);
			//}
			//LogitechGSDK.LogiLedSetTargetDevice(LogitechGSDK.LOGI_DEVICETYPE_ALL);
		}
	}
}
