using System;
using System.IO;
using System.Threading;

namespace Shared
{
	public interface INetworkSerializer
	{
		event Action<BaseDto> ObjectDeserialized;
		void ReadStream(Stream stream, CancellationToken token = default(CancellationToken));
		void Write(Stream stream, BaseDto dto);
		byte[] Serialize(BaseDto dto);
	}
}