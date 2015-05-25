using System;
using System.Threading.Tasks;
using NodeMachine.Model;
using System.Linq;
using WebSocketSharp;

namespace NodeMachine.Connection
{
    public class RemoteSubdivisionContext
    {
        private readonly ProceduralNode _root;
        private WebSocket _socket;

        private WebSocket Socket
        {
            get
            {
                return _socket;
            }
            set
            {
                if (_socket != null)
                    throw new InvalidOperationException("Cannot set remote context socket twice");
                _socket = value;
                _socket.OnMessage += OnWebsocketMessage;
                _socket.OnClose += OnWebsocketClose;
            }
        }

        private RemoteSubdivisionContext(ProceduralNode root)
        {
            _root = root;

        }

        private void OnWebsocketMessage(object sender, MessageEventArgs messageEventArgs)
        {
        }

        private void OnWebsocketClose(object sender, CloseEventArgs e)
        {
        }

        /// <summary>
        /// Assume this node is a WSNode and try to connect
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public static async Task<RemoteSubdivisionContext> Connect(ProceduralNode node)
        {
            var ws = node.Metadata.SingleOrDefault(a => a.Key == "websocket_location");
            if (ws.Key == null)
                return null;

            RemoteSubdivisionContext context = new RemoteSubdivisionContext(node);

            var socket = new WebSocket((string)ws.Value);
            context.Socket = socket;
            await socket.ConnectAsync();

            if (!socket.IsAlive)
            {
                socket.Dispose();
                return null;
            }

            return context;
        }
    }
}
