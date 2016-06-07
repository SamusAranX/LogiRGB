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
			Debug.WriteLine("Loaded!");
			((App)Application.Current).colorManager.ColorChanged += ColorManager_ColorChanged;
		}

		private void ColorManager_ColorChanged(object sender, ColorChangedEventArgs e) {
			var newColor = e.NewColor.MoreIntenseColor();

			colorBorder.Background = new SolidColorBrush(e.NewColor.ToMediaColor());
			colorBorderNew.Background = new SolidColorBrush(newColor.ToMediaColor());
			colorLabel.Content = e.NewColor.ToHexString() + " - " + $"({e.NewColor.GetHue()}, {e.NewColor.GetSaturation()}, {e.NewColor.GetBrightness()})";
			colorLabelNew.Content = newColor.ToHexString() + "\n" + $"({newColor.GetHue()}, {newColor.GetSaturation()}, {newColor.GetBrightness()})";
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
	}
}
