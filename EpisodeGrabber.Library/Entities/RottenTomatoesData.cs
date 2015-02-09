using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EpisodeGrabber.Library.Entities {
	public class RottenTomatoesData {
		public int ID { get; set; }
		public string Url { get; set; }
		public string IMDBId { get; set; }
		public string Name { get; set; }
		public string MPAARating { get; set; }
		public string Studio { get; set; }
		public int Runtime { get; set; }
		public string Description { get; set; }
		public DateTime Created { get; set; }
		public List<string> Genres { get; set; }
		public List<Image> Images { get; set; }
		public Rating CriticsRating { get; set; }
		public Rating AudienceRating { get; set; }

		public RottenTomatoesData() {
			this.Genres = new List<string>();
			this.Images = new List<Image>();
		}
	}
}
