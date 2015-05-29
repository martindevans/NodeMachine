using System.Linq;
using JetBrains.Annotations;
using NodeMachine.Model;
using RestSharp;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace NodeMachine.Connection
{
    public class Topology
        : ITopology, INotifyPropertyChanged
    {
        private readonly GameConnection _connection;

        private bool _hasLiveConnection;
        public bool HasLiveConnection
        {
            get
            {
                return _hasLiveConnection;
            }
            set
            {
                _hasLiveConnection = value;
                OnPropertyChanged();
            }
        }

        private ProceduralNode _root;
        public ProceduralNode Root
        {
            get
            {
                return _root;
            }
            private set
            {
                _root = value;
                OnPropertyChanged();

                if (HasLiveConnection)
                {
                    throw new NotImplementedException("Close existing live connection");
                }

                var kvp = _root.Metadata.SingleOrDefault(a => a.Key == "live_connection_address");
                if (kvp.Key == null)
                    HasLiveConnection = false;
                else
                {
                    throw new NotImplementedException("Open Live Connection");
                }
            }
        }

        public Topology(GameConnection connection)
        {
            _connection = connection;
        }

        public async Task<ProceduralNode> Refresh()
        {
            var request = new RestRequest("/scene/services/worldgeometryservice/topology", Method.GET);
            var response = await _connection.Request<ProceduralNode>(request);

            if (response.ResponseStatus == ResponseStatus.Completed)
                Root = response.Data;
            return Root;
        }

        public async Task Clear()
        {
            var request = new RestRequest("/scene/services/worldgeometryservice/topology", Method.DELETE);
            var response = await _connection.Request(request);

            if (response.ResponseStatus == ResponseStatus.Completed)
                Root = null;
        }

        public async Task<bool> SetRoot(Guid scriptId)
        {
            var request = new RestRequest("/scene/services/worldgeometryservice/topology", Method.PUT);
            request.AddParameter("text", scriptId.ToString(), ParameterType.RequestBody);

            var response = await _connection.Request(request);

            if (response.ResponseStatus == ResponseStatus.Completed)
            {
                Root = null;
                await Refresh();
                return true;
            }

            return false;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
