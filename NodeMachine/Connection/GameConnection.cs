using NodeMachine.Annotations;
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

        public ITopology Topology { get; private set; }

        public IScreenCollection Screens { get; private set; }

        public GameConnection()
        {
            Screens = new ScreenCollection(this);
            Topology = new Topology(this);
        }

        public async Task Connect()
        {
            while (!await IsConnected())
            {
            }
        }

        public async Task<bool> IsConnected()
        {
            var request = new RestRequest("/scene", Method.GET);
            request.Timeout = 500;
            var response = await _client.ExecuteGetTaskAsync(request, _cancellation.Token);
            return response.StatusCode == HttpStatusCode.OK;
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
