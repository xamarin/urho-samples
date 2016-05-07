namespace FaceDetection
{
	public struct FrameWithFaces
	{
		public byte[] FrameData { get; set; }
		public int FrameWidth { get; set; }
		public int FrameHeight { get; set; }
		public Rect[] Faces { get; set; }
	}
}