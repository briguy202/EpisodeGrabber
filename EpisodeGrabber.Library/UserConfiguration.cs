using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using EpisodeGrabber.Library.Entities;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace EpisodeGrabber.Library {
	public class UserConfiguration {

		#region Declarations
		public const string SettingsFileName = "EpisodeGrabber\\settings.xml";
		#endregion

		#region Properties
		public int TraceVerbosity { get; set; }
		public List<string> ScanFolders { get; set; }
		public ObservableCollection<string> MediaTypes { get; set; }
		public string DownloadFormat { get; set; }
		public string DownloadFolder { get; set; }
		public string MovieNameFormat { get; set; }
		#endregion

		#region Constructors
		public UserConfiguration() {
			this.ScanFolders = new List<string>();
			this.MediaTypes = new ObservableCollection<string>();
			this.TraceVerbosity = -1;
		}
		#endregion

		#region Methods
		public static UserConfiguration GetConfiguration() {
			UserConfiguration _return = null;
			string settingsFilePath = UserConfiguration.GetSettingsFilePath();

			if (!File.Exists(settingsFilePath)) {
				_return = new UserConfiguration();
			} else {
				XmlSerializer serializer = new XmlSerializer(typeof(UserConfiguration));
				using (XmlReader reader = XmlReader.Create(settingsFilePath)) {
					_return = serializer.Deserialize(reader) as UserConfiguration;
				}
			}

			return _return;
		}

		public void Save() {
			string settingsFilePath = UserConfiguration.GetSettingsFilePath();
			DirectoryInfo settingsFileDirectory = Directory.GetParent(settingsFilePath);
			if (!settingsFileDirectory.Exists) {
				settingsFileDirectory.Create();
			}
			XmlSerializer serializer = new XmlSerializer(typeof(UserConfiguration));
			using (XmlWriter writer = XmlWriter.Create(settingsFilePath, new XmlWriterSettings() { Indent = true })) {
				serializer.Serialize(writer, this);
			}
		}

		public static string GetSettingsFilePath() {
			string appFolderPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData);
			return string.Concat(appFolderPath, "\\", UserConfiguration.SettingsFileName);
		}
		#endregion

	}
}
