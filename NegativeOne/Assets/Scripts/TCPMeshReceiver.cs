using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class TCPMeshReceiver : MonoBehaviour {

    public int BUFFER = 4341760;

    public int listeningPort = 0;
    private TcpListener _server;

    private bool _running = true;

    public int SIZE = 0;

    void Start ()
    {
        listeningPort = int.Parse(GetComponent<Properties>().remoteSetupInfo.avatarListenPort);
        _server = new TcpListener(IPAddress.Any, listeningPort);
        _server.Start();

        Thread acceptLoop = new Thread(new ParameterizedThreadStart(AcceptClients));
        acceptLoop.Start();
    }

    void AcceptClients(object o)
    {
        while (_running)
        {
            TcpClient newclient = _server.AcceptTcpClient();
            Thread clientThread = new Thread(new ParameterizedThreadStart(clientHandler));
            clientThread.Start(newclient);
        }
    }

    void clientHandler(object o)
    {
        Debug.Log(": Accepted a connection");

        int SIZEHELLO = 200;
        TcpClient client = (TcpClient)o;

        bool login = false;

        using (NetworkStream ns = client.GetStream())
        {
            byte[] message = new byte[SIZEHELLO];
            int bytesRead = 0;
            byte[] buffer = new byte[BUFFER];

            try
            {
                bytesRead = ns.Read(message, 0, SIZEHELLO);
            }
            catch (Exception e)
            {
                Debug.Log(": Connection Lost");
                Debug.LogException(e);
                _close(client);
            }

            if (bytesRead == 0)
            {
                Debug.Log("bytesRead == 0");
                _close(client);
            }

            // start login
            Debug.Log("Start Login");
            string s = System.Text.Encoding.Default.GetString(message);
            string[] l = s.Split('/');
            if (l.Length == 3 && l[0] == "k")
            {
                Debug.Log("New stream from " + l[1]);
                login = true;
            }
            else
            {
                Debug.LogError("Invalid Login data from: ");
                _close(client);
            }

            while (login && _running)
            {
                try
                {
                    bytesRead = ns.Read(message, 0, 4);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    _running = false;
                    break;
                }

                if (bytesRead == 0)
                {
                    _running = false;
                    break;
                }

                byte[] sizeb = { message[0], message[1], message[2], message[3] };
                int size = BitConverter.ToInt32(sizeb, 0);

                SIZE = size;
                Debug.Log(size);
                int index = 0;
                while (size > 0)
                {
                    try
                    {
                        if (size < buffer.Length)
                        {
                            bytesRead = ns.Read(buffer, index, size - index);
                        }
                        else Debug.LogError("size is bigger: " + size);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError("BUFFER: " + buffer.Length + "     SIZE: " + size);
                        Debug.LogException(e);
                        login = false;
                        break;
                    }
                    if (bytesRead == 0)
                    {
                        login = false;
                        break;
                    }
                    index += bytesRead;
                    size -= bytesRead;
                }
            }

        }
    }

    void Update ()
    {
		
	}

    private void _close(TcpClient client)
    {
        Exception e = new Exception("CLOSE EXCEPTION");
        Debug.LogException(e);
        Debug.Log(e.StackTrace);

        client.GetStream().Close();
        client.Close();
        client = null;
        //Debug.Break();
    }
}
