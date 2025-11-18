using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public struct HandPacket
{
    public string State;
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
    private static bool _instanceExists = false;

    private Thread _receiveThread;
    private UdpClient _client;
    private readonly object _clientLock = new object();

    public int port = 5052;
    public bool startReceiving = true;
    public bool printToConsole;
    public float startDelaySeconds = 0.5f; // Python이 먼저 뜨도록 약간의 딜레이

    public HandPacket Packet => _packet;
    private HandPacket _packet = new("Fist", "");

    private void Awake()
    {
        // 중복 방지: 씬에 여러 인스턴스 생기는 걸 차단
        if (_instanceExists)
        {
            Debug.LogWarning("UDPReceive duplicated! Destroying new instance.");
            Destroy(gameObject);
            return;
        }
        _instanceExists = true;
        DontDestroyOnLoad(gameObject); // 필요 없다면 제거
    }

    private void OnEnable()
    {
        // 약간의 딜레이를 두고 수신 스레드 시작
        Invoke(nameof(StartReceiveThread), startDelaySeconds);
    }

    private void StartReceiveThread()
    {
        if (_receiveThread != null && _receiveThread.IsAlive)
            return;

        startReceiving = true;
        _receiveThread = new Thread(ReceiveData)
        {
            IsBackground = true
        };
        _receiveThread.Start();
    }

    private void ReceiveData()
    {
        // 단순 재시도 로직: 포트 점유 상황이면 조금 기다렸다가 재시도
        int attempts = 0;
        const int maxAttempts = 5;
        while (attempts < maxAttempts && startReceiving)
        {
            try
            {
                lock (_clientLock)
                {
                    // 이미 열려있다면 넘어감
                    if (_client == null)
                        _client = new UdpClient(port);
                }
                break; // 성공하면 루프 탈출
            }
            catch (Exception e)
            {
                attempts++;
                Debug.LogError($"UDP port {port} bind failed (attempt {attempts}/{maxAttempts}): {e.Message}");
                // 다른 프로세스가 포트를 잡고 있을 수 있으니 잠시 대기
                Thread.Sleep(300);
            }
        }

        if (_client == null)
        {
            Debug.LogError($"UDPReceive: Failed to bind UDP port {port} after {maxAttempts} attempts. Aborting receive thread.");
            return;
        }

        IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
        while (startReceiving)
        {
            try
            {
                byte[] dataBytes = _client.Receive(ref anyIP); // 블로킹 호출
                if (dataBytes == null || dataBytes.Length == 0)
                    continue;

                _packet.Data = Encoding.UTF8.GetString(dataBytes);

                if (string.IsNullOrEmpty(_packet.Data))
                    _packet.InitParams();

                if (printToConsole)
                    Debug.Log(_packet.Data);
            }
            catch (SocketException se)
            {
                // 소켓이 닫히면 Receive에서 예외 발생 -> 정상 종료 루틴으로 빠짐
                Debug.Log($"UDPReceive: SocketException (likely closed): {se.Message}");
                break;
            }
            catch (Exception ex)
            {
                Debug.LogError($"UDPReceive exception: {ex}");
                // 잠시 쉬고 재시도
                Thread.Sleep(10);
            }
        }

        // 정리
        try
        {
            lock (_clientLock)
            {
                _client?.Close();
                _client = null;
            }
        }
        catch { }
    }

    private void StopReceiveThread(bool waitForJoin = true)
    {
        startReceiving = false;

        // Close the socket to unblock Receive()
        try
        {
            lock (_clientLock)
            {
                _client?.Close();
            }
        }
        catch { }

        if (_receiveThread != null && _receiveThread.IsAlive)
        {
            if (waitForJoin)
            {
                // 안전하게 Join (짧은 타임아웃)
                if (!_receiveThread.Join(500))
                {
                    try { _receiveThread.Abort(); } catch { }
                }
            }
            else
            {
                try { _receiveThread.Abort(); } catch { }
            }
        }
        _receiveThread = null;
    }

    private void OnDisable()
    {
        StopReceiveThread();
    }

    private void OnApplicationQuit()
    {
        StopReceiveThread();
    }

    private void OnDestroy()
    {
        _instanceExists = false;
        StopReceiveThread();
    }
}
