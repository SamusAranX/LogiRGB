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
		
		[DllImport("gdi32")]
		static extern int DeleteObject(IntPtr o);

		/// <summary>
		/// Creates a BitmapSource object from a Bitmap.
		/// </summary>
		/// <param name="source">A Bitmap object</param>
		/// <returns>A BitmapSource object</returns>
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
		
		/// <summary>
		/// Converts a GDI+ Color object to a WPF Color object.
		/// </summary>
		/// <param name="color">A GDI+ Color object</param>
		/// <returns>A WPF Color object</returns>
		public static MColor ToMediaColor(this DColor color) {
			return MColor.FromArgb(color.A, color.R, color.G, color.B);
		}

		/// <summary>
		/// Converts a WPF Color object to a GDI+ Color object.
		/// </summary>
		/// <param name="color">A WPF Color object</param>
		/// <returns>A GDI+ Color object</returns>
		public static DColor ToDrawingColor(this MColor color) {
			return DColor.FromArgb(color.A, color.R, color.G, color.B);
		}

		/// <summary>
		/// Takes a color and makes it more intense if it isn't already.
		/// </summary>
		/// <param name="col">A GDI+ Color object to make more intense</param>
		/// <returns>A GDI+ Color object that's more intense if it wasn't already</returns>
		public static DColor MoreIntenseColor(this DColor col) {
			//Debug.WriteLine("MoreIntenseColor: " + col.ToString());

			var hue = col.GetHue();
			var sat = col.GetSaturation();
			var bri = col.GetBrightness();
			//Debug.WriteLine($"MoreIntenseColor: {hue}-{sat}-{bri}");

			// If color is already pretty intense, don't modify it any further
			if ((sat >= 0.65f && sat <= 1.0f) && (bri >= 0.35f || bri <= 0.65f))
				return col;

			// If color doesn't look intense, make it more intense
			//if (sat < 0.5f)
			//	sat = 0.5f;
			if (sat < 0.8f)
				sat = 0.95f;

			if (bri < 0.35f || bri > 0.65f)
				bri = 0.5f;

			//Debug.WriteLine($"MoreIntenseColor: {hue}-{sat}-{bri}");
			var moreIntenseColor = Helpers.ColorFromAHSB(255, hue, sat, bri);
			//Debug.WriteLine("MoreIntenseColor: " + moreIntenseColor.ToString());

			return moreIntenseColor;
		}

		/// <summary>
		/// Takes a specified range from an array and returns it as a sub-array.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="data">The array to create a sub-array from</param>
		/// <param name="index">An int that represents the starting index</param>
		/// <param name="length">An int that represents the length of the sub-array</param>
		/// <returns>A sub-array</returns>
		public static T[] SubArray<T>(this T[] data, int index, int length) {
			T[] result = new T[length];
			Array.Copy(data, index, result, 0, length);
			return result;
		}

		// Converts a Color object to a byte array
		public static byte[] ToByteArray(this DColor col) {
			return new byte[] { col.R, col.G, col.B };
		}
		public static DColor ToColor(this byte[] bytes) {
			if (bytes.Length != 3) {
				Debug.WriteLine("ByteArrayToColor: Out of range");
				throw new ArgumentOutOfRangeException();
			}

			return DColor.FromArgb(bytes[0], bytes[1], bytes[2]);
		}
		public static string ToHexString(this DColor c, bool includeHash=false) {
			return (includeHash ? "#" : "") + c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2");
		}

		// Returns an image as a byte array
		public static byte[] ToByteArray(this Image image, ImageFormat format) {
			using (MemoryStream ms = new MemoryStream()) {
				image.Save(ms, format);
				return ms.ToArray();
			}
		}

		// Resizes an image without resampling it because performance
		public static Bitmap ResizeQuickly(this Bitmap bitmap, DSize size) {
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

	}
}
