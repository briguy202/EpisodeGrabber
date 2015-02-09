using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;
using CommonLibrary.Framework;
using CommonLibrary.Framework.Tracing;
using EpisodeGrabber.Library.DAO;

namespace EpisodeGrabber.Library.Entities {
	public class Show : EntityBase {

		//[XmlIgnore]
		//public override DateTime Created { get { return (this.Seasons.FirstOrDefault() != null) ? this.Seasons.First().Created : DateTime.MinValue; } }
		public List<Season> Seasons { get; set; }
		public string AirDay { get; set; }
		public string AirTime { get; set; }
		public string MainBannerImageURL { get; set; }
		public string IMDB_ID { get; set; }
		public string Network { get; set; }
		public string Rating { get; set; }
		public string ContentRating { get; set; }
		public string Status { get; set; }
		public int Runtime { get; set; }
		
		public Show() : base() {
			this.Seasons = new List<Season>();
		}

		public override string ToString() {
			return string.Format("{0} - {1}", this.Name, this.ID.ToString());
		}
	}
}