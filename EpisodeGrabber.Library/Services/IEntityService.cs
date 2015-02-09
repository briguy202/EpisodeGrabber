using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EpisodeGrabber.Library.Entities;

namespace EpisodeGrabber.Library.Services {
	public interface IEntityService {
		void Initialize(EntityBase entity, IEnumerable<string> mediaTypes);
		void Save(EntityBase entity);
		void Delete(EntityBase entity);
		void CreateFiles(EntityBase entity, bool overwrite, UserConfiguration configuration);
	}
}