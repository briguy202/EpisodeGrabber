using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EpisodeGrabber.Library.Entities {
	public class Person {
		public int ID { get; set; }
		public string Name { get; set; }
		public string Character { get; set; }
		public string Job { get; set; }

		public override string ToString() {
			return string.Format("{0} ({1})", this.Name, this.ID.ToString());
		}
	}
}