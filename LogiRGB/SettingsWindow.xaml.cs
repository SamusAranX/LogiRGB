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

using MColor = System.Windows.Media.Color;

namespace LogiRGB {
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class SettingsWindow : Window {
		public SettingsWindow() {
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

		//
		// Window and Event Management
		//

		private void Window_Loaded(object sender, RoutedEventArgs e) {
			((App)Application.Current).colorManager.ColorChanged += ColorManager_ColorChanged;
		}

		private void ColorManager_ColorChanged(object sender, ColorChangedEventArgs e) {
			var colors = e.AllColors;

			Debug.WriteLine("All colors: " + string.Join(", ", colors.Select(c => c.ToString())));

			Border[] colorBorders = { color1, color2, color3, color4 };
			foreach (Border b in colorBorders) {
				b.Background = new SolidColorBrush(MColor.FromArgb(200, 255, 255, 255));
			}
			for (int i = 0; i < colors.Length; i++) {
				colorBorders[i].Background = new SolidColorBrush(Helpers.ToMediaColor(colors[i]));
			}
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
			((App)Application.Current).colorManager.ColorChanged -= ColorManager_ColorChanged;
		}



		//
		// SUPER SECRIT DEBUGGING STUFF
		//

		private void SaveTest_Click(object sender, RoutedEventArgs e) {
			Settings settings = new Settings();

			try {
				var r = new Random();
				var loopMax = r.Next(128, 512);
				for (int i = 0; i < loopMax; i++) {
					byte[] testColors = new byte[3];
					r.NextBytes(testColors);

					settings.HashesAndColors.Add("key" + i.ToString(), testColors);
				}
			} catch (Exception ex) {
				Debug.WriteLine(ex.Message);
			}

			Debug.WriteLine("SaveSettings: " + settings.SaveSettings().ToString());
		}

		private void LoadTest_Click(object sender, RoutedEventArgs e) {
			var settings = Settings.LoadSettings();

			Debug.WriteLine(string.Join(", ", settings.FallbackColor));
			Debug.WriteLine(string.Join(", ", settings.HashesAndColors));
		}

		private void SetColorTest_Click(object sender, RoutedEventArgs e) {
			var colR = (int)SliderR.Value;
			var colG = (int)SliderG.Value;
			var colB = (int)SliderB.Value;

			LogitechGSDK.LogiLedSetLighting(colR, colG, colB);
		}

		private void PulseColorTest_Click(object sender, RoutedEventArgs e) {
			
		}
	}
}
