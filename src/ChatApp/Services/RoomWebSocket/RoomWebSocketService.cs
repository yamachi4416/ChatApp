using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebSockets;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace ChatApp.Services.RoomwebSocket
{
    public interface IRoomWebSocketService : IDisposable
    {
        Task SendAsync<E>(E value, string userId);

        Task SendAsync<E>(E value, IEnumerable<string> userIds);

        Task AddAsync(string userId, HttpContext context);

        ISet<string> RegistedSet(IEnumerable<string> userIds);

        void Remove(IEnumerable<string> userIds);

        void Remove(string userId);
    }

    public class RoomWebSocketService : IRoomWebSocketService
    {
        private readonly RoomWebSocketContainer
            _container = new RoomWebSocketContainer();

        private readonly JsonSerializerSettings jsonSettings =
            new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

        private IEnumerable<RoomWebSocket> Channels => _container.Channels.Values;

        public async Task AddAsync(string userId, HttpContext context)
        {
            using (var sock = await context.WebSockets.AcceptWebSocketAsync())
            {
                await _container.AddAsync(userId, sock);
            }
        }

        public void Remove(IEnumerable<string> userIds)
        {
            if (userIds.Count() == 0) return;
            var userIdSet = new HashSet<string>(userIds);
            _container.Remove(sock => userIdSet.Contains(sock.UserId));
        }

        public void Remove(string userId)
        {
            _container.Remove(s => s.UserId == userId);
        }

        public async Task SendAsync<E>(E value, IEnumerable<string> userIds)
        {
            if (userIds.Count() == 0) return;

            var userIdSet = new HashSet<string>(userIds);
            var jsonStr = JsonConvert.SerializeObject(value, jsonSettings);
            var buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(jsonStr));

            await Task.WhenAll(
                from s in Channels
                where userIdSet.Contains(s.UserId)
                select s.Send(buffer));
        }

        public async Task SendAsync<E>(E value, string userId)
        {
            await SendAsync(value, new HashSet<string> { userId });
        }

        public ISet<string> RegistedSet(IEnumerable<string> userIds)
        {
            var userIdSet = new HashSet<string>(userIds);

            return new HashSet<string>(
                from s in Channels
                where userIdSet.Contains(s.UserId)
                select s.UserId);
        }

        public void Dispose()
        {
            _container.Dispose();
        }
    }

    class RoomWebSocketContainer : IDisposable
    {
        public readonly ConcurrentDictionary<Guid, RoomWebSocket>
            Channels = new ConcurrentDictionary<Guid, RoomWebSocket>();

        public async Task AddAsync(string userId, WebSocket socket)
        {
            using (var sock = new RoomWebSocket(userId, socket, this))
            {
                await sock.Wait((r, b) => { });
            }
        }

        public void Remove(Func<RoomWebSocket, bool> filter)
        {
            foreach (var sock in Channels.Values.Where(filter))
            {
                sock.Dispose();
            }
        }

        public void AddSocket(RoomWebSocket sock)
        {
            Channels.TryAdd(sock.Id, sock);
        }

        public void RemoveSocket(RoomWebSocket sock)
        {
            Channels.TryRemove(sock.Id, out RoomWebSocket tmp);
        }

        public void Dispose()
        {
            Remove(_ => true);
        }
    }

    class RoomWebSocket : IDisposable
    {
        public Guid Id { get; } = Guid.NewGuid();

        public string UserId { get; }

        private readonly RoomWebSocketContainer _container;

        private readonly WebSocket _socket;

        public RoomWebSocket(string userId, WebSocket socket, RoomWebSocketContainer container)
        {
            UserId = userId;
            _socket = socket;
            _container = container;
            _container.AddSocket(this);
        }

        public async Task Send(ArraySegment<byte> buffer)
        {
            try
            {
                var token = CancellationToken.None;
                await _socket.SendAsync(
                    buffer: buffer,
                    messageType: WebSocketMessageType.Text,
                    endOfMessage: true,
                    cancellationToken: token);
            }
            catch
            {
                Dispose();
            }
        }

        public async Task Wait(Action<WebSocketReceiveResult, ArraySegment<byte>> callback)
        {
            var buffer = new byte[4096];

            WebSocketReceiveResult received = null;
            do
            {
                var arrayBuffer = new ArraySegment<byte>(buffer);
                received = await _socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                callback(received, arrayBuffer);
            } while (received.MessageType != WebSocketMessageType.Close);

            await _socket.CloseAsync(received.CloseStatus.Value, received.CloseStatusDescription, CancellationToken.None);
        }

        public void Dispose()
        {
            _socket?.Dispose();
            _container?.RemoveSocket(this);
        }
    }
}
