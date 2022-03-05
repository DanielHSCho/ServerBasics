using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Threading;
using ServerCore;

namespace Server
{
    class ClientSession : PacketSession
    {
        public int SessionId { get; set; }
        public GameRoom Room { get; set; }

        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"On Connected : {endPoint}");

            // TODO : 채팅 테스트를 위해 임시로 어떤 채팅방에 강제 입장
            // TODO : 실제 게임 개발 시에는 입장 후 이 단계에서 클라가 리소스 로딩 다 했다고 신호 보내면 그때 입장 처리해야함

            Program.Room.Enter(this);

            // 끊어주는건 임시 주석
            // Thread.Sleep(5000);
            // Disconnect();
        }

        public override void OnRecvPacket(ArraySegment<byte> buffer)
        {
            PacketManager.Instance.OnRecvPacket(this, buffer);
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            SessionManager.Instance.Remove(this);

            if(Room != null) {
                Room.Leave(this);
                Room = null;
            }

            Console.WriteLine($"On Disconnected : {endPoint}");
        }

        public override void OnSend(int numOfBytes)
        {
            Console.WriteLine($"Transferred Bytes : {numOfBytes}");
        }
    }
}

//// <패킷 제네레이터> 22.02.28 - Packet 제네레이터로 생성한 패킷 테스트 
//namespace Server
//{
//	class ClientSession : PacketSession
//    {
//        public override void OnConnected(EndPoint endPoint)
//        {
//            Console.WriteLine($"On Connected : {endPoint}");
//            //Send(sendBuff);
//            Thread.Sleep(5000);
//            Disconnect();
//        }

//        public override void OnRecvPacket(ArraySegment<byte> buffer)
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
//                        Console.WriteLine($"PlayerInfoReq:{req.playerId} {req.name}");

//                        // 1. List 빼는 부분 추가
//                        foreach (PlayerInfoReq.Skill skill in req.skills) {
//                            Console.WriteLine($"Skill({skill.id}:{skill.level}:{skill.duration})");
//                        }
//                    }
//                    break;
//            }

//            Console.WriteLine($"PlayerPacketId:{id}, Size:{size}");
//        }

//        public override void OnDisconnected(EndPoint endPoint)
//        {
//            Console.WriteLine($"On Disconnected : {endPoint}");
//        }

//        public override void OnSend(int numOfBytes)
//        {
//            Console.WriteLine($"Transferred Bytes : {numOfBytes}");
//        }
//    }
//}




// <Packet 직렬화 4#> 22.02.27 - List 싱크
//namespace Server
//{
//    public abstract class Packet
//    {
//        public ushort size;
//        public ushort packetId;

//        public abstract ArraySegment<byte> Write();
//        public abstract void Read(ArraySegment<byte> segment);
//    }

//    class PlayerInfoReq : Packet
//    {
//        public long playerId;
//        public string name;

//        public struct SkillInfo
//        {
//            public int id;
//            public short level;
//            public float duration;

//            public bool Write(Span<byte> s, ref ushort count)
//            {
//                bool success = true;

//                success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.id);
//                count += sizeof(int);
//                success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.level);
//                count += sizeof(short);
//                success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.duration);
//                count += sizeof(float);

//                return success;
//            }

//            public void Read(ReadOnlySpan<byte> s, ref ushort count)
//            {
//                this.id = BitConverter.ToInt32(s.Slice(count, s.Length - count));
//                count += sizeof(int);
//                this.level = BitConverter.ToInt16(s.Slice(count, s.Length - count));
//                count += sizeof(short);
//                this.duration = BitConverter.ToSingle(s.Slice(count, s.Length - count));
//                count += sizeof(float);
//            }
//        }

//        public List<SkillInfo> skills = new List<SkillInfo>();

//        public PlayerInfoReq()
//        {
//            this.packetId = (ushort)PacketID.PlayerInfoReq;
//        }

//        public override void Read(ArraySegment<byte> segment)
//        {
//            ushort count = 0;

//            ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);

//            // size / packetId
//            count += sizeof(ushort);
//            count += sizeof(ushort);

//            this.playerId = BitConverter.ToInt64(s.Slice(count, s.Length - count));
//            count += sizeof(long);

