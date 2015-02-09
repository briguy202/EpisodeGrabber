using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EpisodeGrabber.Library.Entities;
using EpisodeGrabber.Library.Services;
using Microsoft.QualityTools.Testing.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EpisodeGrabber.Library.Services.Tests {
	[TestClass()]
	public class ShowServiceTests {
		private ShowService _service;
		private Show _show;

		[TestInitialize()]
		public void Setup() {
			_service = new ShowService();
			_show = new Show() {
				Path = "C:\\MyShow",
				Seasons = new List<Season>() {
					new Season() {
						ID = 123,
						Number = 1,
						Episodes = new List<Episode>() {
							new Episode() {
								ID = 456,
								EpisodeNumber = 1,
								Name = "My Test Episode"
							}
						}
					}
				}
			};
		}

		[TestMethod()]
		public void IsShowFileTest() {
			bool result = false;
			
			result = ShowService.IsShowFile("Some Show Season 1 Episode 3");
			Assert.IsFalse(result);

			result = ShowService.IsShowFile("S3E1");
			Assert.IsTrue(result);

			result = ShowService.IsShowFile("S03E1");
			Assert.IsTrue(result);

			result = ShowService.IsShowFile("S03E01");
			Assert.IsTrue(result);

			result = ShowService.IsShowFile("S03E001");
			Assert.IsTrue(result);
		}

		//[TestMethod()]
		//public void InitializeTest() {
		//	using (ShimsContext.Create()) {
		//		System.IO.Fakes.ShimDirectoryInfo.AllInstances.GetDirectories = (path) => {
		//			if (1 == 1) { }
		//			return null;
		//		};

		//		_service.Initialize(_show);

		//	}
		//}

		//[TestMethod()]
		//public void CreateFilesTest() {
		//	Assert.Fail();
		//}

		//[TestMethod()]
		//public void GetBannerImageTest() {
		//	Assert.Fail();
		//}

		//[TestMethod()]
		//public void FetchMetadataByNameTest() {
		//	Assert.Fail();
		//}

		//[TestMethod()]
		//public void FetchMetadataByIDTest() {
		//	Assert.Fail();
		//}
	}
}
