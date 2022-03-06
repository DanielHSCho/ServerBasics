using DummyClient;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Text;

class PacketHandler
{
    public static void S_ChatHandler(PacketSession session, IPacket packet)
    {
        S_Chat chatPacket = packet as S_Chat;
        ServerSession serverSession = session as ServerSession;

        // 맨처음 아이인 경우에만 콘솔출력을 하도록 가정
        if(chatPacket.playerId == 1) {
            Console.WriteLine(chatPacket.chat);
        }
    }
}