//            // string
//            ushort nameLength = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
//            count += sizeof(ushort);
//            this.name = Encoding.Unicode.GetString(s.Slice(count, nameLength));
//            count += nameLength;

//            skills.Clear();
//            ushort skillLength = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
//            count += sizeof(ushort);
//            for (int i = 0; i < skillLength; i++) {
//                SkillInfo skill = new SkillInfo();
//                skill.Read(s, ref count);
//                skills.Add(skill);
//            }
//        }

//        public override ArraySegment<byte> Write()
//        {
//            ArraySegment<byte> segment = SendBufferHelper.Open(4096);

//            ushort count = 0;
//            bool success = true;

//            Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);
//            count += sizeof(ushort);
//            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.packetId);
//            count += sizeof(ushort);
//            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.playerId);
//            count += sizeof(long);

//            // string
//            ushort nameLength = (ushort)Encoding.Unicode.GetBytes(this.name, 0, this.name.Length, segment.Array, segment.Offset + count + sizeof(ushort));
//            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), nameLength);
//            count += sizeof(ushort);
//            count += nameLength;

//            // TODO : 나중에는 이 List 부분도 자동화 하는게 더 좋음
//            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)skills.Count);
//            count += sizeof(ushort);

//            foreach (SkillInfo skill in skills) {
//                success &= skill.Write(s, ref count);
//            }

//            success &= BitConverter.TryWriteBytes(s, count);

//            if (success == false) {
//                return null;
//            }

//            return SendBufferHelper.Close(count);
//        }
//    }

//    //class PlayerInfoOk : Packet
//    //{
//    //    public int hp;
//    //    public int attack;
//    //}

//    public enum PacketID
//    {
//        PlayerInfoReq = 1,
//        PlayerInfoOk = 2
//    }

//    class ClientSession : PacketSession
//    {
//        public override void OnConnected(EndPoint endPoint)
//        {
//            Console.WriteLine($"On Connected : {endPoint}");
//            //Send(sendBuff);
//            Thread.Sleep(5000);
//            Disconnect();
//        }

//        public override void OnRecvPacket(ArraySegment<byte> buffer)
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
//                        Console.WriteLine($"PlayerInfoReq:{req.playerId} {req.name}");

//                        // 1. List 빼는 부분 추가
//                        foreach(PlayerInfoReq.SkillInfo skill in req.skills) {
//                            Console.WriteLine($"Skill({skill.id}:{skill.level}:{skill.duration})");
//                        }
//                    }
//                    break;
//            }

//            Console.WriteLine($"PlayerPacketId:{id}, Size:{size}");
//        }

//        public override void OnDisconnected(EndPoint endPoint)
//        {
//            Console.WriteLine($"On Disconnected : {endPoint}");
//        }

//        public override void OnSend(int numOfBytes)
//        {
//            Console.WriteLine($"Transferred Bytes : {numOfBytes}");
//        }
//    }
//}



// <Packet 직렬화 3#> 22.02.27 - ServerSession에 string 처리 싱크
//namespace Server
//{
//    public abstract class Packet
//    {
//        public ushort size;
//        public ushort packetId;

//        public abstract ArraySegment<byte> Write();
//        public abstract void Read(ArraySegment<byte> segment);
//    }

//    class PlayerInfoReq : Packet
//    {
//        public long playerId;
//        public string name;

//        public PlayerInfoReq()
//        {
//            this.packetId = (ushort)PacketID.PlayerInfoReq;
//        }

//        public override void Read(ArraySegment<byte> segment)
//        {
//            ushort count = 0;

//            ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);

//            // size / packetId
//            count += sizeof(ushort);
//            count += sizeof(ushort);

//            this.playerId = BitConverter.ToInt64(s.Slice(count, s.Length - count));
//            count += sizeof(long);

//            // string
//            ushort nameLength = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
//            count += sizeof(ushort);
//            this.name = Encoding.Unicode.GetString(s.Slice(count, nameLength));
//            count += nameLength;
//        }

//        public override ArraySegment<byte> Write()
//        {
//            ArraySegment<byte> segment = SendBufferHelper.Open(4096);

//            ushort count = 0;
//            bool success = true;

