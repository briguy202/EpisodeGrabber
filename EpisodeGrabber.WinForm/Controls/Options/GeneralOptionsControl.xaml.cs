using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using CommonLibrary.Framework.Tracing;
using EpisodeGrabber.Library;

namespace EpisodeGrabber.Controls.Options {
	/// <summary>
	/// Interaction logic for GeneralOptionsControl.xaml
	/// </summary>
	public partial class GeneralOptionsControl : OptionsControl {

		#region Commands
		public static RoutedUICommand Add = new RoutedUICommand("Add", "Add", typeof(GeneralOptionsControl));
		public static RoutedUICommand Remove = new RoutedUICommand("Remove", "Remove", typeof(GeneralOptionsControl));
		public static RoutedUICommand RestoreDefaults = new RoutedUICommand("RestoreDefaults", "RestoreDefaults", typeof(GeneralOptionsControl));
		public static RoutedUICommand AddMediaType = new RoutedUICommand("AddMediaType", "AddMediaType", typeof(GeneralOptionsControl));
		#endregion

		#region Declarations
		private ObservableCollection<string> DefaultMediaTypes = new ObservableCollection<string>() { "avi", "divx", "m4v", "mpeg" };
		#endregion

		#region Constructors
		public GeneralOptionsControl() {
			InitializeComponent();
		}

		protected override void OnSetConfiguration() {
			base.OnSetConfiguration();

			if (this.Configuration.MediaTypes.Count == 0) {
				this.Configuration.MediaTypes = this.DefaultMediaTypes;
			}
			List<string> list = this.Configuration.MediaTypes.ToList();
			list.Sort();
			this.Configuration.MediaTypes = new ObservableCollection<string>(list);

			this.lstSupportedMediaTypes.DataContext = this.Configuration.MediaTypes;

			foreach (string folder in this.Configuration.ScanFolders) {
				ListBoxItem listItem = new ListBoxItem();
				listItem.Content = folder;
				if (!Directory.Exists(folder)) {
					TraceManager.Trace(string.Format("Unable to find directory {0} in the filesystem.", folder), TraceTypes.Error);
					listItem.Foreground = Brushes.Red;
					listItem.Content += " (directory not found)";
				}
				this.lstScanFolders.Items.Add(listItem);
			}

			if (this.Configuration.TraceVerbosity >= 0) {
				switch (this.Configuration.TraceVerbosity) {
					case TraceVerbosity.Minimal: this.radTracingMinimal.IsChecked = true; break;
					case TraceVerbosity.Verbose: this.radTracingVerbose.IsChecked = true; break;
					default: this.radTracingDefault.IsChecked = true; break;
				}
			} else {
				this.radTracingDefault.IsChecked = true;
			}

			this.tbxDownloadFormat.Text = this.Configuration.DownloadFormat;
			this.tbxDownloadFolder.Text = this.Configuration.DownloadFolder;
		}
		#endregion

		#region Methods
		protected override void OnSave() {
			base.OnSave();

			this.Configuration.ScanFolders.Clear();
			foreach (ListBoxItem item in this.lstScanFolders.Items) {
				this.Configuration.ScanFolders.Add((string)item.Content);
			}

			if (this.radTracingMinimal.IsChecked == true) { this.Configuration.TraceVerbosity = TraceVerbosity.Minimal; }
			if (this.radTracingDefault.IsChecked == true) { this.Configuration.TraceVerbosity = TraceVerbosity.Default; }
			if (this.radTracingVerbose.IsChecked == true) { this.Configuration.TraceVerbosity = TraceVerbosity.Verbose; }

			this.Configuration.ScanFolders.Sort((a, b) => a.CompareTo(b));
			this.Configuration.DownloadFormat = this.tbxDownloadFormat.Text;
			this.Configuration.DownloadFolder = this.tbxDownloadFolder.Text;
			TraceManager.SetVerbosity(this.Configuration.TraceVerbosity);
		}
		
		protected void AddFolder() {
			System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog();
			if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
				if (Directory.Exists(dialog.SelectedPath)) {
					ListBoxItem listItem = new ListBoxItem();
					listItem.Content = dialog.SelectedPath;
					this.lstScanFolders.Items.Add(listItem);
				} else {
					MessageBox.Show("The selected folder does not exist.", "Folder does not exist");
				}
			}
		}

		protected void RemoveFolders() {
			while (this.lstScanFolders.SelectedItems.Count > 0) {
				ListBoxItem item = this.lstScanFolders.SelectedItems[0] as ListBoxItem;
				this.lstScanFolders.Items.Remove(item);
			}
		}
		#endregion

		#region Events
		private void Add_Executed(object sender, ExecutedRoutedEventArgs e) {
			this.AddFolder();
		}

		private void Remove_Executed(object sender, ExecutedRoutedEventArgs e) {
			this.RemoveFolders();
		}

		private void Remove_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
			e.CanExecute = this.lstScanFolders.SelectedItem != null;
		}

		private void RestoreDefaults_Executed(object sender, ExecutedRoutedEventArgs e) {
			this.Configuration.MediaTypes.Clear();
			foreach (string type in this.DefaultMediaTypes) {
				this.Configuration.MediaTypes.Add(type);
			}
		}

		private void RestoreDefaults_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
			if (this.DefaultMediaTypes.All((t) => this.lstSupportedMediaTypes.Items.Contains(t)) &&
				this.DefaultMediaTypes.Count == this.lstSupportedMediaTypes.Items.Count) {
				e.CanExecute = false;
			} else {
				e.CanExecute = true;
			}
		}

		private void AddMediaType_Executed(object sender, ExecutedRoutedEventArgs e) {
			string value = this.tbxMediaType.Text;
			value = value.TrimStart('.');
			if (!string.IsNullOrWhiteSpace(value) && !this.Configuration.MediaTypes.Contains(value)) {
				this.Configuration.MediaTypes.Add(value);
			}
		}
		#endregion

	}
}