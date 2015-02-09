using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using CommonLibrary.Framework.Tracing;

namespace EpisodeGrabber.Library.Entities {
	public class Season : EntityBase {

		[XmlIgnore]
		public Show Parent { get; set; }
		public List<Episode> Episodes { get; set; }

		[XmlAttribute]
		public int Number { get; set; }
		//[XmlIgnore]
		//public override string UniqueID { get { return string.Concat(this.Parent.UniqueID.ToString(), "_", this.Number.ToString()); } }
		[XmlIgnore]
		public bool DownloadedAllEpisodes { get; set; }
		[XmlIgnore]
		public bool DownloadedAllExistingEpisodes { get; set; }
		[XmlIgnore]
		public bool DownloadedSomeEpisodes { get; set; }
		[XmlIgnore]
		public bool DownloadedNoEpisodes { get; set; }
		//[XmlIgnore]
		//public override string DescriptiveName { get { return string.Format("{0} season {1}", this.Parent.Name, this.Number.ToString()); } }
		//[XmlIgnore]
		//public override DateTime Created { get { return (this.Episodes.FirstOrDefault() != null) ? this.Episodes.First().Created : DateTime.MinValue; } }
		
		public Season() {
			this.Episodes = new List<Episode>();
			this.Images = new List<Image>();
		}

		public override string ToString() {
			return string.Format("Season {0}", this.Number.ToString());
		}
		
	}
}
