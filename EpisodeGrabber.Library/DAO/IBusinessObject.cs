using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EpisodeGrabber.Library.DAO {
	public interface IBusinessObject<T> {
		List<string> Errors { get; set; }
		T Data { get; }
	}
}
