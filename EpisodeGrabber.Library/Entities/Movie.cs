using System.Collections.Generic;

namespace EpisodeGrabber.Library.Entities {
	public class Movie : EntityBase {

		public int IMDBRating { get; set; }
		public int IMDBIdValue { get; set; }
		public string MPAARating { get; set; }
		public int Runtime { get; set; }
		public List<string> Studios { get; set; }
		public List<Person> Cast { get; set; }
		public double Popularity { get; set; }
		public string Tagline { get; set; }
		public int Budget { get; set; }
		public int Revenue { get; set; }
		public string HomepageUrl { get; set; }
		public string Trailer { get; set; }

		public Movie() {
			this.Studios = new List<string>();
			this.Cast = new List<Person>();
		}
	}
}