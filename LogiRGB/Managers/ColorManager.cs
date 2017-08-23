using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

using DColor = System.Drawing.Color;
using DSize = System.Drawing.Size;
using MColor = System.Windows.Media.Color;
using System.Security.Cryptography;
using System.Runtime.InteropServices;
using System.Diagnostics;
using LogiRGB.Managers;
using LogiRGB.Managers.Plugins;
using LogiRGB.Managers.SDK_Wrappers;

namespace LogiRGB.Managers {
	public class ColorManager {
		private DColor _currentColor;
		public DColor CurrentColor {
			get {
				return this._currentColor;
			}
		}

		private string _typeName;
		private string TypeName {
			get {
				return this._typeName;
			}
		}

		public ColorManager(DColor fallback) {
			this._currentColor = fallback;
			this._typeName = this.GetType().Name;
		}

		public bool Initialize() {
			Debug.WriteLine(this.TypeName + ": Initialize");

			if (!LogitechGSDK.LogiLedInit())
				return false; // Don't return the result of the following calls. If this one fails, get the heck outta here.

			// Only target devices that support RGB lighting
			LogitechGSDK.LogiLedSetTargetDevice(LogitechGSDK.LOGI_DEVICETYPE_RGB);
			LogitechGSDK.LogiLedSaveCurrentLighting();

			return true;
		}

		public bool SetColor(DColor newColor) {
			//TODO: Move LogiSDK logic to this method

			var col = newColor;

			var r = (int)Math.Min(col.R / 2.55, 100);
			var g = (int)Math.Min(col.G / 2.55, 100);
			var b = (int)Math.Min(col.B / 2.55, 100);

			Debug.WriteLine(this.TypeName + ": SetColor " + col.ToString());

			this._currentColor = newColor;
			OnColorChanged(this._currentColor);

			return LogitechGSDK.LogiLedSetLighting(r, g, b);
		}

		public void Shutdown() {
			Debug.WriteLine(this.TypeName + ": Shutdown");

			LogitechGSDK.LogiLedRestoreLighting();
			LogitechGSDK.LogiLedShutdown();
		}

		protected virtual void OnColorChanged(DColor newColor) {
			var handler = ColorChanged;
			if (handler == null)
				return;

			var eventArgs = new ColorChangedEventArgs(newColor);
			handler(this, eventArgs);
		}

		public event EventHandler<ColorChangedEventArgs> ColorChanged;
	}

	public class ColorChangedEventArgs : EventArgs {
		private readonly DColor _newColor;

		public ColorChangedEventArgs(DColor newColor) {
			this._newColor = newColor;
		}

		public DColor NewColor {
			get {
				return this._newColor;
			}
		}
	}
}
