using ServerCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    class PacketHandler
    {
        // 1. 함수 이름은 최대한 패킷이름과 동일하게 맞추자 (PlayerInfoReq -> PlayerInfoReqHandler)
        public static void PlayerInfoReqHandler(PacketSession session, IPacket packet)
        {
            // 2. 캐스팅
            PlayerInfoReq req = packet as PlayerInfoReq;
            Console.WriteLine($"PlayerInfoReq:{req.playerId} {req.name}");

            foreach (PlayerInfoReq.Skill skill in req.skills) {
                Console.WriteLine($"Skill({skill.id}:{skill.level}:{skill.duration})");
            }
        }
    }
}
