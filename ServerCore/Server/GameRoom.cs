using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    class GameRoom
    {
        // TODO : 후에는 Dictionary로 Id랑 클라 세션을 물고 있어도 됨
        List<ClientSession> _sessions = new List<ClientSession>();
        object _lock = new object();

        public void Broadcast(ClientSession session, string chat)
        {
            // 채팅 메세지가 온다면 방의 모든 애들한테 메세지를 뿌리자
            S_Chat packet = new S_Chat();
            packet.playerId = session.SessionId;
            packet.chat = chat;

            ArraySegment<byte> segment = packet.Write();

            // 공유 변수에 접근해야하므로 락을 걸자
            // 위에는 넘겨준 인자만 만들어주므로 상관없음
            lock (_lock) {
                foreach(ClientSession clientSession in _sessions) {
                    clientSession.Send(segment);
                }
            }
        }

        public void Enter(ClientSession session)
        {
            lock (_lock) {
                _sessions.Add(session);
                session.Room = this;
            }
        }

        public void Leave(ClientSession session)
        {
            lock (_lock) {
                _sessions.Remove(session);
            }
        }
    }
}
