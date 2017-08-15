using System;
using System.Collections.Generic;
using System.Deployment.Application;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
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
using LogiRGB.Managers;
using Microsoft.Win32;
using PluginContracts;

using MColor = System.Windows.Media.Color;
using MColorConverter = System.Windows.Media.ColorConverter;
using DColor = System.Drawing.Color;
using System.ComponentModel;

namespace LogiRGB {
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class SettingsWindow : Window, INotifyPropertyChanged {
		[DllImport("user32.dll")]
		private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
		[DllImport("user32.dll")]
		private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

		private const int GWL_STYLE = -16;
		private const int WS_MAXIMIZEBOX = 0x10000;

		// this is not a true const, but #hashtagyolo
		private Regex HEX_COLOR_REGEX = new Regex("([0-9a-f]{6})", RegexOptions.IgnoreCase | RegexOptions.Compiled);

		private void Window_SourceInitialized(object sender, EventArgs e) {
			var hwnd = new WindowInteropHelper((Window)sender).Handle;
			var value = GetWindowLong(hwnd, GWL_STYLE);
			SetWindowLong(hwnd, GWL_STYLE, (value & ~WS_MAXIMIZEBOX));
		}

		public SettingsWindow() {
			InitializeComponent();
		}

		public string CurrentVersion {
			get {
				return ApplicationDeployment.IsNetworkDeployed
					   ? ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString()
					   : Assembly.GetExecutingAssembly().GetName().Version.ToString();
			}
		}

		private Settings Settings {
			get { return ((App)App.Current).settings; }
		}

		private string ActiveColorHash {
			get { return ((App)App.Current).ActiveColorHash; }
		}

		public Visibility CustomColorVisibility {
			get {
				// This can happen right after starting LogiRGB
				if (this.ActiveColorHash == null)
					return Visibility.Hidden;

				return this.Settings.HashesAndColors[this.ActiveColorHash].UsesCustomColor ? Visibility.Visible : Visibility.Hidden;
			}
		}

		private bool IsStartupItem() {
			// The path to the key where Windows looks for startup applications
			RegistryKey rkApp = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

			return rkApp.GetValue("LogiRGB") != null;
		}

		//
		// Window and Event Management
		//

		private void Window_Loaded(object sender, RoutedEventArgs e) {
			Debug.WriteLine("SettingsWindow: Loaded!");

			this.DataContext = this;

			var currentColor = ((App)App.Current).colorManager.CurrentColor;
			colorBorder.Background = new SolidColorBrush(currentColor.ToMediaColor());
			colorLabel.Text = currentColor.ToHexString();
			exeNameLabel.Content = Path.GetFileName(((App)App.Current).ActiveAppName);

			((App)App.Current).colorManager.ColorChanged += ColorManager_ColorChanged;
			((App)App.Current).focusWatcher.FocusChanged += FocusWatcher_FocusChanged;
		}

		//
		// Stuff that's not in a tab item
		//

		private void Debug_Click(object sender, RoutedEventArgs e) {
			((App)App.Current).settings.HashesAndColors.Clear();
			Debug.WriteLine("SettingsWindow: Hashes and colors reset");
		}

		private void Exit_Click(object sender, RoutedEventArgs e) {
			App.Current.Shutdown();
		}

		private void FocusWatcher_FocusChanged(object sender, FocusChangedEventArgs e) {
			exeNameLabel.Content = Path.GetFileName(e.Filename);
		}

		private void ColorManager_ColorChanged(object sender, ColorChangedEventArgs e) {
			var newColor = e.NewColor;

			OnPropertyChanged("CustomColorVisibility");

			colorBorder.Background = new SolidColorBrush(e.NewColor.ToMediaColor());
			colorLabel.Text = e.NewColor.ToHexString();
		}

		//
		// Color preview
		//

		private void colorLabel_KeyDown(object sender, KeyEventArgs e) {
			if (e.Key == Key.Enter) {
				Debug.WriteLine("SettingsWindow: Clearing focus");

				Keyboard.ClearFocus();
				
				var hexString = ((TextBox)sender).Text;
				Debug.WriteLine(hexString);
				if (HEX_COLOR_REGEX.IsMatch(hexString)) {
					Debug.WriteLine("SettingsWindow: Regex match");
					try {
						var newColor = ((MColor)MColorConverter.ConvertFromString(hexString)).ToDrawingColor();
						Debug.WriteLine(newColor);

						// Update color in the hash/color dictionary
						this.Settings.HashesAndColors[((App)App.Current).ActiveColorHash].CustomColor = newColor.ToByteArray();
						this.Settings.HashesAndColors[((App)App.Current).ActiveColorHash].UsesCustomColor = true;
						this.Settings.SaveSettings();

						// The ColorChanged event above will handle the background and the hex color label
						((App)App.Current).colorManager.SetColor(newColor);

						return;
					} catch (Exception) { /* eh */ }
				}

				// Entered text is not a valid hex color, reset everything
				Debug.WriteLine("SettingsWindow: Invalid hex number, restoring old color");
				colorLabel.Text = ((App)App.Current).colorManager.CurrentColor.ToHexString();
			}
		}

		private void Reset_Click(object sender, RoutedEventArgs e) {
			this.Settings.HashesAndColors[this.ActiveColorHash].UsesCustomColor = false;
			var color = Helpers.ByteArrayToColor(this.Settings.HashesAndColors[this.ActiveColorHash].Color);
			((App)App.Current).colorManager.SetColor(color);
		}

		//
		// About window
		//
		
		private void UpdateCheck_Click(object sender, RoutedEventArgs e) {
			//TODO: Actually implement updates
		}

		//
		// General stuff
		//

		private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e) {
			var link = (Hyperlink)e.OriginalSource;
			Process.Start(link.NavigateUri.AbsoluteUri);
		}

		private void Window_Closing(object sender, CancelEventArgs e) {
			((App)App.Current).colorManager.ColorChanged -= ColorManager_ColorChanged;
			((App)App.Current).focusWatcher.FocusChanged -= FocusWatcher_FocusChanged;
		}

		//
		// INotifyPropertyChanged
		//

		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged(string name) {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}
	}
}
