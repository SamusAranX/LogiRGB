using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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

namespace LogiRGB {
	public static class Helpers {
		

		// Gets the biggest icon of an .exe file
		public static Bitmap GetEXEIconBitmap(string path) {
			IconExtractor ie = new IconExtractor(path);
			if (ie.Count > 0) {
				Icon bigIcon = IconUtil.Split(ie.GetIcon(0)).OrderByDescending(i => IconUtil.ToBitmap(i).PhysicalDimension.Width).First();
				return IconUtil.ToBitmap(bigIcon);
			} else {
				Debug.WriteLine("ExtractAssociatedIcon");
				return Icon.ExtractAssociatedIcon(path).ToBitmap();
			}
		}

		public static DColor ByteArrayToColor(byte[] bytes) {
			if (bytes.Length != 3) {
				Debug.WriteLine("ByteArrayToColor: Out of range");
				throw new ArgumentOutOfRangeException();
			} 

			return DColor.FromArgb(bytes[0], bytes[1], bytes[2]);
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
