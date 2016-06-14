using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using PluginContracts;

namespace LogiRGB.Converters {
	class SettingsPluginMetadataConverter : BaseConverter, IValueConverter {
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			var lazyPlugins = (IEnumerable<Lazy<IPlugin, IPluginMetadata>>)value;

			return lazyPlugins.Select(lp => lp.Metadata.Name);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			Debug.WriteLine(value);
			Debug.WriteLine(targetType.Name);

			return null;
		}
	}
}
