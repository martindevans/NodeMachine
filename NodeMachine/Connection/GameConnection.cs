using NodeMachine.Annotations;
using NodeMachine.Model;
using RestSharp;
using System.ComponentModel;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace NodeMachine.Connection
{
    public class GameConnection : IGameConnection, INotifyPropertyChanged
    {
        private readonly CancellationTokenSource _cancellation = new CancellationTokenSource();

        private readonly RestClient _client = new RestClient("http://localhost:41338");

        private ProceduralNode _topology;
        public ProceduralNode Topology
        {
            get
            {
                return _topology;
            }
            private set
            {
                _topology = value;
                OnPropertyChanged();
            }
        }

        public IScreenCollection Screens { get; private set; }

        public GameConnection()
        {
            Screens = new ScreenCollection(this);
        }

        public async Task Connect()
        {
            IRestResponse response;
            do
            {
                var request = new RestRequest("/scene", Method.GET);
                response = await _client.ExecuteGetTaskAsync(request, _cancellation.Token);
            } while (response.StatusCode != HttpStatusCode.OK);
        }

        public async Task<ProceduralNode> RefreshTopology()
        {
            var request = new RestRequest("/scene/services/worldgeometryservice/topology", Method.GET);
            var response = await Request<ProceduralNode>(request);

            if (response.ResponseStatus == ResponseStatus.Completed)
                Topology = response.Data;
            return Topology;
        }

        public async Task ClearTopology()
        {
            var request = new RestRequest("/scene/services/worldgeometryservice/topology", Method.DELETE);
            var response = await Request(request);

            if (response.ResponseStatus == ResponseStatus.Completed)
                Topology = null;
        }

        internal async Task<IRestResponse> Request(IRestRequest request)
        {
            return await _client.ExecuteTaskAsync(request, _cancellation.Token);
        }

        internal async Task<IRestResponse<T>> Request<T>(IRestRequest request)
        {
            return await _client.ExecuteTaskAsync<T>(request, _cancellation.Token);
        }

        public void Disconnect()
        {
            _cancellation.Cancel();
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
