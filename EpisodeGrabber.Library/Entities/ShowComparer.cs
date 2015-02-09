using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EpisodeGrabber.Library.Entities {
	public class ShowComparer : IEqualityComparer<Show> {

		bool IEqualityComparer<Show>.Equals(Show x, Show y) {
			return x.ID == y.ID;
		}

		int IEqualityComparer<Show>.GetHashCode(Show obj) {
			return obj.ID;
		}
	}
}
