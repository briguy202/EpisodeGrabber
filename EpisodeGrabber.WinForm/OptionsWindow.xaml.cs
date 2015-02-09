using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using EpisodeGrabber.Library;
using CommonLibrary.Framework.Tracing;
using EpisodeGrabber.Controls.Options;

namespace EpisodeGrabber {
	/// <summary>
	/// Interaction logic for OptionsWindow.xaml
	/// </summary>
	public partial class OptionsWindow : Window {

		#region Declarations
		private UserConfiguration _configuration;
		private OptionsControl _openPanel;
		#endregion

		#region Constructors
		public OptionsWindow() {
			InitializeComponent();

			_configuration = UserConfiguration.GetConfiguration();
			this.lstItems_SelectionChanged(this.lstItems, null);
		}
		#endregion

		#region Events
		private void btnSave_Click(object sender, RoutedEventArgs e) {
			foreach (ListBoxItem item in this.lstItems.Items) {
				OptionsControl panel = this.GetControl((string)item.Content);
				if (panel != null) {
					panel.Save();
				}
			}
			_configuration.Save();
			this.Close();
		}

		private void lstItems_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			ListBox list = sender as ListBox;
			if (this.IsInitialized && list != null) {
				ListBoxItem selected = list.SelectedItem as ListBoxItem;
				if (selected != null) {
					string value = (string)selected.Content;
					this.lblOptions.Content = string.Concat(value, " Options");
					
					OptionsControl panel = this.GetControl(value);
					if (panel != null) {
						if (panel.Configuration == null) {
							panel.SetConfiguration(_configuration);
						}
						if (_openPanel != null) {
							_openPanel.Visibility = System.Windows.Visibility.Collapsed;
						}
						panel.Visibility = System.Windows.Visibility.Visible;
						_openPanel = panel;
					}
				}
			}
		}

		private OptionsControl GetControl(string name) {
			string findName = string.Concat("pnl", name.Replace(" ", string.Empty));
			OptionsControl panel = this.FindName(findName) as OptionsControl;
			return panel;
		}
		#endregion

	}
}