//            Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);
//            count += sizeof(ushort);
//            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.packetId);
//            count += sizeof(ushort);
//            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.playerId);
//            count += sizeof(long);

//            // string
//            ushort nameLength = (ushort)Encoding.Unicode.GetByteCount(this.name);
//            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), nameLength);
//            count += sizeof(ushort);
//            Array.Copy(Encoding.Unicode.GetBytes(this.name), 0, segment.Array, count, nameLength);
//            count += nameLength;

//            success &= BitConverter.TryWriteBytes(s, count);

//            if (success == false) {
//                return null;
//            }

//            return SendBufferHelper.Close(count);
//        }
//    }

//    public enum PacketID
//    {
//        PlayerInfoReq = 1,
//        PlayerInfoOk = 2
//    }

//    class ClientSession : PacketSession
//    {
//        public override void OnConnected(EndPoint endPoint)
//        {
//            Console.WriteLine($"On Connected : {endPoint}");
//            //Send(sendBuff);
//            Thread.Sleep(5000);
//            Disconnect();
//        }

//        public override void OnRecvPacket(ArraySegment<byte> buffer)
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
//                        Console.WriteLine($"PlayerInfoReq:{req.playerId} {req.name}");
//                    }
//                    break;
//            }

//            Console.WriteLine($"PlayerPacketId:{id}, Size:{size}");
//        }

//        public override void OnDisconnected(EndPoint endPoint)
//        {
//            Console.WriteLine($"On Disconnected : {endPoint}");
//        }

//        public override void OnSend(int numOfBytes)
//        {
//            Console.WriteLine($"Transferred Bytes : {numOfBytes}");
//        }
//    }
//}



// <Packet 직렬화 3#> 22.02.26 - ServerSession 개선된 것 싱크
//namespace Server
//{
//    public abstract class Packet
//    {
//        public ushort size;
//        public ushort packetId;

//        public abstract ArraySegment<byte> Write();
//        public abstract void Read(ArraySegment<byte> segment);
//    }

//    class PlayerInfoReq : Packet
//    {
//        public long playerId;
//        public PlayerInfoReq()
//        {
//            this.packetId = (ushort)PacketID.PlayerInfoReq;
//        }

//        public override void Read(ArraySegment<byte> segment)
//        {
//            ushort count = 0;

//            ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);

//            ushort size = BitConverter.ToUInt16(segment.Array, segment.Offset);
//            count += 2;
//            // packetId
//            count += 2;

//            this.playerId = BitConverter.ToInt64(s.Slice(count, s.Length - count));
//            count += 8;
//        }

//        public override ArraySegment<byte> Write()
//        {
//            ArraySegment<byte> segment = SendBufferHelper.Open(4096);

//            ushort count = 0;
//            bool success = true;

//            Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);

//            count += sizeof(ushort);
//            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.packetId);
//            count += sizeof(ushort);
//            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.playerId);
//            count += sizeof(long);
//            success &= BitConverter.TryWriteBytes(s, count);

//            if (success == false) {
//                return null; // 널체크를 할 수 있음
//            }

//            return SendBufferHelper.Close(count);
//        }
//    }

//    public enum PacketID
//    {
//        PlayerInfoReq = 1,
//        PlayerInfoOk = 2
//    }

//    class ClientSession : PacketSession
//    {
//        public override void OnConnected(EndPoint endPoint)
//        {
//            Console.WriteLine($"On Connected : {endPoint}");
//            //Send(sendBuff);
//            Thread.Sleep(5000);
//            Disconnect();
//        }

//        public override void OnRecvPacket(ArraySegment<byte> buffer)
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
//                        Console.WriteLine($"PlayerInfoReq:{req.playerId}");
//                    }
//                    break;
//            }

//            Console.WriteLine($"PlayerPacketId:{id}, Size:{size}");
//        }

//        public override void OnDisconnected(EndPoint endPoint)
//        {
//            Console.WriteLine($"On Disconnected : {endPoint}");
//        }

//        public override void OnSend(int numOfBytes)
//        {
//            Console.WriteLine($"Transferred Bytes : {numOfBytes}");
//        }
//    }
//}



