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
using Env = System.Environment;
using System.Security.Cryptography;

namespace LogiRGB {
	public class ColorManager {

		private DColor _currentColor;
		public DColor CurrentColor {
			get {
				return _currentColor;
			}
		}

		public ColorManager(DColor fallback) {
			_currentColor = fallback;
		}

		public bool InitializeSDK() {
			if (!LogitechGSDK.LogiLedInit())
				return false; // Don't return the result of the following calls. If this one fails, get the heck outta here.

			LogitechGSDK.LogiLedSetTargetDevice(LogitechGSDK.LOGI_DEVICETYPE_ALL);
			LogitechGSDK.LogiLedSaveCurrentLighting();

			return true;
		}

		public void Shutdown() {
			LogitechGSDK.LogiLedRestoreLighting();
			LogitechGSDK.LogiLedShutdown();
		}

		public bool SetColor(DColor newColor) {
			var pct = Helpers.ColorToPercentage(newColor);

			if (!LogitechGSDK.LogiLedSetLighting(pct.R, pct.G, pct.B))
				return false;
			
			_currentColor = newColor;

			OnColorChanged(_currentColor);

			return true;
		}

		/// <summary>
		/// Looks for the most common (or dominant) colors in an image
		/// </summary>
		/// <param name="image">The image to be analyzed</param>
		/// <param name="tolerance">Tolerance with which to check for monochrome colors.</param>
		/// <returns>Returns a sorted array of the four most dominant colors</returns>
		public static DColor[] AnalyzeImage(Bitmap image, int tolerance = 32) {
			var wu = new nQuant.WuQuantizer();
			var quantizedBitmap = new Bitmap(wu.QuantizeImage(image, 40, 70));

#if DEBUG
			var unixTimestamp = (DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds.ToString();
			var unixTimestampBytes = Encoding.UTF8.GetBytes(unixTimestamp);
			using(var md5 = MD5.Create()) {
				var debugPath = Path.Combine(Env.GetFolderPath(Env.SpecialFolder.MyPictures), "LogiRGB_Debug", BitConverter.ToString(md5.ComputeHash(unixTimestampBytes)) + ".png").Replace("-", string.Empty).ToLowerInvariant();
				Directory.CreateDirectory(Path.GetDirectoryName(debugPath));
				quantizedBitmap.Save(debugPath);
			}
#endif

			int pixelSize = 4;
			Dictionary<int, int> colorDict = new Dictionary<int, int>();

			var imgData = quantizedBitmap.LockBits(new Rectangle(System.Drawing.Point.Empty, quantizedBitmap.Size), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
			unsafe
			{
				for (int y = 0; y < imgData.Height; y++) {
					byte* row = (byte*)imgData.Scan0 + (y * imgData.Stride);

					for (int x = 0; x < imgData.Width; x++) {
						byte b = row[x * pixelSize];
						byte g = row[x * pixelSize + 1];
						byte r = row[x * pixelSize + 2];
						byte a = row[x * pixelSize + 3];

						// if alpha is greater than 200 and if not monochrome
						if (a > 200 && (Math.Abs(r - g) + Math.Abs(g - b) + Math.Abs(b - r)) > tolerance) {
							byte[] bytes = { b, g, r, a };
							int bgra = BitConverter.ToInt32(bytes.ToArray(), 0);

							if (colorDict.ContainsKey(bgra)) {
								colorDict[bgra]++;
							} else {
								colorDict[bgra] = 0;
								//Debug.WriteLine(Color.FromArgb(bgra));
							}
						}
					}
				}
			}
			quantizedBitmap.UnlockBits(imgData);

			DColor[] fourColors = { };
			if (colorDict.Count > 0) {
				fourColors = colorDict.Where(c => c.Key != 0)
							.OrderByDescending(c => c.Value)
							.Select(c => DColor.FromArgb(c.Key))
							//.Where(c => !c.isMonochrome(tolerance))
							.Take(4).ToArray();
			} else {
				fourColors.Populate(DColor.FromArgb(0, 127, 255));
			}

			return fourColors;
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
			_newColor = newColor;
		}

		public DColor NewColor {
			get {
				return _newColor;
			}
		}
	}
}
