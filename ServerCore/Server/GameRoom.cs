using ServerCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    class GameRoom : IJobQueue
    {
        // TODO : 후에는 Dictionary로 Id랑 클라 세션을 물고 있어도 됨
        List<ClientSession> _sessions = new List<ClientSession>();
        JobQueue _jobQueue = new JobQueue();

        public void Push(Action job)
        {
            _jobQueue.Push(job);
        }

        public void Broadcast(ClientSession session, string chat)
        {
            // 채팅 메세지가 온다면 방의 모든 애들한테 메세지를 뿌리자
            S_Chat packet = new S_Chat();
            packet.playerId = session.SessionId;
            packet.chat = $"{chat} I am {packet.playerId}";

            ArraySegment<byte> segment = packet.Write();

            foreach (ClientSession clientSession in _sessions) {
                clientSession.Send(segment);
            }
        }

        public void Enter(ClientSession session)
        {
            _sessions.Add(session);
            session.Room = this;
        }

        public void Leave(ClientSession session)
        {
            _sessions.Remove(session);
        }
    }
}
