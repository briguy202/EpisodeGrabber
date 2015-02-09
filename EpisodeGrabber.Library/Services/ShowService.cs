using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using CommonLibrary.Framework;
using CommonLibrary.Framework.Tracing;
using CommonLibrary.Framework.Validation;
using EpisodeGrabber.Library.DAO;
using EpisodeGrabber.Library.Entities;

namespace EpisodeGrabber.Library.Services {
	public class ShowService : EntityServiceBase, IFetchService, IEntityService {

		#region Constants
		public const string SeasonEpisodeRegex = @"S\d{1,2}E\d{1,2}";
		#endregion

		#region Methods
		public static bool IsShowFile(string filename) {
			bool match = Regex.IsMatch(filename, ShowService.SeasonEpisodeRegex, RegexOptions.IgnoreCase);
			return match;
		}

		public void Initialize(EntityBase entity, IEnumerable<string> mediaTypes) {
			Show show = (Show)entity;
			foreach (Season season in show.Seasons) {
				season.Parent = show;
				DirectoryInfo seasonDirectory = null;
				bool allEpisodesExist = true;
				bool allExistingEpisodesExist = true;
				bool someEpisodesExist = false;

				DirectoryInfo showDirectory = new DirectoryInfo(show.Path);
				if (showDirectory.Exists) {
					string seasonName = season.Name ?? season.ToString();
					DirectoryInfo[] directories = showDirectory.GetDirectories(seasonName);
					if (directories != null && directories.Length == 1) {
						seasonDirectory = directories[0];
						season.Path = seasonDirectory.FullName;
					}
				}

				foreach (Episode episode in season.Episodes) {
					episode.Parent = season;

					if (episode.Created > DateTime.MinValue) {
						FileInfo episodeFile = null;
						if (seasonDirectory != null && seasonDirectory.Exists) {
							var files = seasonDirectory.GetFiles(string.Format("*S{0}E{1}*", season.Number.ToString("D2"), episode.EpisodeNumber.ToString("D2"))).Where((f) => mediaTypes.Contains(f.Extension.TrimStart('.'))).ToList();;
							if (files != null && files.Count() == 1) {
								episodeFile = files[0];
								episode.Path = episodeFile.FullName;
							}
						}

						if (episodeFile != null) {
							someEpisodesExist = true;
						} else {
							allEpisodesExist = false;

							// Check if the episode has aired yet.
							if (episode.Created < DateTime.Now) {
								allExistingEpisodesExist = false;
							}
						}
					} else {
						TraceManager.Trace(string.Format("{0} is being skipped due to first aired date of {1}.", episode.Name, episode.Created.ToString()), TraceVerbosity.Verbose);
					}
				}

				season.DownloadedAllEpisodes = allEpisodesExist;
				season.DownloadedAllExistingEpisodes = allExistingEpisodesExist;
				season.DownloadedSomeEpisodes = someEpisodesExist;
				season.DownloadedNoEpisodes = (someEpisodesExist == false);
			}
		}

		public void CreateFiles(EntityBase entity, bool overwrite, UserConfiguration configuration) {
			Show show = (Show)entity;
			DirectoryInfo directory = new DirectoryInfo(show.Path);
			if (directory.Exists) {
				string filePath = string.Concat(directory.FullName, "\\series.xml");
				if (File.Exists(filePath)) {
					if (overwrite) {
						File.Delete(filePath);
					} else {
						return;
					}
				}

				XmlDocument doc = new XmlDocument();
				XmlNode rootNode = doc.AddNode("Series");
				rootNode.AddNode("id", show.ID.ToString());
				rootNode.AddNode("Actors", string.Concat("|", string.Join("|", show.Actors), "|"));
				rootNode.AddNode("ContentRating", show.ContentRating);
				rootNode.AddNode("FirstAired", show.Created.ToString("yyyy-MM-dd"));
				rootNode.AddNode("Genre", string.Concat("|", string.Join("|", show.Genres), "|"));
				rootNode.AddNode("IMDbId", show.IMDB_ID);
				rootNode.AddNode("IMDB_ID", show.IMDB_ID);
				rootNode.AddNode("Overview", show.Description);
				rootNode.AddNode("Network", show.Network);
				rootNode.AddNode("Rating", show.Rating);
				rootNode.AddNode("Runtime", show.Runtime.ToString());
				rootNode.AddNode("SeriesName", show.Name);
				rootNode.AddNode("Status", show.Status);

				// Save the file
				doc.Save(filePath);

				// Download banner images
				string bannerFilePath = System.IO.Path.Combine(show.Path, "banner.jpg");
				if (!File.Exists(bannerFilePath)) {
					Library.Entities.Image image = this.GetBannerImage(show);
					if (image != null && !string.IsNullOrWhiteSpace(image.URL)) {
						EpisodeService.DownloadImage(image.URL, bannerFilePath);
					} else {
						TraceManager.TraceFormat("Unable to find a suitable banner image for {0}.", show.Name);
					}
				}

				// Download poster image
				string posterFile = System.IO.Path.Combine(show.Path, "folder.jpg");
				if (!File.Exists(posterFile)) {
					Image poster = show.Images.Where((i) => i.MappedType == ImageType.Poster).OrderByDescending((i) => i.Height).ToList().FirstOrDefault();
					if (poster != null) {
						TraceManager.Trace("Downloading poster image ...", TraceTypes.OperationStarted);
						EpisodeService.DownloadImage(poster.URL, posterFile);
					} else {
						TraceManager.Trace(string.Format("Unable to find a poster file for {0}.", show.Name), TraceTypes.Error);
					}
				}

				foreach (Season season in show.Seasons) {
					if (!string.IsNullOrEmpty(season.Path)) {
						new SeasonService().CreateFiles(season, overwrite, configuration);
					}
				}
			} else {
				throw new DirectoryNotFoundException(string.Format("Directory {0} was not found.", directory.FullName));
			}
		}

		public override Image GetBannerImage(EntityBase entity) {
			Show show = (Show)entity;
			return show.Images.FirstOrDefault((a) => a.MappedType == ImageType.SeasonWide || a.MappedType == ImageType.Graphical);
		}

		public BusinessObject<List<EntityBase>> FetchMetadataByName(string name) {
			ValidationUtility.ThrowIfNullOrEmpty(name, "name");

			name = this.ParseName(name);
			var entities = new TheTVDBDAO().GetShowsByName(name);
			entities.Data.ForEach((e) => e.Updated = DateTime.Now);
			return entities;
		}

		public BusinessObject<EntityBase> FetchMetadataByID(int id) {
			ValidationUtility.ThrowIfLessThan(id, 1, "id");

			var entity = new TheTVDBDAO().GetShowByID(id);
			entity.Data.Updated = DateTime.Now;
			return entity;
		}
		#endregion
	}
}
