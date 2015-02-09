using System.Windows;
using EpisodeGrabber.Library.Entities;
using System.Collections.Generic;
using System.Diagnostics;

namespace EpisodeGrabber {
	/// <summary>
	/// Interaction logic for ManageImages.xaml
	/// </summary>
	public partial class ManageImages : Window {
		public ManageImages() {
			InitializeComponent();
		}

		private void StackPanel_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e) {
			if (e.ClickCount == 2) {
				Image image = this.lstImages.SelectedItem as Image;
				if (image != null) {
					Process.Start(image.URL);
				}
			}
		}
	}
}