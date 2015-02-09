using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using CommonLibrary.Framework;
using CommonLibrary.Framework.Tracing;
using CommonLibrary.Framework.Validation;
using EpisodeGrabber.Library.Entities;

namespace EpisodeGrabber.Library.DAO {
	public class TheTVDBDAO {
		private const string ApiKey = "9FF850D5F2F3170F";

		public BusinessObject<List<EntityBase>> GetShowsByName(string name) {
			var shows = new BusinessObject<List<EntityBase>>();

			string url = string.Format("http://www.thetvdb.com/api/GetSeries.php?seriesname={0}", name);
			XmlDocument xml = new XmlDocument();
			try {
				xml.Load(url);
			} catch (System.Net.WebException) {
				shows.Errors.Add(string.Format("The web request to {0} timed out.", url));
			}

			if (xml != null && xml.HasChildNodes) {
				shows.Data = new List<EntityBase>();

				foreach (XmlNode seriesNode in xml.SelectNodes("/Data/Series")) {
					Show show = this.GetShowFromXML(seriesNode);
					shows.Data.Add(show);
				}
			}

			return shows;
		}

		/// <summary>
		/// Gets a show from TheTVDB.com.
		/// Example URL (Dexter): http://www.thetvdb.com/api/9FF850D5F2F3170F/series/79349/all/en.xml
		/// </summary>
		/// <param name="showID"></param>
		/// <returns></returns>
		public BusinessObject<EntityBase> GetShowByID(int showID) {
			Show show = null;
			string url = this.GetShowDataURL(showID);
			XmlDocument showXML = new XmlDocument();
			showXML.Load(url);

			if (showXML.HasChildNodes) {
				show = new Show();

				show.ID = showXML.GetNodeValue<int>("/Data/Series/id", int.MinValue);
				show.Name = showXML.GetNodeValue<string>("/Data/Series/SeriesName");
				show.AirDay = showXML.GetNodeValue<string>("/Data/Series/Airs_DayOfWeek");
				show.AirTime = showXML.GetNodeValue<string>("/Data/Series/Airs_Time");
				show.Rating = showXML.GetNodeValue<string>("/Data/Series/Rating");
				show.ContentRating = showXML.GetNodeValue<string>("/Data/Series/ContentRating");
				show.Runtime = showXML.GetNodeValue<int>("/Data/Series/Runtime", 0);
				show.Created = DateTime.Parse(showXML.GetNodeValue<string>("/Data/Series/FirstAired"));
				show.IMDB_ID = showXML.GetNodeValue<string>("/Data/Series/IMDB_ID");
				show.Network = showXML.GetNodeValue<string>("/Data/Series/Network");
				show.Description = showXML.GetNodeValue<string>("/Data/Series/Overview");
				show.Status = showXML.GetNodeValue<string>("/Data/Series/Status");
				show.MainBannerImageURL = this.GetImageURL(showXML.GetNodeValue<string>("/Data/Series/banner"));
				show.Actors = new List<string>(showXML.GetNodeValue<string>("/Data/Series/Actors").Trim('|').Split('|'));
				show.Genres = new List<string>(showXML.GetNodeValue<string>("/Data/Series/Genre").Trim('|').Split('|'));

				foreach (XmlNode episodeNode in showXML.SelectNodes("/Data/Episode")) {
					string seasonNumberValue = episodeNode.GetNodeValue<string>("Combined_season");
					int seasonNumber = int.MinValue;
					if (!string.IsNullOrEmpty(seasonNumberValue) && int.TryParse(seasonNumberValue, out seasonNumber) && seasonNumber > 0) {
						Season season = show.Seasons.SingleOrDefault((s) => s.Number == seasonNumber);
						if (season == null) {
							season = new Season();
							season.Number = seasonNumber;
							season.Name = string.Format("Season {0}", seasonNumber);
							show.Seasons.Add(season);
						}

						Episode episode = new Episode();
						if (!episodeNode.SelectSingleNode("FirstAired").IsNullOrEmpty()) {
							episode.Created = DateTime.Parse(episodeNode.GetNodeValue<string>("FirstAired"));
						}

						episode.ID = episodeNode.GetNodeValue<int>("id", int.MinValue);
						episode.Name = episodeNode.GetNodeValue<string>("EpisodeName");
						episode.EpisodeNumber = episodeNode.GetNodeValue<int>("EpisodeNumber", int.MinValue);
						episode.SeasonNumber = seasonNumber;
						episode.Description = episodeNode.GetNodeValue<string>("Overview", string.Empty);
						episode.DVD_Chapter = episodeNode.GetNodeValue<int>("DVD_chapter", int.MinValue);
						episode.DVD_DiscID = episodeNode.GetNodeValue<int>("DVD_discid", int.MinValue);
						episode.DVD_EpisodeNumber = episodeNode.GetNodeValue<double>("DVD_episodenumber", double.MinValue);
						episode.DVD_SeasonNumber = episodeNode.GetNodeValue<int>("DVD_season", int.MinValue);
						episode.Director = episodeNode.GetNodeValue<string>("Director");
						episode.GuestStars = new List<string>(episodeNode.GetNodeValue<string>("GuestStars", string.Empty).Trim('|').Split('|'));
						episode.IMDB_ID = episodeNode.GetNodeValue<string>("IMDB_ID");
						episode.Language = episodeNode.GetNodeValue<string>("Language");
						episode.ProductionCode = episodeNode.GetNodeValue<int>("ProductionCode", int.MinValue);
						episode.Rating = episodeNode.GetNodeValue<double>("Rating", double.MinValue);
						episode.Writer = episodeNode.GetNodeValue<string>("Writer");
						episode.SeasonID = episodeNode.GetNodeValue<int>("seasonid", int.MinValue);
						episode.SeriesID = episodeNode.GetNodeValue<int>("seriesid", int.MinValue);
						episode.Images.Add(new Image() {
							URL = this.GetImageURL(episodeNode.GetNodeValue<string>("filename")),
							ScopeType = ImageScopeType.Episode
						});

						season.Episodes.Add(episode);
					}
				}

				List<Image> images = this.GetImages(show.ID);
				foreach (Image image in images) {
					if (image.ScopeType == ImageScopeType.Season) {
						Season season = show.Seasons.FirstOrDefault((a) => a.Number == image.SeasonNumber);
						if (season != null) {
							season.Images.Add(image);
						}
					} else {
						show.Images.Add(image);
					}
				}
			}

			var returnObj = new BusinessObject<EntityBase>();
			returnObj.Data = show;
			return returnObj;
		}

