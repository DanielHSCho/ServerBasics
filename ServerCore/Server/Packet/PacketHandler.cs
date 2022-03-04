using ServerCore;
using System;
using System.Collections.Generic;
using System.Text;

class PacketHandler
{
    public static void PlayerInfoReqHandler(PacketSession session, IPacket packet)
    {
        PlayerInfoReq req = packet as PlayerInfoReq;
        Console.WriteLine($"PlayerInfoReq:{req.playerId} {req.name}");

        foreach (PlayerInfoReq.Skill skill in req.skills) {
            Console.WriteLine($"Skill({skill.id}:{skill.level}:{skill.duration})");
        }
    }

    public static void TestHandler(PacketSession session, IPacket packet)
    {

    }
}



// <패킷 제네레이터 5#> 22.03.04 - 패킷 핸들러 추가
//namespace Server
//{
//    class PacketHandler
//    {
//        // 1. 함수 이름은 최대한 패킷이름과 동일하게 맞추자 (PlayerInfoReq -> PlayerInfoReqHandler)
//        public static void PlayerInfoReqHandler(PacketSession session, IPacket packet)
//        {
//            // 2. 캐스팅
//            PlayerInfoReq req = packet as PlayerInfoReq;
//            Console.WriteLine($"PlayerInfoReq:{req.playerId} {req.name}");

//            foreach (PlayerInfoReq.Skill skill in req.skills) {
//                Console.WriteLine($"Skill({skill.id}:{skill.level}:{skill.duration})");
//            }
//        }
//    }
//}
