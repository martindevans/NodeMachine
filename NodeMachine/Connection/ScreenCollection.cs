using NodeMachine.Model;
using RestSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NodeMachine.Connection
{
    public class ScreenCollection
        : IScreenCollection
    {
        private readonly GameConnection _connection;

        public ScreenCollection(GameConnection connection)
        {
            _connection = connection;
        }

        public IEnumerator<Screen> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public async Task<Screen> Head()
        {
            var r = await _connection.Request<Screen>(new RestRequest("/screens/head", Method.GET));
            return r.Data;
        }
    }
}
