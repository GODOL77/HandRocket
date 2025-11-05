using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;



public struct HandPacket
{
    public string State;    // 손이 Object를 못 들어올릴 때 State를 구분해서 손에 Attach 하는용
    public string Data;

    public HandPacket(string state, string data)
    {
        State = state;
        Data = data;
    }

    public void InitParams()
    {
        State = "";
        Data = "0";
    }
}

public class UDPReceive : MonoBehaviour
{
    private Thread _receiveThread;
    private UdpClient _client;
    public int port = 5052;
    public bool startReceiving = true;
    public bool printToConsole;

    public HandPacket Packet => _packet;
    private HandPacket _packet = new("Fist", "");

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
                _packet.Data = Encoding.UTF8.GetString(dataBype);

                if (_packet.Data == "" || string.IsNullOrEmpty(_packet.Data))
                {
                    _packet.InitParams();
                    print(_packet.Data);
                }
                
                if(printToConsole)
                    print(_packet.Data);
            }
            catch (Exception e)
            {
                print(e.ToString());
            }
        }
    }
}
