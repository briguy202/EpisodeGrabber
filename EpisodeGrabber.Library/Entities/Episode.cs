using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;
using CommonLibrary.Framework;
using CommonLibrary.Framework.Tracing;
using EpisodeGrabber.Library.Services;

namespace EpisodeGrabber.Library.Entities {
	public class Episode : EntityBase {

		//public override string UniqueID { get { return string.Concat(this.Parent.UniqueID, "_", this.ID.ToString()); } }
		[XmlIgnore]
		public Season Parent { get; set; }
		public int SeasonNumber { get; set; }
		public int EpisodeNumber { get; set; }
		public int DVD_Chapter { get; set; }
		public int DVD_DiscID { get; set; }
		public double DVD_EpisodeNumber { get; set; }
		public int DVD_SeasonNumber { get; set; }
		public string Director { get; set; }
		public List<string> GuestStars { get; set; }
		public string IMDB_ID { get; set; }
		public string Language { get; set; }
		public int ProductionCode { get; set; }
		public double Rating { get; set; }
		public string Writer { get; set; }
		public int SeasonID { get; set; }
		public int SeriesID { get; set; }
		
		public Episode() {
			this.GuestStars = new List<string>();
			this.Images = new List<Image>();
		}

		public override string ToString() {
			return new EpisodeService().GetIdentifierAndName(this, true, false);
		}
		
	}
}
