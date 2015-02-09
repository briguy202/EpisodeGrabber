using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using CommonLibrary.Framework;
using CommonLibrary.Framework.Tracing;
using CommonLibrary.Framework.Validation;
using EpisodeGrabber.Library.Entities;

namespace EpisodeGrabber.Library.Services {
	public class EpisodeService : EntityServiceBase, IEntityService {

		public Image GetThumbnailImage(Episode episode) {
			return episode.Images.FirstOrDefault((i) => i.ScopeType == ImageScopeType.Episode);
		}

		public string GetIdentifier(Episode episode, bool padWithZeros) {
			string seasonValue = (padWithZeros) ? episode.SeasonNumber.ToString().PadLeft(2, '0') : episode.SeasonNumber.ToString();
			string episodeValue = (padWithZeros) ? episode.EpisodeNumber.ToString().PadLeft(2, '0') : episode.EpisodeNumber.ToString();
			string value = string.Format("S{0}E{1}", seasonValue, episodeValue);
			return value;
		}

		/// <summary>
		/// Returns the episode identifier and name (ex. S02E04 - Some Episode)
		/// </summary>
		/// <param name="padWithZeros">Whether leading zeros should be used in the season and episode numbers</param>
		/// <param name="useSafeName">Whether the episode name should have invalid path characters removed.</param>
		/// <returns></returns>
		public string GetIdentifierAndName(Episode episode, bool padWithZeros, bool useSafeName) {
			string name = (useSafeName) ? episode.Name.ReplaceInvalidPathCharacters(string.Empty) : episode.Name;
			string value = string.Format("{0} - {1}", this.GetIdentifier(episode, padWithZeros), name);
			return value;
		}

		public void CreateFiles(EntityBase entity, bool overwrite, UserConfiguration configuration) {
			Episode episode = (Episode)entity;
			DirectoryInfo seasonDirectory = new DirectoryInfo(episode.Parent.Path);
			DirectoryInfo metadataDirectory = new DirectoryInfo(System.IO.Path.Combine(seasonDirectory.FullName, "metadata"));

			if (seasonDirectory.Exists) {
				XmlDocument doc = new XmlDocument();
				string episodeName = this.GetIdentifierAndName(episode, true, true);
				string episodeImageName = string.Concat(episodeName, ".jpg");
				string episodeXmlName = string.Concat(episodeName, ".xml");
				string episodeFilePath = System.IO.Path.Combine(metadataDirectory.FullName, episodeXmlName);

				if (overwrite && File.Exists(episodeFilePath)) { File.Delete(episodeFilePath); }
				FileInfo episodeFile = new FileInfo(episodeFilePath);
				XmlNode rootNode = null;

				if (!episodeFile.Exists) {
					Directory.CreateDirectory(episodeFile.DirectoryName);
					rootNode = doc.AddNode("Item");
				} else {
					doc.Load(episodeFilePath);
					rootNode = doc.SelectSingleNode("//Item");
				}

				// Download the episode thumbnail image for the episode if it doesn't already exists.
				string imageFilePath = System.IO.Path.Combine(metadataDirectory.FullName, episodeImageName);
				if (!File.Exists(imageFilePath)) {
					Image thumbnail = this.GetThumbnailImage(episode);
					if (thumbnail != null && !string.IsNullOrEmpty(thumbnail.URL)) {
						TraceManager.TraceFormat("Downloading image {0} to {1}", thumbnail.URL, imageFilePath);
						EpisodeService.DownloadImage(thumbnail.URL, imageFilePath);
					} else {
						TraceManager.Trace(string.Format("Unable to find thumbnail image for {0}", episode.Name), TraceTypes.Error);
					}
				}

				// Rename the file if necessary
				string currentEpisodeName = System.IO.Path.GetFileNameWithoutExtension(episode.Path);
				string extension = System.IO.Path.GetExtension(episode.Path);
				string newPath = string.Concat(System.IO.Path.Combine(seasonDirectory.FullName, episodeName), extension);
				MatchCollection matches = Regex.Matches(currentEpisodeName, ShowService.SeasonEpisodeRegex, RegexOptions.IgnoreCase);

				if (currentEpisodeName != episodeName && matches.Count == 1) {
					TraceManager.Trace(string.Format("Renaming {0} to {1}", episode.Path, newPath), TraceTypes.OperationStarted);
					File.Move(episode.Path, newPath);
				} else if (matches.Count > 1) {
					TraceManager.Trace(string.Format("Skipping renaming \"{0}\" to \"{1}\" because there were ({2}) season/episode numbers in the filename.", currentEpisodeName, episodeName, matches.Count.ToString()), TraceVerbosity.Minimal);
				}

				rootNode.AddNode("ID", episode.EpisodeNumber.ToString());
				rootNode.AddNode("EpisodeID", episode.ID.ToString());
				rootNode.AddNode("EpisodeName", episode.Name);
				rootNode.AddNode("EpisodeNumber", episode.EpisodeNumber.ToString());
				rootNode.AddNode("FirstAired", episode.Created.ToString("yyyy-MM-dd"));
				rootNode.AddNode("Overview", episode.Description);
				rootNode.AddNode("DVD_chapter", EpisodeService.WriteIntValue(episode.DVD_Chapter));
				rootNode.AddNode("DVD_discid", EpisodeService.WriteIntValue(episode.DVD_DiscID));
				rootNode.AddNode("DVD_episodenumber", EpisodeService.WriteDoubleValue(episode.DVD_EpisodeNumber));
				rootNode.AddNode("DVD_season", EpisodeService.WriteIntValue(episode.DVD_SeasonNumber));
				rootNode.AddNode("Director", episode.Director);
				rootNode.AddNode("GuestStars", string.Concat("|", string.Join("|", episode.GuestStars), "|"));
				rootNode.AddNode("IMDB_ID", episode.IMDB_ID);
				rootNode.AddNode("Language", episode.Language);
				rootNode.AddNode("ProductionCode", EpisodeService.WriteIntValue(episode.ProductionCode));
				rootNode.AddNode("Rating", EpisodeService.WriteDoubleValue(episode.Rating));
				rootNode.AddNode("Writer", episode.Writer);
				rootNode.AddNode("SeasonNumber", EpisodeService.WriteIntValue(episode.SeasonNumber));
				rootNode.AddNode("absolute_number");
				rootNode.AddNode("seasonid", EpisodeService.WriteIntValue(episode.SeasonID));
				rootNode.AddNode("seriesid", EpisodeService.WriteIntValue(episode.SeriesID));
				rootNode.AddNode("filename", string.Concat("/", episodeImageName));

				doc.Save(episodeFilePath);
			} else {
				throw new DirectoryNotFoundException(string.Format("Directory {0} was not found.", seasonDirectory.FullName));
			}
		}

