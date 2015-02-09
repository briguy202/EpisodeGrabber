using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;
using CommonLibrary.Framework;
using CommonLibrary.Framework.Tracing;
using CommonLibrary.Framework.Validation;
using EpisodeGrabber.Library.Entities;

namespace EpisodeGrabber.Library.Services {
	public abstract class EntityServiceBase {

		#region Constants
		protected const string SaveFileName = "grabber.xml";
		protected const string OldSaveFileName = "show.xml";
		#endregion

		public static IEntityService GetService(EntityBase entity) {
			ValidationUtility.ThrowIfNullOrEmpty(entity, "entity");

			IEntityService service = null;
			if (entity is Show) service = new ShowService();
			else if (entity is Episode) service = new EpisodeService();
			else if (entity is Season) service = new SeasonService();
			else if (entity is Movie) service = new MovieService();
			return service;
		}

		public static void DeleteCache(string directoryPath) {
			if (Directory.Exists(directoryPath)) {
				var files = Directory.GetFiles(directoryPath, SaveFileName, SearchOption.AllDirectories);
				files.ForEach(f => File.Delete(f));
			}
		}

		public static List<EntityBase> ScanDirectory(string directoryPath, IEnumerable<string> mediaTypes) {
			List<EntityBase> entities = new List<EntityBase>();

			DirectoryInfo directory = new DirectoryInfo(directoryPath);
			if (directory.Exists) {
				// Rename the old-named files
				FileInfo[] oldFiles = directory.GetFiles(EntityServiceBase.OldSaveFileName);
				if (oldFiles.Count() == 1) {
					string oldName = oldFiles[0].FullName;
					string newName = System.IO.Path.Combine(directory.FullName, EntityServiceBase.SaveFileName);

					if (!File.Exists(newName)) {
						TraceManager.Trace(string.Format("Old file name {0} found, renaming to {1}.", oldName, newName));
						oldFiles[0].MoveTo(newName);
					} else {
						TraceManager.Trace(string.Format("Old file {0} exists and new file {1} is already there - deleting the old file.", oldName, newName));
						oldFiles[0].Delete();
					}
				}

				FileInfo[] grabberFiles = directory.GetFiles(EntityServiceBase.SaveFileName);
				if (grabberFiles.Count() == 1) {
					TraceManager.Trace(string.Format("{0} file found at {1}.", EntityServiceBase.SaveFileName, directory.FullName), (int)TraceVerbosity.Verbose);

					EntityBase entity = null;
					string filepath = grabberFiles[0].FullName;
					Type type = null;
					XmlDocument doc = new XmlDocument();

					try {
						doc.Load(grabberFiles[0].FullName);
					} catch (Exception) {
						TraceManager.Trace(string.Format("Unable to load file {0}.", grabberFiles[0].FullName), TraceTypes.Error);
					}

					if (doc.DocumentElement != null) {
						string typeName = doc.DocumentElement.Name;
						switch (typeName.ToLowerInvariant()) {
							case "show": type = typeof(Show); break;
							case "movie": type = typeof(Movie); break;
						}

						if (type != null) {
							try {
								XmlSerializer serializer = new XmlSerializer(type);
								using (FileStream fs = new FileStream(grabberFiles[0].FullName, FileMode.Open)) {
									entity = (EntityBase)serializer.Deserialize(fs);
								}
							} catch (Exception) {
								// Pass silently
							}
						}
					}

					if (entity != null) {
						entity.Path = directory.FullName;
						entities.Add(entity);
					}
				}

				// If there was no configuration file, try to figure out what kind of item this is.
				if (entities.IsNullOrEmpty()) {
					DirectoryInfo seasonDirectory = directory.GetDirectories("Season *").FirstOrDefault();
					if (seasonDirectory != null) {
						string showName = seasonDirectory.Parent.Name;
						entities.Add(new Show() { Path = directory.FullName, Name = showName });
					}
				}

				if (entities.IsNullOrEmpty()) {
					FileInfo[] videoFiles = directory.GetFilesPattern("*.avi|*.mp4|*.divx|*.m4v");
					if (videoFiles.Count() > 0) {
						bool isAllShows = true;
						bool isNoneShows = true;
						foreach (FileInfo videoFile in videoFiles) {
							if (!ShowService.IsShowFile(videoFile.FullName)) {
								isAllShows = false;
							} else {
								isNoneShows = false;
							}
						}

						if (isAllShows) {
							// If all the video files in the directory have the season/episode type file naming convention, then
							// the directory that houses these files must be the show directory.
							TraceManager.Trace(string.Format("Show found at {0}", directory.FullName), TraceVerbosity.Verbose);
							entities.Add(new Show() { Path = directory.FullName, Name = directory.Name });
						} else if (isNoneShows) {
							// If none of the video files in the directory have the season/episode type file naming convention, then
							// the directory that houses these files is assumed to be a movie directory.
							TraceManager.Trace(string.Format("Movie found at {0}", directory.FullName), TraceVerbosity.Verbose);
							Match match = Regex.Match(directory.Name, @"(.*?)\((\d{4})\)(.*?)");
							if (match.Success) {
								int year = int.Parse(match.Groups[2].Value);
								string name = string.Concat(match.Groups[1].Value, " ", match.Groups[3].Value).Trim();
								entities.Add(new Movie() { Path = directory.FullName, Name = name, Created = new DateTime(year, 1, 1) });
							} else {
								entities.Add(new Movie() { Path = directory.FullName, Name = directory.Name });
							}
						}
					}
				}

				if (entities.IsNullOrEmpty()) {
					if (directory.GetDirectories().Count() > 0) {
						TraceManager.Trace(string.Format("Nothing found at {0}, looking deeper ...", directory.FullName), TraceVerbosity.Verbose);
						foreach (DirectoryInfo subDirectory in directory.GetDirectories()) {
							entities.AddRange(EntityServiceBase.ScanDirectory(subDirectory.FullName, mediaTypes));
						}
					} else {
						TraceManager.Trace(string.Format("Nothing found at {0}", directory.FullName), TraceVerbosity.Verbose);
					}
				}

				// Initialize the shows ... hacking here ...
				entities.Where(e => e is Show).ForEach(e => new ShowService().Initialize(e, mediaTypes));
			}

			return entities;
		}

		public void Save(EntityBase entity) {
			string filePath = System.IO.Path.Combine(entity.Path, EntityServiceBase.SaveFileName);
			XmlSerializer serializer = new XmlSerializer(entity.GetType());
			TraceManager.Trace(string.Format("Writing file '{0}'", filePath), TraceVerbosity.Verbose, TraceTypes.Information);
			using (XmlWriter writer = XmlWriter.Create(filePath, new XmlWriterSettings() { Indent = true })) {
				serializer.Serialize(writer, entity);
			}
		}

		public void Delete(EntityBase entity) {
			string filePath = System.IO.Path.Combine(entity.Path, EntityServiceBase.SaveFileName);
			if (File.Exists(filePath)) {
				File.Delete(filePath);
			}
		}

		public virtual Image GetBannerImage(EntityBase entity) {
			return entity.Images.FirstOrDefault();
		}

		protected string ParseName(string name) {
			// Remove the year value in parenthesis
			string value = Regex.Replace(name, @"\(\d{4}\)", string.Empty);
			value = value.Replace('_', ' ');
			value.Trim();
			return value;
		}
	}
}
