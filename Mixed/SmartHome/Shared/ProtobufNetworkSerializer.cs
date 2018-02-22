using System;
using System.IO;
using System.Threading;
using ProtoBuf;
using ProtoBuf.Meta;

namespace Shared
{
	/// <summary>
	/// Protobuf based impl
	/// uses WithLengthPrefix
	/// </summary>
	public class ProtobufNetworkSerializer : INetworkSerializer
	{
		public event Action<BaseDto> ObjectDeserialized;

		public void ReadFromStream(Stream stream, CancellationToken token)
		{
			RuntimeTypeModel.Default.MetadataTimeoutMilliseconds = 300000;
			while (!token.IsCancellationRequested)
			{
				var obj = Serializer.DeserializeWithLengthPrefix<BaseDto>(stream, PrefixStyle.Base128, fieldNumber: 1);
				ObjectDeserialized?.Invoke(obj);
			}
		}

		public void WriteToStream(Stream stream, BaseDto dto)
		{
			var bytes = Serialize(dto);
			stream.Write(bytes, 0, bytes.Length);
			stream.Flush();
		}

		public byte[] Serialize(BaseDto dto)
		{
			using (var ms = new MemoryStream())
			{
				Serializer.SerializeWithLengthPrefix(ms, dto, PrefixStyle.Base128, fieldNumber: 1);
				return ms.ToArray();
			}
		}

		public T Deserialize<T>(byte[] data) where T : BaseDto
		{
			using (var ms = new MemoryStream(data))
			{
				return Serializer.DeserializeWithLengthPrefix<T>(ms, PrefixStyle.Base128, fieldNumber: 1);
			}
		}
	}
}
