using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CommonLibrary.Framework.Tracing;
using EpisodeGrabber.Library.Entities;

namespace EpisodeGrabber.Library.Services {
	public class SeasonService : EntityServiceBase, IEntityService {

		#region Enums
		public enum EpisodeListTypes {
			All,
			Some,
			None
		}
		#endregion

		#region Methods
		public void CreateFiles(EntityBase entity, bool overwrite, UserConfiguration configuration) {
			Season season = (Season)entity;
			string metadataDirectory = System.IO.Path.Combine(season.Path, "metadata");
			if (!Directory.Exists(metadataDirectory)) {
				DirectoryInfo dir = Directory.CreateDirectory(metadataDirectory);
				dir.Attributes = FileAttributes.Hidden;
			}

			// Download banner images
			string bannerFilePath = string.Concat(season.Path, "\\banner.jpg");
			if (!File.Exists(bannerFilePath)) {
				Library.Entities.Image image = this.GetBannerImage(season);
				if (image != null && !string.IsNullOrWhiteSpace(image.URL)) {
					EpisodeService.DownloadImage(image.URL, bannerFilePath);
				} else {
					TraceManager.TraceFormat("Unable to find a suitable banner image for {0}.", season.Name);
				}
			}

			foreach (Episode episode in season.Episodes) {
				if (!string.IsNullOrEmpty(episode.Path)) {
					new EpisodeService().CreateFiles(episode, overwrite, configuration);
				}
			}
		}

		public override Image GetBannerImage(EntityBase entity) {
			Season season = (Season)entity;
			Image image = season.Images.FirstOrDefault((a) => a.MappedType == ImageType.SeasonWide || a.MappedType == ImageType.Graphical);
			if (image == null) {
				image = new ShowService().GetBannerImage(season.Parent);
			}

			return image;
		}
		#endregion

		public void Initialize(EntityBase entity, IEnumerable<string> mediaTypes) {
			// do nothing
		}
	}
}
