using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EpisodeGrabber.Library.DAO {
	public class BusinessObject<T> {
		public List<string> Errors { get; set; }
		public T Data { get; set; }

		public BusinessObject() {
			this.Errors = new List<string>();
		}
	}
}