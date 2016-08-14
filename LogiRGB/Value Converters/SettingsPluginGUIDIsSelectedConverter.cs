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
	class SettingsPluginGUIDIsSelectedConverter : BaseConverter, IValueConverter {
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			var pluginMetadata = (IPluginMetadata)value;
			var activePlugins = ((App)App.Current).settings.ActivePluginGUIDs;

			return activePlugins.Contains(pluginMetadata.GUID);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			Debug.WriteLine(value);
			Debug.WriteLine(targetType.Name);

			return null;
		}
	}
}
