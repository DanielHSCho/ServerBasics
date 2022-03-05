using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    class GameRoom
    {
        // TODO : 후에는 Dictionary로 Id랑 클라 세셧을 물고 있어도 됨
        List<ClientSession> _session = new List<ClientSession>();
        object _lock = new object();

        public void Enter(ClientSession session)
        {
            lock (_lock) {
                _session.Add(session);
                session.Room = this;
            }
        }

        public void Leave(ClientSession session)
        {
            lock (_lock) {
                _session.Remove(session);
            }
        }
    }
}