// <Packet 직렬화 2#> 22.02.26 - ServerSession Packet 직렬화부분 추상화된 것 여기도 변경
//namespace Server
//{
//    // 2. 패킷 부분은 ServerSession에서 거의 복붙한 것
//    public abstract class Packet
//    {
//        public ushort size;
//        public ushort packetId;

//        public abstract ArraySegment<byte> Write();
//        public abstract void Read(ArraySegment<byte> segment);
//    }

//    class PlayerInfoReq : Packet
//    {
//        public long playerId;
//        public PlayerInfoReq()
//        {
//            this.playerId = (ushort)PacketID.PlayerInfoReq;
//        }

//        public override void Read(ArraySegment<byte> segment)
//        {
//            ushort count = 0;

//            ushort size = BitConverter.ToUInt16(segment.Array, segment.Offset);
//            count += 2;
//            count += 2;

//            // 3. ServerSession 쪽도 유효범위 지정해주므로 여기도 똑같이 싱크
//            //this.playerId = BitConverter.ToInt64(segment.Array, segment.Offset + count);
//            this.playerId = BitConverter.ToInt64(new ReadOnlySpan<byte>(segment.Array, segment.Offset + count, segment.Count - count));
//            count += 8;
//        }

//        public override ArraySegment<byte> Write()
//        {
//            ArraySegment<byte> openSegment = SendBufferHelper.Open(4096);

//            ushort count = 0;
//            bool success = true;

//            count += 2;
//            success &= BitConverter.TryWriteBytes(new Span<byte>(openSegment.Array, openSegment.Offset + count, openSegment.Count - count)
//                , this.packetId);
//            count += 2;
//            success &= BitConverter.TryWriteBytes(new Span<byte>(openSegment.Array, openSegment.Offset + count, openSegment.Count - count)
//                , this.playerId);
//            count += 8;
//            success &= BitConverter.TryWriteBytes(new Span<byte>(openSegment.Array, openSegment.Offset, openSegment.Count),
//                count);

//            if (success == false) {
//                return null; // 널체크를 할 수 있음
//            }

//            return SendBufferHelper.Close(count);
//        }
//    }

//    public enum PacketID
//    {
//        PlayerInfoReq = 1,
//        PlayerInfoOk = 2
//    }

//    class ClientSession : PacketSession
//    {
//        public override void OnConnected(EndPoint endPoint)
//        {
//            Console.WriteLine($"On Connected : {endPoint}");
//            //Send(sendBuff);
//            Thread.Sleep(5000);
//            Disconnect();
//        }

//        public override void OnRecvPacket(ArraySegment<byte> buffer)
//        {
//            ushort count = 0;

//            ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
//            count += 2;
//            ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
//            count += 2;

//            switch ((PacketID)id) {
//                case PacketID.PlayerInfoReq: {
//                        // 1. 
//                        //long playerId = BitConverter.ToInt64(buffer.Array, buffer.Offset + count);
//                        //count += 8;

//                        // 1. 위에 부분을 이렇게 개선
//                        PlayerInfoReq req = new PlayerInfoReq();
//                        req.Read(buffer);
//                        Console.WriteLine($"PlayerInfoReq:{req.playerId}");
//                    }
//                    break;
//            }

//            Console.WriteLine($"PlayerPacketId:{id}, Size:{size}");
//        }

//        public override void OnDisconnected(EndPoint endPoint)
//        {
//            Console.WriteLine($"On Disconnected : {endPoint}");
//        }

//        public override void OnSend(int numOfBytes)
//        {
//            Console.WriteLine($"Transferred Bytes : {numOfBytes}");
//        }
//    }
//}



// <Packet 직렬화 1#> 22.02.26 - 패킷 역직렬화
//namespace Server
//{
//    class Packet
//    {
//        public ushort size;
//        public ushort packetId;
//    }

//    class PlayerInfoReq : Packet
//    {
//        public long playerId;
//    }

//    class PlayerInfoOk : Packet
//    {
//        public int hp;
//        public int attack;
//    }

//    public enum PacketID
//    {
//        PlayerInfoReq = 1,
//        PlayerInfoOk = 2
//    }

//    class ClientSession : PacketSession
//    {
//        public override void OnConnected(EndPoint endPoint)
//        {
//            Console.WriteLine($"On Connected : {endPoint}");

//            //Packet packet = new Packet() { size = 100, packetId = 10 }; 

