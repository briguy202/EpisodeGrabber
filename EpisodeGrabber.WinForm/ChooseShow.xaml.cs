using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using EpisodeGrabber.Library.Entities;

namespace EpisodeGrabber {
	/// <summary>
	/// Interaction logic for ChooseShow.xaml
	/// </summary>
	public partial class ChooseShow : Window {
		public EntityBase SelectedItem { get; set; }
		public static RoutedUICommand SelectShow = new RoutedUICommand("SelectShow", "SelectShow", typeof(ChooseShow));

		public ChooseShow(string searchTerm, List<EntityBase> entities) {
			InitializeComponent();

			this.dgShows.DataContext = entities;

			this.txtHeader.Text = this.txtHeader.Text.Replace("%COUNT%", entities.Count.ToString());
			this.txtHeader.Text = this.txtHeader.Text.Replace("%NAME%", searchTerm);
		}

		private void Select_Execute(object sender, ExecutedRoutedEventArgs e) {
			this.DialogResult = true;
			this.SelectedItem = (EntityBase)this.dgShows.SelectedItem;
			this.Close();
		}

		private void SelectShow_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
			e.CanExecute = this.dgShows.SelectedItems.Count > 0;
		}
	}
}
