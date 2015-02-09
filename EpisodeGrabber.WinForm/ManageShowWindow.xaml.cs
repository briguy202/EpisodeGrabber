using System.Collections.Generic;
using System.Windows;
using EpisodeGrabber.Library;
using EpisodeGrabber.Library.Entities;

namespace EpisodeGrabber {
	/// <summary>
	/// Interaction logic for ManageShowWindow.xaml
	/// </summary>
	public partial class ManageShowWindow : Window {
		public Show ShowEntity { get; set; }
		public List<Show> Shows { get; set; }

		public ManageShowWindow() {
			InitializeComponent();

			this.tbxName.Focus();
		}

		private void button1_Click(object sender, RoutedEventArgs e) {
			List<Show> shows = new List<Show>();
			if (!string.IsNullOrWhiteSpace(this.tbxID.Text)) {
				//shows = new List<Show>() { Library.Entities.Show.GetShowByID(int.Parse(this.tbxID.Text)) };
			} else if (!string.IsNullOrWhiteSpace(this.tbxName.Text)) {
				//shows = Library.Entities.Show.GetShowsByName(this.tbxName.Text).Data;
			}

			if (shows.Count > 0) {
				shows.Sort(new System.Comparison<Show>((a, b) => b.Created.CompareTo(a.Created)));
				this.resultsGrid.DataContext = shows;
			} else {
				MessageBox.Show("No results were found for your search.");
			}
		}

		private void btnOK_Click(object sender, RoutedEventArgs e) {
			Show selectedShow = this.resultsGrid.SelectedItem as Show;
			if (selectedShow != null) {
				//Show show = Library.Entities.Show.GetShowByID(selectedShow.ID);
				//this.ShowEntity = show;
				this.DialogResult = true;
				this.Close();
			} else {
				MessageBox.Show("Please select a show first");
			}
		}

		private void resultsGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e) {
			this.btnOK_Click(null, null);
		}
	}
}