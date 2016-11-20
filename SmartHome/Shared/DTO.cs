using System.Collections.Generic;
using System.Runtime.InteropServices;
using ProtoBuf;

namespace Shared
{
	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
	[ProtoInclude(100, typeof(SurfaceDto))]
	[ProtoInclude(200, typeof(CurrentPositionDto))]
	[ProtoInclude(300, typeof(BulbAddedDto))]
	[ProtoInclude(400, typeof(SpaceDto))]
	[ProtoInclude(500, typeof(PointerPositionChangedDto))]
	public class BaseDto { }

	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
	public class SurfaceDto : BaseDto
	{
		public string Id { get; set; }
		public Vector3Dto BoundsCenter { get; set; }
		public Vector4Dto BoundsOrientation { get; set; }
		public Vector3Dto BoundsExtents { get; set; }
		public short[] IndexData { get; set; }
		public SpatialVertexDto[] VertexData { get; set; }
	}

	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
	public class CurrentPositionDto : BaseDto
	{
		public Vector3Dto Position { get; set; }
		public Vector3Dto Direction { get; set; }
	}

	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
	public class BulbAddedDto : BaseDto
	{
		public Vector3Dto Position { get; set; }
	}

	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
	public class SpaceDto : BaseDto
	{
		public Dictionary<string, SurfaceDto> Surfaces { get; set; } = new Dictionary<string, SurfaceDto>();
		public List<Vector3Dto> Bulbs { get; set; } = new List<Vector3Dto>();
	}

	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
	public class PointerPositionChangedDto : BaseDto
	{
		public Vector3Dto Position { get; set; }
	}

	// Additional:

	[StructLayout(LayoutKind.Sequential)]
	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
	public struct SpatialVertexDto
	{
		public uint Color { get; set; }
		public float NormalX { get; set; }
		public float NormalY { get; set; }
		public float NormalZ { get; set; }
		public float PositionX { get; set; }
		public float PositionY { get; set; }
		public float PositionZ { get; set; }
	}

	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
	public struct Vector3Dto
	{
		public Vector3Dto(float x, float y, float z)
		{
			X = x;
			Y = y;
			Z = z;
		}

		public float X { get; set; }
		public float Y { get; set; }
		public float Z { get; set; }
	}

	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
	public struct Vector4Dto
	{
		public Vector4Dto(float x, float y, float z, float w)
		{
			X = x;
			Y = y;
			Z = z;
			W = w;
		}

		public float W { get; set; }
		public float X { get; set; }
		public float Y { get; set; }
		public float Z { get; set; }
	}
}