		public string GetShowDataURL(int showID) {
			string url = string.Format("http://www.thetvdb.com/api/{0}/series/{1}/all/en.xml", ApiKey, showID.ToString());
			return url;
		}

		public List<Image> GetImages(int showID) {
			List<Image> images = new List<Image>();
			string url = string.Format("http://www.thetvdb.com/api/{0}/series/{1}/banners.xml", ApiKey, showID.ToString());
			TraceManager.Trace(string.Format("Retrieving images from {0}.", url), TraceVerbosity.Verbose, TraceTypes.OperationStarted);
			XmlDocument doc = new XmlDocument();
			doc.Load(url);

			if (doc != null && doc.HasChildNodes) {
				foreach (XmlNode node in doc.SelectNodes("/Banners/Banner")) {
					Image image = new Image();
					image.ID = node.GetNodeValue<string>("id");
					image.ScopeType = Image.MapImageScopeTypeValue(node.GetNodeValue<string>("BannerType"));
					image.MappedType = Image.MapImageTypeValue(node.GetNodeValue<string>("BannerType"), node.GetNodeValue<string>("BannerType2"));
					image.URL = this.GetImageURL(node.GetNodeValue<string>("BannerPath"));
					image.ThumbnailURL = this.GetImageURL(node.GetNodeValue<string>("ThumbnailPath"));
					image.SeasonNumber = node.GetNodeValue<int>("Season", -1);

					string dimensionValue = null;
					Regex regex = new Regex("\\d+?x\\d+?");
					string bannerType1 = node.GetNodeValue<string>("BannerType");
					string bannerType2 = node.GetNodeValue<string>("BannerType2");
					dimensionValue = (regex.IsMatch(bannerType1)) ? bannerType1 : (regex.IsMatch(bannerType2)) ? bannerType2 : null;

					if (!string.IsNullOrEmpty(dimensionValue)) {
						string[] sizes = dimensionValue.Split('x');
						image.Width = int.Parse(sizes[0]);
						image.Height = int.Parse(sizes[1]);
					}

					images.Add(image);
				}
			}
			return images;
		}

		protected string GetServerTime() {
			string serverTime = null;
			string url = "http://www.thetvdb.com/api/Updates.php?type=none";
			XmlDocument serverTimeXML = new XmlDocument();
			serverTimeXML.Load(url);

			if (serverTimeXML.HasChildNodes) {
				serverTime = serverTimeXML.SelectSingleNode("//Time").InnerText;
			}
			return serverTime;
		}

		protected string GetImageURL(string relativePath) {
			string value = null;
			if (!string.IsNullOrWhiteSpace(relativePath)) {
				value = string.Format("http://www.thetvdb.com/banners/{0}", relativePath);
			}
			return value;
		}

		protected Show GetShowFromXML(XmlNode seriesNode) {
			ValidationUtility.ThrowIfNullOrEmpty(seriesNode, "seriesNode");
			Show show = new Show();

			show.ID = int.Parse(seriesNode.SelectSingleNode("seriesid").InnerText);
			show.Name = seriesNode.GetNodeValue<string>("SeriesName");
			show.Description = seriesNode.GetNodeValue<string>("Overview");
			if (!seriesNode.SelectSingleNode("FirstAired").IsNullOrEmpty()) {
				show.Created = seriesNode.GetNodeValue<DateTime>("FirstAired");
			}

			return show;
		}
	}
}
