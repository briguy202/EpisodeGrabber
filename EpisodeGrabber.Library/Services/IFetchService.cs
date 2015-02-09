using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EpisodeGrabber.Library.DAO;
using EpisodeGrabber.Library.Entities;

namespace EpisodeGrabber.Library.Services {
	public interface IFetchService {
		BusinessObject<List<EntityBase>> FetchMetadataByName(string name);
		BusinessObject<EntityBase> FetchMetadataByID(int id);
	}
}