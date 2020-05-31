namespace Rhapsody.Objects {
	public struct EndpointOptions {
		public string Host { get; set; }
		public ushort Port { get; set; }
		public bool FallbackRandom { get; set; }
	}
}