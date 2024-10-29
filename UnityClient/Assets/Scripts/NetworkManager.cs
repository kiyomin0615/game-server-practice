using System.Net;
using UnityEngine;
using ServerCore;
using Client;

public class NetworkManager : MonoBehaviour
{
    ServerSession serverSession = new ServerSession();

    void Start()
    {
        string host = Dns.GetHostName();
        IPHostEntry ipInstance = Dns.GetHostEntry(host);
        IPAddress ip = ipInstance.AddressList[0];
        IPEndPoint endPoint = new IPEndPoint(ip, 7777);

        Connector connector = new Connector();

        connector.Connect(endPoint, () =>
            {
                return serverSession;
            });
    }

    void Update()
    {

    }
}
