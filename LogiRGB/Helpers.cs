using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
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

namespace LogiRGB {
	public static class Helpers {

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

		// Converts a WPF Color object to a GDI+ Color object
		public static System.Windows.Media.Color ToMediaColor(this Color color) {
			return System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B);
		}

		// Converts a Color object to a byte array and back
		public static Color ToColor(this byte[] bytes) {
			if (bytes.Length != 3)
				throw new ArgumentOutOfRangeException();

			return Color.FromArgb(bytes[0], bytes[1], bytes[2]);
		}
		public static byte[] ToByteArray(this Color col) {
			return new byte[] { col.R, col.G, col.B };
		}

		// Returns an image as a byte array
		public static byte[] ToByteArray(this Image image, ImageFormat format) {
			using (MemoryStream ms = new MemoryStream()) {
				image.Save(ms, format);
				return ms.ToArray();
			}
		}

		// Resizes an image without resampling it because performance
		public static Bitmap Resize(this Bitmap bitmap, System.Drawing.Size size) {
			Bitmap b = new Bitmap(size.Width, size.Height);
			using (Graphics g = Graphics.FromImage(b)) {
				g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
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

		public static Color MoreIntenseColor(this Color col) {
			var hue = col.GetHue();
			var sat = col.GetSaturation();
			var lum = col.GetBrightness();

			if (sat >= 75 && lum >= 75)
				return col;

			// arbitrary values, yay!
			sat = 90;
			lum = 90;

			return ColorFromHSL(hue, sat, lum);
		}

		// Creates a Color object from HSL values
		// from http://stackoverflow.com/a/4794649/1058399
		public static Color ColorFromHSL(double hue, double saturation, double luminosity) {
			byte r, g, b;
			if (saturation == 0) {
				r = (byte)Math.Round(luminosity * 255d);
				g = (byte)Math.Round(luminosity * 255d);
				b = (byte)Math.Round(luminosity * 255d);
			} else {
				double t1, t2;
				double th = hue / 6.0d;

				if (luminosity < 0.5d) {
					t2 = luminosity * (1d + saturation);
				} else {
					t2 = (luminosity + saturation) - (luminosity * saturation);
				}
				t1 = 2d * luminosity - t2;

				double tr, tg, tb;
				tr = th + (1.0d / 3.0d);
				tg = th;
				tb = th - (1.0d / 3.0d);

				tr = ColorCalc(tr, t1, t2);
				tg = ColorCalc(tg, t1, t2);
				tb = ColorCalc(tb, t1, t2);
				r = (byte)Math.Round(tr * 255d);
				g = (byte)Math.Round(tg * 255d);
				b = (byte)Math.Round(tb * 255d);
			}
			return Color.FromArgb(r, g, b);
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

		/// <summary>
		/// Looks for the most common (or dominant) colors in an image
		/// </summary>
		/// <param name="image">The image to be analyzed</param>
		/// <param name="tolerance">Tolerance with which to check for monochrome colors.</param>
		/// <returns>Returns a sorted array of the four most dominant colors</returns>
		public static Color[] AnalyzeImage(Bitmap image, int tolerance = 32) {
			var wu = new nQuant.WuQuantizer();
			var quantizedBitmap = new Bitmap(wu.QuantizeImage(image, 40, 70));

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

			Color[] fourColors = { };
			if (colorDict.Count > 0) {
				fourColors = colorDict.Where(c => c.Key != 0)
							.OrderByDescending(c => c.Value)
							.Select(c => Color.FromArgb(c.Key))
							//.Where(c => !c.isMonochrome(tolerance))
							.Take(4).ToArray();
			} else {
				fourColors.Populate(Color.FromArgb(0, 127, 255));
			}

			return fourColors;
		}
	}
}
