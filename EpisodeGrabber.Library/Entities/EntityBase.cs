using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;
using CommonLibrary.Framework;
using CommonLibrary.Framework.Tracing;

namespace EpisodeGrabber.Library.Entities {
	public abstract class EntityBase {
		
		public bool IsAdult { get; set; }
		public DateTime Created { get; set; }
		public DateTime Updated { get; set; }
		public int ID { get; set; }
		[XmlAttribute]
		public string Name { get; set; }
		public string AlternateName { get; set; }
		public string Description { get; set; }
		public string UniqueID { get { return this.ID.ToString(); } }
		public List<Image> Images { get; set; }
		[XmlIgnore]
		public string Path { get; set; }
		public List<string> Genres { get; set; }
		public List<string> Actors { get; set; }
		
		public EntityBase() {
			this.ID = 0;
			this.Images = new List<Image>();
			this.Actors = new List<string>();
			this.Genres = new List<string>();
		}
		
	}
}