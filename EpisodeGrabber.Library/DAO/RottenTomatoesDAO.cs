using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Linq;
using System.Web.Script.Serialization;
using CommonLibrary.Framework;
using CommonLibrary.Framework.Validation;
using EpisodeGrabber.Library.Entities;
using CommonLibrary.Framework.Tracing;

namespace EpisodeGrabber.Library.DAO {
	public class RottenTomatoesDAO {
		private const string ApiKey = "dgknfyd952s8gkmrgj5rd9xm";

		public BusinessObject<List<RottenTomatoesData>> GetByName(string name) {
			ValidationUtility.ThrowIfNullOrEmpty(name, "name");

			// Example: http://api.rottentomatoes.com/api/public/v1.0/movies.json?q=Sideways&page_limit=10&apikey=dgknfyd952s8gkmrgj5rd9xm
			BusinessObject<List<RottenTomatoesData>> entities = new BusinessObject<List<RottenTomatoesData>>();
			string url = string.Format("http://api.rottentomatoes.com/api/public/v1.0/movies.json?q={0}&page_limit=10&apikey={1}", WebUtility.HtmlEncode(name), RottenTomatoesDAO.ApiKey);
			Dictionary<string, object> data = this.RequestData(url);
			
			if (data != null) {
				ArrayList moviesData = data["movies"] as ArrayList;
				if (moviesData.Count > 0) {
					entities.Data = new List<RottenTomatoesData>();
					foreach (Dictionary<string, object> movieData in moviesData) {
						RottenTomatoesData movie = this.MapToMovie(movieData);
						entities.Data.Add(movie);
					}
				}
			}

			return entities;
		}

		public BusinessObject<RottenTomatoesData> GetByID(int id) {
			ValidationUtility.ThrowIfNullOrEmpty(id, "id");

			BusinessObject<RottenTomatoesData> entity = new BusinessObject<RottenTomatoesData>();
			string url = string.Format("http://api.rottentomatoes.com/api/public/v1.0/movies/{0}.json?apikey={1}", id.ToString(), RottenTomatoesDAO.ApiKey);
			Dictionary<string, object> data = this.RequestData(url);

			if (data != null) {
				RottenTomatoesData movie = this.MapToMovie(data);
				entity.Data = movie;
			}

			return entity;
		}

		public BusinessObject<RottenTomatoesData> GetByIMDB(string id) {
			ValidationUtility.ThrowIfNullOrEmpty(id, "id");

			BusinessObject<RottenTomatoesData> entity = new BusinessObject<RottenTomatoesData>();
			string url = string.Format("http://api.rottentomatoes.com/api/public/v1.0/movie_alias.json?id={0}&type=imdb", id);
			Dictionary<string, object> data = this.RequestData(url);

			if (data != null) {
				RottenTomatoesData movie = this.MapToMovie(data);
				entity.Data = movie;
			}

			return entity;
		}

		private Dictionary<string, object> RequestData(string url) {
			ValidationUtility.ThrowIfNullOrEmpty(url, "url");
			TraceManager.Trace(string.Format("Requesting url '{0}' ...", url.Replace(RottenTomatoesDAO.ApiKey, "[Key]")), TraceTypes.OperationStarted);
			WebRequest request = HttpWebRequest.Create(url);
			WebResponse response = null;
			Dictionary<string, object> data = null;

			try {
				response = request.GetResponse();
			} catch (Exception ex) {
				TraceManager.Trace(string.Format("Unable to make request: Exception: {0}.  URL: {1}", ex.Message, url), TraceTypes.Error);
			}

			if (response != null) {
				string responseValue;
				using (StreamReader reader = new StreamReader(response.GetResponseStream())) {
					responseValue = reader.ReadToEnd();
				}

				if (!string.IsNullOrWhiteSpace(responseValue)) {
					JavaScriptSerializer serializer = new JavaScriptSerializer();
					data = serializer.Deserialize<Dictionary<string, object>>(responseValue);
				}
			}

			return data;
		}

		private RottenTomatoesData MapToMovie(Dictionary<string, object> movieData) {
			ValidationUtility.ThrowIfNullOrEmpty(movieData, "movieData");
			RottenTomatoesData data = new RottenTomatoesData();
			data.ID = movieData.GetData<int>("id");
			data.Name = movieData.GetData<string>("title");
			data.MPAARating = movieData.GetData<string>("mpaa_rating");
			data.Studio = movieData.GetData<string>("studio");
			data.Runtime = movieData.GetData<int>("runtime");
			data.Description = string.Concat(movieData.GetData<string>("synopsis"), movieData.GetData<string>("critics_consensus"));

			Dictionary<string, object> releaseDates = movieData.GetData<Dictionary<string, object>>("release_dates");
			data.Created = releaseDates.GetData<DateTime>("theater");

			Dictionary<string, object> alternateIDs = movieData.GetData<Dictionary<string, object>>("alternate_ids");
			data.IMDBId = string.Concat("tt", alternateIDs.GetData<int>("imdb").ToString());

			Dictionary<string, object> ratingData = movieData.GetData<Dictionary<string, object>>("ratings");
			data.CriticsRating = new Rating();
			data.CriticsRating.Description = ratingData.GetData<string>("critics_rating");
			data.CriticsRating.Score = ratingData.GetData<int>("critics_score");
			data.AudienceRating = new Rating();
			data.AudienceRating.Description = ratingData.GetData<string>("audience_rating");
			data.AudienceRating.Score = ratingData.GetData<int>("audience_score");

			Dictionary<string, object> postersData = movieData.GetData<Dictionary<string, object>>("posters");
			data.Images = new List<Image>();
			data.Images.Add(new Image() { MappedType = ImageType.Poster, ThumbnailURL = postersData.GetData<string>("original") });

			Dictionary<string, object> linksData = movieData.GetData<Dictionary<string, object>>("links");
			data.Url = linksData.GetData<string>("alternate");

			ArrayList genres = movieData.GetData<ArrayList>("genres");
			if (genres != null) {
				data.Genres.AddRange(genres.ToArray().Cast<string>());
			}
			
			return data;
		}
	}
}