using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
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

		public static BitmapSource GetEXEIcon(string path) {
			return GetEXEIconBitmap(path).ToBitmapSource();
		}

		public static System.Windows.Media.Color ToMediaColor(this Color color) {
			return System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B);
		}

		public static Bitmap Resize(this Bitmap bitmap, System.Drawing.Size size) {
			Bitmap b = new Bitmap(size.Width, size.Height);
			using (Graphics g = Graphics.FromImage(b)) {
				g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
				g.DrawImage(bitmap, 0, 0, size.Width, size.Height);
			}
			return b;
		}

		public static void Populate<T>(this T[] arr, T value) {
			for (int i = arr.Count(); i < arr.Length; i++) {
				arr[i] = value;
			}
		}

		public static Tuple<Bitmap, Color[]> AnalyzeImage(Bitmap image, int tolerance) {
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

			return Tuple.Create(quantizedBitmap, fourColors);
		}
	}
}
