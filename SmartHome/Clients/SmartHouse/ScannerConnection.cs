using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Shared;
using Sockets.Plugin;
using Sockets.Plugin.Abstractions;

namespace SmartHome
{
	public class ScannerConnection
	{
		const int Port = 5206;
		TcpSocketListener listener;
		ITcpSocketClient client;
		Dictionary<Type, Action<object>> callbacks = new Dictionary<Type, Action<object>>();

		public INetworkSerializer Serializer { get; private set; }

		public async Task WaitForCompanion()
		{
			Serializer = new ProtobufNetworkSerializer();
			var tcs = new TaskCompletionSource<bool>();
			listener = new TcpSocketListener();
			listener.ConnectionReceived += (s, e) =>
			{
				Serializer.ObjectDeserialized += SimpleNetworkSerializerObjectDeserialized;
				tcs.TrySetResult(true);
				client = e.SocketClient;
				try
				{
					Serializer.ReadFromStream(client.ReadStream);
				}
				catch (Exception exc)
				{
					//show error?
				}
			};
			await listener.StartListeningAsync(Port);
			await tcs.Task;
		}

		public void Send(BaseDto dto)
		{
			try
			{
				Serializer.WriteToStream(client.WriteStream, dto);
			}
			catch (Exception exc)
			{
				//show error?
			}
		}

		public void RegisterFor<T>(Action<T> callback)
		{
			lock (callbacks)
			{
				callbacks[typeof(T)] = obj => callback((T) obj);
			}
		}

		public static async Task<string> GetLocalIp()
		{
			var interfaces = await CommsInterface.GetAllInterfacesAsync();
			//TODO: check if any
			return interfaces.Last(i => !i.IsLoopback && i.IsUsable).IpAddress + ":" + Port;
		}

		void SimpleNetworkSerializerObjectDeserialized(object obj)
		{
			if (obj == null)
				return;

			lock (callbacks)
			{
				Action<object> callback;
				if (callbacks.TryGetValue(obj.GetType(), out callback))
				{
					callback(obj);
				}
			}
		}
	}
}
