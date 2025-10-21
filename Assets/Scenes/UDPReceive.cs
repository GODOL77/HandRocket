using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class UDPReceive : MonoBehaviour
{
    private Thread _receiveThread;
    private UdpClient _client;
    public int port = 5052;
    public bool startReceiving = true;
    public bool printToConsole = false;
    public string data;

    public void Start()
    {
        _receiveThread = new Thread(ReceiveData)
        {
            IsBackground = true
        };
        _receiveThread.Start();
    }

    private void ReceiveData()
    {
        _client = new UdpClient(port);
        while (startReceiving)
        {
            try
            {
                IPEndPoint anyIP = new(IPAddress.Any, 0);
                byte[] dataBype = _client.Receive(ref anyIP);
                data = Encoding.UTF8.GetString(dataBype);
                
                if(printToConsole)
                    print(data);
            }
            catch (Exception e)
            {
                print(e.ToString());
            }
        }
    }
}
