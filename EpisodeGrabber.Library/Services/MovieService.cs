using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using CommonLibrary.Framework;
using CommonLibrary.Framework.Tracing;
using EpisodeGrabber.Library.DAO;
using EpisodeGrabber.Library.Entities;

namespace EpisodeGrabber.Library.Services {
	public class MovieService : EntityServiceBase, IFetchService, IEntityService {
		public void CreateFiles(EntityBase entity, bool overwrite, UserConfiguration configuration) {
			Movie movie = (Movie)entity;
			DirectoryInfo directory = new DirectoryInfo(movie.Path);
			if (directory.Exists) {
				// Rename
				string movieNameFormat = (!string.IsNullOrWhiteSpace(configuration.MovieNameFormat)) ? configuration.MovieNameFormat : "{NAME} ({YEAR})";
				string renamedFolderName = movieNameFormat.Replace("{YEAR}", movie.Created.Year.ToString()).Replace("{NAME}", movie.Name);
				renamedFolderName = renamedFolderName.ReplaceInvalidPathCharacters(string.Empty);
				if (System.IO.Path.GetFileName(movie.Path) != renamedFolderName) {
					string renamedFullPath = System.IO.Path.Combine(directory.Parent.FullName, renamedFolderName);
					TraceManager.Trace(string.Format("Renaming {0} to {1}.", movie.Path, renamedFullPath), TraceVerbosity.Minimal, TraceTypes.OperationCompleted);
					try {
						directory.MoveTo(renamedFullPath);
					} catch (Exception ex) {
						TraceManager.Trace(string.Format("Unable to move file {0} to {1}. Exception: {2}", directory.FullName, renamedFullPath, ex.Message), TraceTypes.Error);
					}
					movie.Path = renamedFullPath;
				}

				// Create the mymovies.xml file
				string filePath = string.Concat(directory.FullName, "\\mymovies.xml");
				if (File.Exists(filePath)) {
					if (overwrite) {
						File.Delete(filePath);
					} else {
						return;
					}
				}

				XmlDocument doc = new XmlDocument();
				XmlNode rootNode = doc.AddNode("Title");
				rootNode.AddNode("LocalTitle", movie.Name);
				rootNode.AddNode("OriginalTitle", movie.Name);
				rootNode.AddNode("SortTitle", movie.Name);
				rootNode.AddNode("ForcedTitle");
				rootNode.AddNode("IMDBrating", movie.IMDBRating.ToString("F1"));
				rootNode.AddNode("ProductionYear", movie.Created.Year.ToString());
				rootNode.AddNode("MPAARating", movie.MPAARating.ToUpperInvariant());
				rootNode.AddNode("Added");
				rootNode.AddNode("IMDbId", movie.IMDBIdValue.ToString());
				rootNode.AddNode("RunningTime", movie.Runtime.ToString());
				rootNode.AddNode("TMDbId", movie.ID.ToString());
				rootNode.AddNode("CDUniverseId");

				XmlNode studios = rootNode.AddNode("Studios");
				movie.Studios.ForEach((studio) => studios.AddNode("Studio", studio));
				XmlNode persons = rootNode.AddNode("Persons");
				foreach (Person person in movie.Cast) {
					XmlNode personNode = persons.AddNode("Person");
					personNode.AddNode("Name", person.Name);
					personNode.AddNode("Type", person.Job);
					personNode.AddNode("Role", person.Character);
				}
				XmlNode genres = rootNode.AddNode("Genres");
				movie.Genres.ForEach((genre) => genres.AddNode("Genre", genre));

				rootNode.AddNode("Description", movie.Description);
				rootNode.AddNode("Budget", movie.Budget.ToString());
				rootNode.AddNode("Revenue", movie.Revenue.ToString());

				// Save the file
				doc.Save(filePath);

				// Fetch the poster
				string posterFile = System.IO.Path.Combine(movie.Path, "folder.jpg");
				if (!File.Exists(posterFile)) {
					Image poster = movie.Images.Where((i) => i.MappedType == ImageType.Poster).OrderByDescending((i) => i.Height).ToList().FirstOrDefault();
					this.SetPosterImage(movie, poster);
				}
			} else {
				throw new DirectoryNotFoundException(string.Format("Directory {0} was not found.", directory.FullName));
			}
		}

		public void SetPosterImage(Movie movie, Image posterImage) {
			// Fetch the poster
			if (posterImage != null) {
				string posterFile = System.IO.Path.Combine(movie.Path, "folder.jpg");
				TraceManager.Trace("Downloading poster image ...", TraceTypes.OperationStarted);
				EpisodeService.DownloadImage(posterImage.URL, posterFile);
			} else {
				TraceManager.Trace(string.Format("Unable to find a poster file for {0}.", movie.Name), TraceTypes.Error);
			}
		}

		public BusinessObject<List<EntityBase>> FetchMetadataByName(string name) {
			var data = new TheMovieDBDAO().GetByName(name);
			return data;
		}

		public BusinessObject<EntityBase> FetchMetadataByID(int id) {
			var data = new TheMovieDBDAO().GetByID(id);
			return data;
		}

		public void Initialize(EntityBase entity, IEnumerable<string> mediaTypes) {
			// do nothing
		}
	}
}
