using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LogiRGB {
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window {
		public MainWindow() {
			InitializeComponent();
		}

		private void button_Click(object sender, RoutedEventArgs e) {
			Application.Current.Shutdown();
		}
		private void button_Flash(object sender, RoutedEventArgs e) {
			LogitechGSDK.LogiLedFlashLighting(int.Parse(tbR.Text), int.Parse(tbG.Text), int.Parse(tbB.Text), 1000, 1000);
		}
		private void button_Pulse(object sender, RoutedEventArgs e) {
			LogitechGSDK.LogiLedPulseLighting(int.Parse(tbR.Text), int.Parse(tbG.Text), int.Parse(tbB.Text), 1000, 1000);
		}
		private void button_SetColor(object sender, RoutedEventArgs e) {
			LogitechGSDK.LogiLedSetLighting(int.Parse(tbR.Text), int.Parse(tbG.Text), int.Parse(tbB.Text));
		}

		private void Window_Loaded(object sender, RoutedEventArgs e) {
			if (!Helpers.IsAdministrator()) {
				MessageBox.Show("Without administrator privileges, LogiRGB won't be able to read some applications' data.\nYou don't have to grant it these privileges, but without them, some applications will not trigger a color change.", "LogiRGB", MessageBoxButton.OK, MessageBoxImage.Information);
			}

			if (!LogitechGSDK.LogiLedInit()) {
				MessageBox.Show("Couldn't initialize SDK.", "LogiRGB", MessageBoxButton.OK, MessageBoxImage.Information);
			}
			LogitechGSDK.LogiLedSetTargetDevice(LogitechGSDK.LOGI_DEVICETYPE_ALL);
		}
	}
}
