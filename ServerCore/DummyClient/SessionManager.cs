using System;
using System.Collections.Generic;
using System.Text;

namespace DummyClient
{
    class SessionManager
    {
        private static SessionManager _session = new SessionManager();
        public static SessionManager Instance { get { return _session; } }

        private List<ServerSession> _sessionList = new List<ServerSession>();
        private object _lock = new object();

        public void SendForEach()
        {
            lock (_lock) {
                foreach(ServerSession session in _sessionList) {
                    C_Chat chatPacket = new C_Chat();
                    chatPacket.chat = $"Hellow Server!";
                    ArraySegment<byte> segment = chatPacket.Write();

                    session.Send(segment);
                }
            }
        }

        public ServerSession Generate()
        {
            lock (_lock) {
                ServerSession session = new ServerSession();
                _sessionList.Add(session);
                return session;
            }
        }
    }
}
