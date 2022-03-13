using DummyClient;
using ServerCore;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    ServerSession _session = new ServerSession();
    // 갯수는 유니티 클라에선 1개만 
    private int _simulationCount = 1;

    void Start()
    {
        string host = Dns.GetHostName();
        IPHostEntry ipHost = Dns.GetHostEntry(host);
        IPAddress ipAddr = ipHost.AddressList[0];
        IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

        // TODO : 동작은 하지만, try catch 처리로 네트워크 실패 처리 해야함
        Connector connector = new Connector();
        connector.Connect(endPoint, () => { return _session; }, _simulationCount);
    }

    void Update()
    {
        
    }
}
