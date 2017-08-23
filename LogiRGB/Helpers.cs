using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using TsudaKageyu;

using DColor = System.Drawing.Color;
using DSize = System.Drawing.Size;
using MColor = System.Windows.Media.Color;
using Env = System.Environment;

namespace LogiRGB {
	public static class Helpers {

		// Gets the biggest icon of an .exe file
		public static Bitmap GetEXEIconBitmap(string path, IntPtr hwnd) {
			IconExtractor ie = new IconExtractor(path);
			if (ie.Count > 0) {
				Icon bigIcon = IconUtil.Split(ie.GetIcon(0)).OrderByDescending(i => {
					try {
						return IconUtil.ToBitmap(i).PhysicalDimension.Width;
					} catch (ArgumentException) {
						return 1;
					}
				}).First();

				return IconUtil.ToBitmap(bigIcon);
			} else {
				Debug.WriteLine("Alternate Extraction Mode");

				IntPtr iconHandle = WinApi.SendMessage(hwnd, WinApi.WM_GETICON, WinApi.ICON_BIG, 0);
				if (iconHandle == IntPtr.Zero)
					iconHandle = WinApi.SendMessage(hwnd, WinApi.WM_GETICON, WinApi.ICON_SMALL, 0);
				if (iconHandle == IntPtr.Zero)
					iconHandle = WinApi.SendMessage(hwnd, WinApi.WM_GETICON, WinApi.ICON_SMALL2, 0);
				if (iconHandle == IntPtr.Zero)
					iconHandle = WinApi.GetClassLongPtr(hwnd, WinApi.GCL_HICON);
				if (iconHandle == IntPtr.Zero)
					iconHandle = WinApi.GetClassLongPtr(hwnd, WinApi.GCL_HICONSM);

				if (iconHandle == IntPtr.Zero)
					return null;

				Icon icn = Icon.FromHandle(iconHandle);

				Debug.WriteLine(icn.Size);

				return IconUtil.ToBitmap(icn);
			}
			//} else {
			//	Debug.WriteLine("ExtractAssociatedIcon");
			//	return Icon.ExtractAssociatedIcon(path).ToBitmap();
			//}
		}

		public static DColor ByteArrayToColor(byte[] bytes) {
			if (bytes.Length != 3) {
				Debug.WriteLine("ByteArrayToColor: Out of range");
				throw new ArgumentOutOfRangeException();
			} 

			return DColor.FromArgb(bytes[0], bytes[1], bytes[2]);
		}

		/// <summary>
		/// Looks for the most common (or dominant) colors in an image
		/// </summary>
		/// <param name="image">The image to be analyzed</param>
		/// <param name="tolerance">Tolerance with which to check for monochrome colors.</param>
		/// <returns>Returns a sorted array of the four most dominant colors</returns>
		public static DColor[] AnalyzeImage(Bitmap image, int tolerance = 32, bool allowMonochrome = false) {
			var wu = new nQuant.WuQuantizer();
			var quantizedBitmap = new Bitmap(wu.QuantizeImage(image, 40, 70));
			

#if DEBUG
			var unixTimestamp = (DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds.ToString();
			var unixTimestampBytes = Encoding.UTF8.GetBytes(unixTimestamp);
			using (var md5 = MD5.Create()) {
				var debugPath = Path.Combine(Env.GetFolderPath(Env.SpecialFolder.MyPictures), "LogiRGB_Debug", BitConverter.ToString(md5.ComputeHash(unixTimestampBytes)) + ".png").Replace("-", string.Empty).ToLowerInvariant();
				Directory.CreateDirectory(Path.GetDirectoryName(debugPath));
				quantizedBitmap.Save(debugPath);
			}
#endif

			int pixelSize = 4;
			Dictionary<int, int> colorDict = new Dictionary<int, int>(); // Dictionary to sort colors with

			var imgData = quantizedBitmap.LockBits(new Rectangle(System.Drawing.Point.Empty, quantizedBitmap.Size), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
			unsafe {
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

		/// <summary>
		/// Creates a Color object from Alpha, Hue, Saturation, and Brightness values
		/// </summary>
		/// <param name="a">Alpha, 0-255</param>
		/// <param name="h">Hue, 0.0-360.0</param>
		/// <param name="s">Saturation, 0.0-1.0</param>
		/// <param name="b">Brightness, 0.0-1.0</param>
		/// <returns></returns>
		public static DColor ColorFromAHSB(int a, float h, float s, float b) {
			if (0 == s) {
				return DColor.FromArgb(a, Convert.ToInt32(b * 255),
				  Convert.ToInt32(b * 255), Convert.ToInt32(b * 255));
			}

			float fMax, fMid, fMin;
			int iSextant, iMax, iMid, iMin;

			if (0.5 < b) {
				fMax = b - (b * s) + s;
				fMin = b + (b * s) - s;
			} else {
				fMax = b + (b * s);
				fMin = b - (b * s);
			}

			iSextant = (int)Math.Floor(h / 60f);
			if (300f <= h) {
				h -= 360f;
			}
			h /= 60f;
			h -= 2f * (float)Math.Floor(((iSextant + 1f) % 6f) / 2f);
			if (0 == iSextant % 2) {
				fMid = h * (fMax - fMin) + fMin;
			} else {
				fMid = fMin - h * (fMax - fMin);
			}

			iMax = Convert.ToInt32(fMax * 255);
			iMid = Convert.ToInt32(fMid * 255);
			iMin = Convert.ToInt32(fMin * 255);

			switch (iSextant) {
				case 1:
					return DColor.FromArgb(a, iMid, iMax, iMin);
				case 2:
					return DColor.FromArgb(a, iMin, iMax, iMid);
				case 3:
					return DColor.FromArgb(a, iMin, iMid, iMax);
				case 4:
					return DColor.FromArgb(a, iMid, iMin, iMax);
				case 5:
					return DColor.FromArgb(a, iMax, iMin, iMid);
				default:
					return DColor.FromArgb(a, iMax, iMid, iMin);
			}
		}

		private static double ColorCalc(double c, double t1, double t2) {
			if (c < 0)
				c += 1d;
			if (c > 1)
				c -= 1d;
			if (6.0d * c < 1.0d)
				return t1 + (t2 - t1) * 6.0d * c;
			if (2.0d * c < 1.0d)
				return t2;
			if (3.0d * c < 2.0d)
				return t1 + (t2 - t1) * (2.0d / 3.0d - c) * 6.0d;
			return t1;
		}
	}
}
