using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using LogiRGB.Converters;
using PluginContracts;

namespace LogiRGB.Converters {
	class SettingsPluginNameDescDisplayConverter : BaseConverter, IValueConverter {
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			var pluginMetadata = (IPluginMetadata)value;

			return pluginMetadata.Name + "\n" + pluginMetadata.Description;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			Debug.WriteLine(value);
			Debug.WriteLine(targetType.Name);

			return null;
		}
	}
}
