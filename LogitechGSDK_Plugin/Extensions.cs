using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DColor = System.Drawing.Color;

namespace LogitechGSDK_Plugin {
	public static class Extensions {

		public static DColor MoreIntenseColor(this DColor col) {
			//Debug.WriteLine("MoreIntenseColor: " + col.ToString());

			var hue = col.GetHue();
			var sat = col.GetSaturation();
			var bri = col.GetBrightness();
			//Debug.WriteLine($"MoreIntenseColor: {hue}-{sat}-{bri}");

			// If color is already pretty intense, don't modify it any further
			if ((sat >= 0.5f && sat <= 0.9f) && (bri >= 0.5f && bri <= 0.6f))
				return col;

			// If color doesn't look intense, make it more intense
			if (sat < 0.5f)
				sat = 0.5f;
			if (sat > 0.8f)
				sat = 0.9f;

			if (bri < 0.5f)
				bri = 0.5f;
			if (bri > 0.65f)
				bri = 0.6f;

			//Debug.WriteLine($"MoreIntenseColor: {hue}-{sat}-{bri}");
			var moreIntenseColor = Helpers.ColorFromAHSB(255, hue, sat, bri);
			//Debug.WriteLine("MoreIntenseColor: " + moreIntenseColor.ToString());

			return moreIntenseColor;
		}

	}
}
