using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shared;
using Sockets.Plugin;

namespace SmartHome.HoloLens
{
	public class ClientConnection
	{
		TcpSocketClient socketClient;
		INetworkSerializer networkSerializer;
		Func<BaseDto> dtoRealTimeCallback;
		readonly Dictionary<string, BaseDto> objectsToSend = new Dictionary<string, BaseDto>();

		/// <summary>
		/// Fired when conneciton is closed.
		/// </summary>
		public event Action Disconnected;

		/// <summary>
		/// Is connected to uwp/android/ios app
		/// </summary>
		public bool Connected { get; private set; }

		/// <summary>
		/// Connect to a client
		/// </summary>
		public async Task<bool> ConnectAsync(string ip, int port)
		{
			try
			{
				socketClient = new TcpSocketClient();
				networkSerializer = new ProtobufNetworkSerializer();
				await socketClient.ConnectAsync(ip, port);
				Connected = true;
			}
			catch (Exception)
			{
				return false;
			}
			StartSendingData();
			return true;
		}

		public void SendObject(string id, BaseDto dto)
		{
			lock (objectsToSend)
				objectsToSend[id] = dto;
		}

		public void SendObject(BaseDto dto)
		{
			SendObject(Guid.NewGuid().ToString(), dto);
		}

		public void RegisterForRealtimeUpdate(Func<BaseDto> callback)
		{
			dtoRealTimeCallback = callback;
		}

		async void StartSendingData()
		{
			try
			{
				await Task.Run(async () =>
				{
					while (true)
					{
						List<BaseDto> surfacesToSend;

						lock (objectsToSend)
						{
							surfacesToSend = objectsToSend.Values.ToList();
							objectsToSend.Clear();
						}

						if (surfacesToSend.Count > 0)
						{
							foreach (var surface in surfacesToSend)
							{
								networkSerializer.WriteToStream(socketClient.WriteStream, dtoRealTimeCallback());
								networkSerializer.WriteToStream(socketClient.WriteStream, surface);
							}
						}
						else
						{
							networkSerializer.WriteToStream(socketClient.WriteStream, dtoRealTimeCallback());
						}
						await Task.Delay(20);
					}
				});
			}
			catch (Exception exc)
			{
				Connected = false;
				Disconnected?.Invoke();
			}
		}
	}
}
