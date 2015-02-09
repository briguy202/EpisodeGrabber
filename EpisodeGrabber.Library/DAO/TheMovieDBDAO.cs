using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using CommonLibrary.Framework;
using CommonLibrary.Framework.Tracing;
using CommonLibrary.Framework.Validation;
using EpisodeGrabber.Library.Entities;

namespace EpisodeGrabber.Library.DAO {
	public class TheMovieDBDAO {
		private const string ApiKey = "c78b66672025d9b8a5fcb54a07c6ad84";

		public BusinessObject<List<EntityBase>> GetByName(string name) {
			ValidationUtility.ThrowIfNullOrEmpty(name, "name");

			// Example: http://api.themoviedb.org/3/search/movie?api_key=c78b66672025d9b8a5fcb54a07c6ad84&query=transformers
			var entities = new BusinessObject<List<EntityBase>>();
			var result = this.GetData("http://api.themoviedb.org/3/search/movie?api_key={0}&query={1}", name);

			if (result.Errors.IsNullOrEmpty()) {
				if (result.Data != null) {
					entities.Data = new List<EntityBase>();
					((IEnumerable<dynamic>)result.Data.results).ForEach(m => entities.Data.Add(this.MapToMovie(m)));
				}
			} else {
				entities.Errors.AddRange(result.Errors);
			}

			return entities;
		}

		public BusinessObject<EntityBase> GetByID(int id) {
			ValidationUtility.ThrowIfLessThan(id, 1, "id");

			// Example: http://api.themoviedb.org/3/movie/1858?api_key=c78b66672025d9b8a5fcb54a07c6ad84
			var entity = new BusinessObject<EntityBase>();
			var result = this.GetData("http://api.themoviedb.org/3/movie/{1}?api_key={0}", id.ToString());

			if (result.Errors.IsNullOrEmpty()) {
				if (result.Data != null) {
					entity.Data = this.MapToMovie(result.Data);
				}
			} else {
				entity.Errors.AddRange(result.Errors);
			}

			return entity;
		}

		private Movie MapToMovie(dynamic data) {
			ValidationUtility.ThrowIfNullOrEmpty(data, "movieData");

			Movie movie = new Movie();
			movie.ID = data.id;
			movie.Name = data.title;
			movie.AlternateName = data.original_title;
			movie.IsAdult = data.adult;
			movie.Created = DateTime.ParseExact(data.release_date.ToString(), "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
			movie.Popularity = data.popularity;
			movie.Budget = data.budget ?? 0;
			movie.Revenue = data.revenue ?? 0;
			movie.Runtime = data.runtime ?? 0;
			movie.Tagline = data.tagline;
			movie.HomepageUrl = data.homepage;
			movie.IMDBIdValue = (!string.IsNullOrEmpty((string)data.imdb_id)) ? int.Parse(((string)data.imdb_id).Replace("tt", string.Empty)) : 0;
			movie.Description = data.overview;
			movie.Images.Add(new Image() {
				URL = data.poster_path
			});

			if (data.genres != null) ((IEnumerable<dynamic>)data.genres).ForEach(x => movie.Genres.Add(x.name.ToString()));
			if (data.production_companies != null) ((IEnumerable<dynamic>)data.production_companies).ForEach(x => movie.Studios.Add(x.name.ToString()));

			return movie;
		}

		protected BusinessObject<dynamic> GetData(string url, string searchTerm) {
			ValidationUtility.ThrowIfNullOrEmpty(url, "url");
			ValidationUtility.ThrowIfNullOrEmpty(searchTerm, "searchTerm");

			var returnValue = new BusinessObject<dynamic>();
			
			string searchTermEncoded = WebUtility.HtmlEncode(searchTerm);
			string requestUrl = string.Format(url, "[Key]", searchTermEncoded);
			TraceManager.Trace(string.Format("Requesting url '{0}' ...", requestUrl), TraceTypes.OperationStarted);
			requestUrl = string.Format(url, TheMovieDBDAO.ApiKey, searchTermEncoded);

			try {
				var request = WebRequest.Create(requestUrl);
				using (var response = request.GetResponse()) {
					using (var stream = response.GetResponseStream()) {
						string json = new StreamReader(stream).ReadToEnd();
						dynamic obj = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
						returnValue.Data = obj;
					}
				}
			} catch (Exception ex) {
				returnValue.Errors.Add(ex.Message);
			}

			return returnValue;
		}
	}
}