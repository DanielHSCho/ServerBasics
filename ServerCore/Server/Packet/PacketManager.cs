using ServerCore;
using System;
using System.Collections.Generic;
using System.Text;

// <패킷 제네레이터 5#> 22.03.04 - 기초 작업 / Client Session의 부분 이전
namespace Server
{
    // 1. 패킷 매니저 싱글턴 추가
    class PacketManager
    {
        #region Singleton
        static PacketManager _instance;
        public static PacketManager Instance
        {
            get {
                if(_instance == null) {
                    _instance = new PacketManager();
                }
                return _instance;
            }
        }
        #endregion

        // 2. ClientSession 부분 이전
        public void OnRecvPacket(PacketSession session, ArraySegment<byte> buffer)
        {
            ushort count = 0;

            ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
            count += 2;
            ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
            count += 2;

            switch ((PacketID)id) {
                case PacketID.PlayerInfoReq: {
                        PlayerInfoReq req = new PlayerInfoReq();
                        req.Read(buffer);
                    }
                    break;
            }
        }
    }
}
