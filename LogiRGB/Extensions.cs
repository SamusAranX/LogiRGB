using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

using DColor = System.Drawing.Color;
using DSize = System.Drawing.Size;
using MColor = System.Windows.Media.Color;

namespace LogiRGB {
	public static class Extensions {

		// Convert Bitmap objects to BitmapSource objects
		[DllImport("gdi32")]
		static extern int DeleteObject(IntPtr o);

		public static BitmapSource ToBitmapSource(this Bitmap source) {
			IntPtr ip = source.GetHbitmap();
			BitmapSource bs = null;
			try {
				bs = Imaging.CreateBitmapSourceFromHBitmap(ip, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
			} finally {
				DeleteObject(ip);
			}

			return bs;
		}

		// Converts a WPF Color object to a GDI+ Color object
		public static MColor ToMediaColor(this DColor color) {
			return MColor.FromArgb(color.A, color.R, color.G, color.B);
		}

		public static T[] SubArray<T>(this T[] data, int index, int length) {
			T[] result = new T[length];
			Array.Copy(data, index, result, 0, length);
			return result;
		}

		// Converts a Color object to a byte array
		public static byte[] ToByteArray(this DColor col) {
			return new byte[] { col.R, col.G, col.B };
		}
		public static string ToHexString(this DColor c) {
			return "#" + c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2");
		}

		// Returns an image as a byte array
		public static byte[] ToByteArray(this Image image, ImageFormat format) {
			using (MemoryStream ms = new MemoryStream()) {
				image.Save(ms, format);
				return ms.ToArray();
			}
		}

		// Resizes an image without resampling it because performance
		public static Bitmap Resize(this Bitmap bitmap, DSize size) {
			Bitmap b = new Bitmap(size.Width, size.Height);
			using (Graphics g = Graphics.FromImage(b)) {
				g.InterpolationMode = InterpolationMode.NearestNeighbor;
				g.DrawImage(bitmap, 0, 0, size.Width, size.Height);
			}
			return b;
		}

		// Populates an array in place
		public static void Populate<T>(this T[] arr, T value) {
			for (int i = arr.Count(); i < arr.Length; i++) {
				arr[i] = value;
			}
		}

		public static DColor MoreIntenseColor(this DColor col) {
			Debug.WriteLine("MoreIntenseColor: " + col.ToString());

			var hue = col.GetHue();
			var sat = col.GetSaturation();
			var bri = col.GetBrightness();
			Debug.WriteLine($"MoreIntenseColor: {hue}-{sat}-{bri}");

			if ((sat >= 0.5f && sat <= 0.9f) && (bri >= 0.5f && bri <= 0.6f))
				return col;

			if (sat < 0.5f)
				sat = 0.5f;
			if (sat > 0.8f)
				sat = 0.9f;

			if (bri < 0.5f)
				bri = 0.5f;
			if (bri > 0.65f)
				bri = 0.6f;

			// arbitrary values, yay!
			//sat = 0.95f;
			//bri = 0.55f;
			
			Debug.WriteLine($"MoreIntenseColor: {hue}-{sat}-{bri}");
			var moreIntenseColor = Helpers.ColorFromAHSB(255, hue, sat, bri);
			Debug.WriteLine("MoreIntenseColor: " + moreIntenseColor.ToString());

			return moreIntenseColor;
		}

	}
}
