using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    class GameRoom
    {
        // 1. 방에는 Session들이 모여 있음
        // TODO : 후에는 Dictionary로 Id랑 클라 세셧을 물고 있어도 됨
        List<ClientSession> _session = new List<ClientSession>();

        // 3. 멀티스레드에서는 Enter와 Leave가 동시 다발적으로 발생할 수 있다
        // - 이때 자료구조인 List나 딕셔너리에 Add해주는 것은 불안정 할 수 있으므로 락걸어주자
        object _lock = new object();

        // 2. Enter와 Leave API 추가
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
