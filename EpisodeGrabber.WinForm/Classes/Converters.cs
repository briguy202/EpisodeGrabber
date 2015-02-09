using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using EpisodeGrabber.Library.Entities;
using System.Windows.Media.Imaging;
using System.IO;

namespace EpisodeGrabber.Classes {
	public class ImageToThumbnailPathConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			Image image = value as Image;
			string path = null;

			if (image != null) {
				path = (string.IsNullOrWhiteSpace(image.ThumbnailURL)) ? image.URL : image.ThumbnailURL;
			}

			return path;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			throw new NotImplementedException();
		}
	}

	public class MinDateTimeConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			return ((DateTime)value > DateTime.MinValue) ? value : null;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			return (value != null) ? value : DateTime.MinValue;
		}
	}

	public class ZeroIntConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			return ((int)value != 0) ? value : null;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			return (value != null) ? value : 0;
		}
	}

	public class BitmapImageConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			string path = (string)value;
			BitmapImage image = new BitmapImage(new Uri(path));
			return image;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			throw new NotImplementedException();
		}
	}

	public class RecursiveBitmapImageConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			BitmapImage image = null;
			if (value != null) {
				string path = (string)value;
				string filename = (string)parameter;
				int count = 0;

				while (image == null && count < 2) {
					string fullPath = string.Format("{0}\\{1}", path, filename);
					if (File.Exists(fullPath)) {
						image = new BitmapImage(new Uri(fullPath)).Clone();
					}
					count++;
					path = Directory.GetParent(path).FullName;
				}
			}

			return image;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			throw new NotImplementedException();
		}
	}
}