		public override Image GetBannerImage(EntityBase entity) {
			Episode episode = (Episode)entity;
			Image image = episode.Images.FirstOrDefault((a) => a.MappedType == ImageType.SeasonWide || a.MappedType == ImageType.Graphical);
			if (image == null) {
				image = new SeasonService().GetBannerImage(episode.Parent);
			}

			return image;
		}

		public static bool DownloadImage(string url, string filename) {
			ValidationUtility.ThrowIfNullOrEmpty(url, "url");
			ValidationUtility.ThrowIfNullOrEmpty(filename, "filename");

			ImageFormat imageType = ImageFormat.Jpeg;
			WebResponse response = null;
			Stream remoteStream = null;
			StreamReader readStream = null;

			try {
				TraceManager.Trace(string.Format("Making image request for {0}.", url), TraceVerbosity.Minimal, TraceTypes.WebRequest);
				WebRequest request = WebRequest.Create(url);
				if (request != null) {
					response = request.GetResponse();
					TraceManager.Trace("Request completed.", TraceTypes.Default);
					if (response != null) {
						remoteStream = response.GetResponseStream();
						string content_type = response.Headers["Content-type"];

						if (content_type == "image/jpeg" || content_type == "image/jpg") {
							imageType = ImageFormat.Jpeg;
						} else if (content_type == "image/png") {
							imageType = ImageFormat.Png;
						} else if (content_type == "image/gif") {
							imageType = ImageFormat.Gif;
						} else {
							TraceManager.TraceFormat("Unrecognized image type '{0}' at {1}, aborting image download.", content_type, url);
							return false;
						}

						readStream = new StreamReader(remoteStream);
						System.Drawing.Image image = System.Drawing.Image.FromStream(remoteStream);
						if (image == null) { return false; }

						TraceManager.Trace(string.Format("Saving file to {0}.", filename), TraceTypes.Default);
						FileInfo file = new FileInfo(filename);
						if (!file.Directory.Exists) {
							file.Directory.Create();
						}
						if (file.Exists) {
							file.Delete();
						}
						image.Save(filename, imageType);
						image.Dispose();
					} else {
						TraceManager.Trace(string.Format("Response for {0} was null or empty.", url), TraceTypes.Error);
					}
				}
			} catch (Exception ex) {
				TraceManager.Trace(string.Format("Download image failed: {0}", ex.Message), TraceVerbosity.Minimal, TraceTypes.Exception, ex);
			} finally {
				if (response != null) response.Close();
				if (remoteStream != null) remoteStream.Close();
				if (readStream != null) readStream.Close();
			}

			return true;
		}

		public static void AddToImagesFile(string showDirectoryPath, Image image, string imageFilePath) {
			ValidationUtility.ThrowIfNullOrEmpty(showDirectoryPath, "showDirectoryPath");
			ValidationUtility.ThrowIfNullOrEmpty(image, "image");
			ValidationUtility.ThrowIfNullOrEmpty(imageFilePath, "imageFilePath");

			DirectoryInfo showDirectory = new DirectoryInfo(showDirectoryPath);
			if (showDirectory.Exists) {
				XmlDocument doc = new XmlDocument();
				XmlNode imagesNode = null;
				string imagesFilePath = string.Concat(showDirectory.FullName, "\\Images.xml");
				FileInfo imagesFile = new FileInfo(imagesFilePath);

				if (!imagesFile.Exists) {
					XmlNode rootNode = doc.AddNode("Root");
					imagesNode = rootNode.AddNode("Images");
				} else {
					doc.Load(imagesFilePath);
					imagesNode = doc.SelectSingleNode("//Root/Images");
					if (imagesNode == null) {
						imagesNode = doc.SelectSingleNode("//Root").AddNode("Images");
					}
				}

				XmlNode imageNode = imagesNode.AddNode("Image");
				imageNode.AddNode("ID", image.ID.ToString());
				imageNode.AddNode("Path", imageFilePath);

				doc.Save(imagesFile.FullName);
			} else {
				throw new DirectoryNotFoundException(string.Format("Directory {0} was not found.", showDirectoryPath));
			}
		}

		public static string WriteIntValue(int value) {
			return (value > 0) ? value.ToString() : string.Empty;
		}

		public static string WriteDoubleValue(double value) {
			return (value > 0) ? value.ToString() : string.Empty;
		}

		public void Initialize(EntityBase entity, IEnumerable<string> mediaTypes) {
			// do nothing
		}
	}
}