//            //ArraySegment<byte> openSegment = SendBufferHelper.Open(4096);
//            //byte[] buffer = BitConverter.GetBytes(packet.size); 
//            //byte[] buffer2 = BitConverter.GetBytes(packet.packetId); 
//            //Array.Copy(buffer, 0, openSegment.Array, openSegment.Offset, buffer.Length);
//            //Array.Copy(buffer2, 0, openSegment.Array, openSegment.Offset + buffer.Length, buffer2.Length);
//            //ArraySegment<byte> sendBuff = SendBufferHelper.Close(buffer.Length + buffer2.Length);

//            //// TODO : 나중엔 세션 매니저에서 기억하게끔 만들어줘야 함
//            //// 스트레스 테스트 + 튜닝하면서 안정성 검증 필요
//            //Send(sendBuff);
//            Thread.Sleep(5000);
//            Disconnect();
//        }

//        public override void OnRecvPacket(ArraySegment<byte> buffer)
//        {
//            // 1. 패킷 파싱부분 추가
//            ushort count = 0;

//            ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
//            count += 2;
//            ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
//            count += 2;

//            // 2. 패킷 Id 처리
//            switch ((PacketID)id) {
//                case PacketID.PlayerInfoReq: {
//                        // 3. 8바이트이므로 64
//                        long playerId = BitConverter.ToInt64(buffer.Array, buffer.Offset + count);
//                        count += 8;
//                        Console.WriteLine($"PlayerInfoReq:{playerId}");
//                    }
//                    break;
//            }

//            Console.WriteLine($"PlayerPacketId:{id}, Size:{size}");
//        }

//        public override void OnDisconnected(EndPoint endPoint)
//        {
//            Console.WriteLine($"On Disconnected : {endPoint}");
//        }

//        public override void OnSend(int numOfBytes)
//        {
//            Console.WriteLine($"Transferred Bytes : {numOfBytes}");
//        }
//    }
//}




// <패킷 직렬화> - 22.02.24 - 클라세션 / 패킷 싱크
//namespace Server
//{
//    class Packet
//    {
//        public ushort size;
//        public ushort packetId;
//    }

//    // 5. 클라의 ServerSession에서 복붙 -> 후에 공통부분으로 이전필요함
//    class PlayerInfoReq : Packet
//    {
//        public long playerId;
//    }

//    class PlayerInfoOk : Packet
//    {
//        public int hp;
//        public int attack;
//    }

//    public enum PacketID
//    {
//        PlayerInfoReq = 1,
//        PlayerInfoOk = 2
//    }

//    class ClientSession : PacketSession
//    {
//        public override void OnConnected(EndPoint endPoint)
//        {
//            Console.WriteLine($"On Connected : {endPoint}");

//            //Packet packet = new Packet() { size = 100, packetId = 10 }; 

//            //ArraySegment<byte> openSegment = SendBufferHelper.Open(4096);
//            //byte[] buffer = BitConverter.GetBytes(packet.size); 
//            //byte[] buffer2 = BitConverter.GetBytes(packet.packetId); 
//            //Array.Copy(buffer, 0, openSegment.Array, openSegment.Offset, buffer.Length);
//            //Array.Copy(buffer2, 0, openSegment.Array, openSegment.Offset + buffer.Length, buffer2.Length);
//            //ArraySegment<byte> sendBuff = SendBufferHelper.Close(buffer.Length + buffer2.Length);

//            //// TODO : 나중엔 세션 매니저에서 기억하게끔 만들어줘야 함
//            //// 스트레스 테스트 + 튜닝하면서 안정성 검증 필요
//            //Send(sendBuff);
//            Thread.Sleep(5000);
//            Disconnect();
//        }

//        public override void OnRecvPacket(ArraySegment<byte> buffer)
//        {
//            ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
//            ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + 2);
//            Console.WriteLine($"RecvSize:{size} / RecvId:{id}");
//        }

//        public override void OnDisconnected(EndPoint endPoint)
//        {
//            Console.WriteLine($"On Disconnected : {endPoint}");
//        }

//        public override void OnSend(int numOfBytes)
//        {
//            Console.WriteLine($"Transferred Bytes : {numOfBytes}");
//        }
//    }
//}
