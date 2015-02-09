using System.Collections.Generic;
using System.Xml;
using EpisodeGrabber.Library.DAO;
using EpisodeGrabber.Library.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace EpisodeGrabber.Library.DAO.Tests {
	/// <summary>
	///This is a test class for TheTVDBDAOTest and is intended
	///to contain all TheTVDBDAOTest Unit Tests
	///</summary>
	[TestClass()]
	public class TheTVDBDAOTest {
		TheTVDBDAO _dao;
		private int _showID;
		private string _showName;

		[TestInitialize()]
		public void Setup() {
			_dao = new TheTVDBDAO();
			_showID = 79349;
			_showName = "Dexter";
		}

		/// <summary>
		///A test for TheTVDBDAO Constructor
		///</summary>
		[TestMethod()]
		public void TheTVDBDAOConstructorTest() {
			TheTVDBDAO target = new TheTVDBDAO();
			Assert.IsNotNull(target);
		}

		/// <summary>
		///A test for GetImages
		///</summary>
		[TestMethod()]
		public void GetImagesTest() {
			List<Image> images = _dao.GetImages(_showID);
			Assert.IsTrue(images.Count > 0);
		}

		///// <summary>
		/////A test for GetServerTime
		/////</summary>
		//[TestMethod()]
		//[DeploymentItem("EpisodeGrabber.Library.dll")]
		//public void GetServerTimeTest() {
		//	TheTVDBDAO_Accessor target = new TheTVDBDAO_Accessor();
		//	string result = target.GetServerTime();
		//	DateTime dateResult = DateTime.MinValue;
		//	Assert.IsTrue(!string.IsNullOrWhiteSpace(result));
		//}

		/// <summary>
		///A test for GetShowByID
		///</summary>
		[TestMethod()]
		public void GetShowByIDTest() {
			var show = _dao.GetShowByID(_showID);
			Assert.IsNotNull(show);
		}

		/// <summary>
		///A test for GetShowDataURL
		///</summary>
		[TestMethod()]
		public void GetShowDataURLTest() {
			string url = _dao.GetShowDataURL(_showID);
			Assert.IsTrue(!string.IsNullOrWhiteSpace(url));
		}

		/// <summary>
		///A test for GetShowsByName
		///</summary>
		[TestMethod()]
		public void GetShowsByNameTest() {
			var shows = _dao.GetShowsByName(_showName);
			Assert.IsTrue(shows.Data.Count > 0);

			var sunnyShows = _dao.GetShowsByName("Sunny");
			Assert.IsTrue(sunnyShows.Data.Count > 0);
		}
	}
}
