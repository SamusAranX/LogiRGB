using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DColor = System.Drawing.Color;

namespace LogitechGSDK_Plugin {
	public static class Helpers {

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

	}
}
