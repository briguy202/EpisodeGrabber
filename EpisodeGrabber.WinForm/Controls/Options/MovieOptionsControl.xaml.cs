using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using EpisodeGrabber.Library;

namespace EpisodeGrabber.Controls.Options {
	/// <summary>
	/// Interaction logic for MovieOptionsControl.xaml
	/// </summary>
	public partial class MovieOptionsControl : OptionsControl {
		public MovieOptionsControl() : base() {
			InitializeComponent();
		}

		protected override void OnSetConfiguration() {
			base.OnSetConfiguration();
			this.tbxMovieNameFormat.Text = this.Configuration.MovieNameFormat;
		}

		protected override void OnSave() {
			base.OnSave();
			this.Configuration.MovieNameFormat = this.tbxMovieNameFormat.Text;
		}
	}
}