using System;
namespace EpisodeGrabber.Library.Entities {
	public enum ImageScopeType {
		Show,
		Season,
		Episode,
		NotSet
	}

	public enum ImageType {
		FanArt,
		Poster,
		Backdrop,
		SeasonWide,
		Graphical,
		Text,
		Unknown,
		NotSet
	}

	public class Image {
		public string ID { get; set; }
		public string URL { get; set; }
		public string ThumbnailURL { get; set; }
		public string LocalFileName { get; set; }
		public int Height { get; set; }
		public int Width { get; set; }
		public int SeasonNumber { get; set; }
		public ImageScopeType ScopeType { get; set; }
		public ImageType MappedType { get; set; }
		public string OriginalType { get; set; }
		public EntityBase ParentEntity { get; set; }

		public Image() {
			this.ScopeType = ImageScopeType.NotSet;
			this.MappedType = ImageType.NotSet;
		}

		public static ImageScopeType MapImageScopeTypeValue(string value) {
			switch (value) {
				case "season": return ImageScopeType.Season;
				default: return ImageScopeType.Show;
			}
		}

		public static ImageType MapImageTypeValue(string value1, string value2) {
			switch (value1.ToLowerInvariant()) {
				case "backdrop": return ImageType.Backdrop;
				case "poster": return ImageType.Poster;
				case "fanart": return ImageType.FanArt;
			}

			switch (value2) {
				case "seasonwide": return ImageType.SeasonWide;
				case "graphical": return ImageType.Graphical;
				case "text": return ImageType.Text;
			}

			return ImageType.Unknown;
		}

		public override string ToString() {
			return string.Format("{0} - {1}", this.MappedType.ToString(), this.ID.ToString());
		}
	}
}