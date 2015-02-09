using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using EpisodeGrabber.Classes;
using EpisodeGrabber.Library.Entities;
using CommonLibrary.Framework;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;
using System.ComponentModel;
using EpisodeGrabber.Library.Services;

namespace EpisodeGrabber.Controls.Main {
	/// <summary>
	/// Interaction logic for ImageViewer.xaml
	/// </summary>
	public partial class ImageViewer : UserControl {

		#region Commands
		public static RoutedUICommand SetPosterImage = new RoutedUICommand("SetPosterImage", "SetPosterImage", typeof(ImageViewer));
		#endregion

		#region Properties
		public Movie Context { get { return (Movie)this.DataContext; } }
		#endregion

		#region Constructors
		public ImageViewer() {
			InitializeComponent();

			//Movie designMovie = null;
			//XmlSerializer serializer = new XmlSerializer(typeof(Movie));
			//using (XmlReader reader = XmlReader.Create(@"C:\Shared\Movies\Grown Up Movies\(500) Days of Summer (2009)\grabber.xml")) {
			//    designMovie = serializer.Deserialize(reader) as Movie;
			//}
		}
		#endregion

		#region Binding Methods
		private void SetPosterImage_Execute(object sender, ExecutedRoutedEventArgs e) {
			Library.Entities.Image image = (Library.Entities.Image)this.listMovieImages.SelectedItem;
			Movie movie = (Movie)image.ParentEntity;
			Library.Entities.Image posterImage = movie.Images.Where(i => i.ID == image.ID).OrderByDescending(i => i.Width).First();
			
			var service = new MovieService();
			service.SetPosterImage(movie, posterImage);
		}

		private void SetPosterImage_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
			e.CanExecute = this.listMovieImages != null && this.listMovieImages.SelectedItem != null && this.listMovieImages.SelectedItems.Count == 1;
		}
		#endregion
	}
}
