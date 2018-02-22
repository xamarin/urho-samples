using System;
using System.IO;
using System.Threading;

namespace Shared
{
	public interface INetworkSerializer
	{
		event Action<BaseDto> ObjectDeserialized;
		void ReadFromStream(Stream stream, CancellationToken token = default(CancellationToken));
		void WriteToStream(Stream stream, BaseDto dto);
		byte[] Serialize(BaseDto dto);
		T Deserialize<T>(byte[] currentProperty) where T : BaseDto;
	}
}