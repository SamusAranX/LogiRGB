using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace LogiRGB {
	public class ColorManager {

		private Color _currentColor;
		public Color CurrentColor {
			get {
				return _currentColor;
			}
		}

		private Color[] _otherColors;
		public Color[] OtherColors {
			get {
				return _otherColors;
			}
		}

		public Color[] AllColors {
			get {
				return new Color[] { _currentColor }.Concat(_otherColors).ToArray();
			}
		}

		// Destructor, because who has time for a manual cleanup?
		~ColorManager() {
			LogitechGSDK.LogiLedRestoreLighting();
			LogitechGSDK.LogiLedShutdown();
		}

		public ColorManager(Color fallback) {
			_currentColor = fallback;
		}

		public bool Initialize() {
			if (!LogitechGSDK.LogiLedInit())
				return false; // Don't return the result of the following calls. If this one fails, get the heck outta here.

			LogitechGSDK.LogiLedSetTargetDevice(LogitechGSDK.LOGI_DEVICETYPE_ALL);
			LogitechGSDK.LogiLedSaveCurrentLighting();

			return true;
		}

		public bool SetColor(Color newColor, Color[] otherColors) {
			var newR = (int)Math.Min((double)newColor.R / 2.55, 100);
			var newG = (int)Math.Min((double)newColor.G / 2.55, 100);
			var newB = (int)Math.Min((double)newColor.B / 2.55, 100);

			if (!LogitechGSDK.LogiLedSetLighting(newR, newG, newB))
				return false;
			
			_currentColor = newColor;
			_otherColors = otherColors;

			return true;
		}
		
		protected virtual void OnColorChanged(Color newColor, Color[] otherColors) {
			var handler = ColorChanged;
			if (handler == null)
				return;

			var eventArgs = new ColorChangedEventArgs(newColor, otherColors);
			handler(this, eventArgs);
		}

		public event EventHandler<ColorChangedEventArgs> ColorChanged;
	}

	public class ColorChangedEventArgs : EventArgs {
		private readonly Color _newColor;
		private readonly Color[] _otherColors;

		public ColorChangedEventArgs(Color newColor, Color[] otherColors) {
			_newColor = newColor;
			_otherColors = otherColors;
		}

		public Color NewColor {
			get {
				return _newColor;
			}
		}

		public Color[] OtherColors {
			get {
				return _otherColors;
			}
		}

		public Color[] AllColors {
			get {
				return new Color[] { _newColor }.Concat(_otherColors).ToArray();
			}
		}
	}
}
