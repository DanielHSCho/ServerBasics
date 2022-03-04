using ServerCore;
using System;
using System.Collections.Generic;
using System.Text;

// <패킷 제네레이터 5#> 22.03.04 - Register / OnRecvPacket / MakePacket / 델리게이트 딕셔너리
namespace Server
{
    class PacketManager
    {
        #region Singleton
        static PacketManager _instance;
        public static PacketManager Instance
        {
            get {
                if (_instance == null) {
                    _instance = new PacketManager();
                }
                return _instance;
            }
        }
        #endregion

        // 1. 프로토콜 ID로 특정 액션을 할 수 있도록
        Dictionary<ushort, Action<PacketSession, ArraySegment<byte>>> _onRecv = new Dictionary<ushort, Action<PacketSession, ArraySegment<byte>>>();

        // 3. 패킷 핸들러의 함수들과 대응되는 델리게이트를 관리하는 딕셔너리 추가
        Dictionary<ushort, Action<PacketSession, IPacket>> _handler = new Dictionary<ushort, Action<PacketSession, IPacket>>();

        // 2. Register 추가
        // -> 멀티쓰레드가 끼어들기 전에 한번은 해줄 수있도록 Program.cs에서 Register 해주자
        public void Register()
        {
            _onRecv.Add((ushort)PacketID.PlayerInfoReq, MakePacket<PlayerInfoReq>);
            // 4. 패킷 ReqInfo는 2번째인자의 핸들러 넘겨달라는 의미 
            _handler.Add((ushort)PacketID.PlayerInfoReq, PacketHandler.PlayerInfoReqHandler);
        }


        public void OnRecvPacket(PacketSession session, ArraySegment<byte> buffer)
        {
            ushort count = 0;

            ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
            count += 2;
            ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
            count += 2;

            // 6. 이 부분이 이제 아래처럼 변경됨
            //switch ((PacketID)id) {
            //    case PacketID.PlayerInfoReq: {
            //        }
            //        break;
            //}

            Action<PacketSession, ArraySegment<byte>> action = null;
            if(_onRecv.TryGetValue(id, out action)) {
                action.Invoke(session, buffer);
            }
        }

        // 2. 제네릭 타입 인자로 모든 Packet 클래스를 받을 수 있도록 함
        // - T인자의 조건은 IPacket이며, new가 가능해야하는 조건을 추가
        private void MakePacket<T>(PacketSession session, ArraySegment<byte> buffer) where T : IPacket, new()
        {
            T packet = new T();
            packet.Read(buffer);

            // 5. 핸들러를 찾은 다음에 호출
            Action<PacketSession, IPacket> action = null;
            if(_handler.TryGetValue(packet.Protocol, out action)) {
                action.Invoke(session, packet);
            }
        }
    }
}



//// <패킷 제네레이터 5#> 22.03.04 - 기초 작업 / Client Session의 부분 이전
//namespace Server
//{
//    // 1. 패킷 매니저 싱글턴 추가
//    class PacketManager
//    {
//        #region Singleton
//        static PacketManager _instance;
//        public static PacketManager Instance
//        {
//            get {
//                if(_instance == null) {
//                    _instance = new PacketManager();
//                }
//                return _instance;
//            }
//        }
//        #endregion

//        // 2. ClientSession 부분 이전
//        public void OnRecvPacket(PacketSession session, ArraySegment<byte> buffer)
//        {
//            ushort count = 0;

//            ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
//            count += 2;
//            ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
//            count += 2;

//            switch ((PacketID)id) {
//                case PacketID.PlayerInfoReq: {
//                        PlayerInfoReq req = new PlayerInfoReq();
//                        req.Read(buffer);
//                    }
//                    break;
//            }
//        }
//    }
//}
