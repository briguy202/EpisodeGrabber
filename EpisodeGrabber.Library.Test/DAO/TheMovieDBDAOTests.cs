using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonLibrary.Framework;
using EpisodeGrabber.Library.DAO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EpisodeGrabber.Library.DAO.Tests {
	[TestClass()]
	public class TheMovieDBDAOTests {
		private TheMovieDBDAO _dao;
		private string _movieName;
		private int _movieID;

		[TestInitialize()]
		public void Setup() {
			_dao = new TheMovieDBDAO();
			_movieName = "transformers";
			_movieID = 1858;
		}

		[TestMethod()]
		public void GetByNameTest() {
			var result = _dao.GetByName(_movieName);

			Assert.IsTrue(result.Errors.IsNullOrEmpty());
			Assert.IsFalse(result.Data.IsNullOrEmpty(), "Retrieval of '{0}' movie failed.", _movieName);
		}

		[TestMethod()]
		[ExpectedException(typeof(ArgumentNullException))]
		public void GetByNameTest_EmptyNameTest() {
			var result = _dao.GetByName(string.Empty);
		}

		[TestMethod()]
		[ExpectedException(typeof(ArgumentNullException))]
		public void GetByNameTest_NullNameTest() {
			var result = _dao.GetByName(null);
		}

		[TestMethod()]
		[ExpectedException(typeof(ArgumentNullException))]
		public void GetByNameTest_WhitespaceNameTest() {
			var result = _dao.GetByName(" ");
		}

		[TestMethod()]
		public void GetByIDTest() {
			var result = _dao.GetByID(1858);

			Assert.IsTrue(result.Errors.IsNullOrEmpty());
			Assert.IsNotNull(result.Data, "Retrieval of '{0}' by ID {1} failed.", _movieName, _movieID);
		}

		[TestMethod()]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void GetByIDTest_InvalidIDTest() {
			var result = _dao.GetByID(0);
		}
	}
}