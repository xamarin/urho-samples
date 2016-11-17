using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Shared;
using Sockets.Plugin;

namespace SmartHome
{
	public static class ScannerConnection
	{
		const int Port = 5206;
		static INetworkSerializer networkSerializer;
		static TcpSocketListener listener;
		static Dictionary<Type, Action<object>> callbacks = new Dictionary<Type, Action<object>>();

		public static async Task WaitForCompanion()
		{
			networkSerializer = new ProtobufNetworkSerializer();
			var tcs = new TaskCompletionSource<bool>();
			listener = new TcpSocketListener();
			listener.ConnectionReceived += (s, e) =>
			{
				networkSerializer.ObjectDeserialized += SimpleNetworkSerializerObjectDeserialized;
				tcs.TrySetResult(true);
				var client = e.SocketClient;
				try
				{
					networkSerializer.ReadStream(client.ReadStream);
				}
				catch (Exception exc)
				{
					//show error?
				}
			};
			await listener.StartListeningAsync(Port);
			await tcs.Task;
		}

		public static void RegisterFor<T>(Action<T> callback)
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

		static void SimpleNetworkSerializerObjectDeserialized(object obj)
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
