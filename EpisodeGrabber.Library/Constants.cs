namespace EpisodeGrabber.Library {
	public static class TraceVerbosity {
		public const int Minimal = 1;
		public const int Default = 50;
		public const int Verbose = 100;
	}

	public static class TraceTypes {
		public const string Error = "Error";
		public const string Exception = "Exception";
		public const string Default = "Default";
		public const string WebRequest = "WebRequest";
		public const string OperationStarted = "OperationStarted";
		public const string OperationCompleted = "OperationCompleted";
		public const string Information = "Information";
	}
}