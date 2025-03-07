using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class Server : MonoBehaviour
{
    //initalising variables
    public Server instance;

    [Header("Port setup")]
    public int port = 2801;

    [Header("Client lists")]
    private List<ServerClient> clients;
    private List<ServerClient> disconectList;

    [Header("Server Infrastaucture")]
    private LinkedList<(string, ServerClient)> toSend = new();
    private TcpListener server;
    private bool serverStarted;

    [Header("References")]
    private ServerController serverController;
    private Encryption encrypter;

    public LinkedList<(string, ServerClient)> ToSend { get => toSend; set => toSend = value; }

    //Run before first frame, starts the server
    private void Start()
    {
        serverController = FindObjectOfType<ServerController>();
        encrypter = FindObjectOfType<Encryption>();
        instance = this;
        clients = new List<ServerClient>();
        disconectList = new List<ServerClient>();

        try
        {
            server = new TcpListener(IPAddress.Any, port);
            server.Start();

            StartListening();
            serverStarted = true;
            Debug.Log("Sever has beens started on port " + port.ToString());
            StartCoroutine(SendingData());
        }
        catch (Exception e)
        {
            Debug.LogError("Socket error: " + e.Message);
        }
    }

    //run every frame, checks for new requests and if the client is still connected
    private void Update()
    {
        if (!serverStarted)
        {
            return;
        }

        foreach (ServerClient c in clients)
        {
            //is the client still connected?
            if (!IsConnected(c.tcp))
            {
                c.tcp.Close();
                disconectList.Add(c);
                continue;
            }
            //check for message from the client
            else
            {
                NetworkStream s = c.tcp.GetStream();
                if (s.DataAvailable)
                {
                    StreamReader reader = new(s, true);
                    string data = reader.ReadLine();

                    if (data != null)
                    {
                        OnIncomingData(c, data);
                    }
                }
            }
        }

        for (int i = 0; i < disconectList.Count - 1; i++)
        {
            //Broadcast(disconectList[i].clientName + " has dissconeted", clients);
            Debug.LogWarning("Client disconnected : " + disconectList[i].clientName);
            clients.Remove(disconectList[i]);
            disconectList.RemoveAt(i);
        }
    }

    //called when new data is recieved
    private void OnIncomingData(ServerClient c, string rawData)
    {
        string data = encrypter.instance.Decrypt(rawData); //Decrypts the data
        //Debug.Log(c.clientName + " : " + data);

        //Checks to see if the server needs to respond
        if(!data.Contains("RECIEVED"))
        {
            ToSend.AddFirst(("%RECIEVED", c));
        }
        //Checks to see what type of request is being recieved
        if (data.Contains("&NAME"))
        {
            c.clientName = data.Split('|')[1];
            //Debug.Log("Name Returned from " + c.clientName);
            return;
        }
        else if (data.Contains("&STOCKLUDBR"))
        {
            string[] splitData = data.Split('|');
            //Debug.Log("Stock Lookup request from " + c.clientName + " has been received with the parameters : " + splitData[1] + " and " + splitData[2]);
            serverController.instance.StockLookupRequest(c, Convert.ToInt64(splitData[1]), splitData[2]);
            return;
        }
        else if (data.Contains("&CATEGORYDBR"))
        {
            string[] splitData = data.Split('|');
            //Debug.Log("Category request from " + c.clientName + " has been received with the parameter : " + splitData[1]);
            serverController.instance.CategoryRequest(c, Convert.ToInt32(splitData[1]));
        }
        else if (data.Contains("&CATEGORYITEMSDBR"))
        {
            string[] splitData = data.Split('|');
            //Debug.Log("Category item request from " + c.clientName + " has been received with the parameter : " + splitData[1]);
            serverController.instance.CategoryItemRequest(c, Convert.ToInt32(splitData[1]));
        }
        else if (data.Contains("&CATEGORYITEMNAMEDBR"))
        {
            string[] splitData = data.Split('|');
            //Debug.Log("Category item name request from " + c.clientName + " has been received with the parameter : " + splitData[1]);
            serverController.instance.CategoryItemDataRequest(c, Convert.ToInt64(splitData[1]));
        }
        else if (data.Contains("&STAFFLOGINDBR"))
        {
            string[] splitData = data.Split('|');
            //Debug.Log("Staff Login request from " + c.clientName + " has been received with the parameter : " + splitData[1]);
            serverController.instance.StaffLoginRequest(c, Convert.ToInt32(splitData[1]));
        }
        else if (data.Contains("&TRANSACTIONDBWR"))
        {
            string[] splitData = data.Split('|');
            //Debug.Log("Transaction write request from " + c.clientName + " has been recieved with the parameters : " + splitData[1] + ", " + splitData[2] + ", " + splitData[3] + ", " + splitData[4] + " and " + splitData[5]);
            Transaction transToAdd = new Transaction(Convert.ToInt64(splitData[1]), DateTime.Parse(splitData[2]), (float)Convert.ToDouble(splitData[3]), Convert.ToInt32(splitData[4]), Convert.ToInt32(splitData[5]), new List<(Item, int)>());
            serverController.instance.WriteTransactionData(c, transToAdd);
        }
        else if (data.Contains("&TRANSACTIONITEMDBWR"))
        {
            string[] splitData = data.Split('|');
            //Debug.Log("Transaction Item write request from " + c.clientName + " has been recieved with the parameters : " + splitData[1] + ", " + splitData[2] + ", " + splitData[3] + " and " + splitData[4]);
            serverController.instance.WriteTransactionItemData(c, Convert.ToInt64(splitData[1]), Convert.ToInt32(splitData[2]), Convert.ToInt32(splitData[3]), (float)Convert.ToDouble(splitData[4]));
        }
        else if (data.Contains("&AUTHSTAFFDBR"))
        {
            string[] splitData = data.Split('|');
            //Debug.Log("Authorising Staff member request from " + c.clientName + " has been recieved with the parameter : " + splitData[1]);
            serverController.instance.AuthStaffMemberData(c, Convert.ToInt32(splitData[1]));
        }
        else if (data.Contains("&RECIEVED"))
        {
            ToSend.RemoveFirst();
            return;
        }
        else if (data.Contains("&STOCKWR"))
        {
            string[] splitData = data.Split('|');
            serverController.instance.WriteStockItem(c, Convert.ToInt64(splitData[1]), splitData[2], (float)Convert.ToDouble(splitData[3]), Convert.ToInt32(splitData[4]));
        }
        else if (data.Contains("&STOCKDELETER"))
        {
            string[] splitData = data.Split('|');
            FindObjectOfType<DatabaseManager>().instance.DeleteValueInStockTable(Convert.ToInt64(splitData[1]));
        }
        else if (data.Contains("&STAFFWR"))
        {
            string[] splitData = data.Split('|');
            serverController.instance.WriteStaffMember(c, Convert.ToInt32(splitData[1]), splitData[2], splitData[3], splitData[4], splitData[5], splitData[6], Convert.ToInt32(splitData[7]));

            /*
             * ERROR: INT Overflow Error
             * DESCRIPTION: String-Int coversion not accepted
             * STATUS: Resolved
             * SOLUTION: Correct 'splitData' array counting (all were -1 out)
             */
        }
        else if (data.Contains("&DIRECTSTOCKDBR"))
        {
            string[] splitData = data.Split('|');
            serverController.instance.DirectStockRequest(c, Convert.ToInt32(splitData[1]));
        }
        else
        {
            Debug.LogWarning(c.clientName + " has sent the following message: " + data);
            return;
        }
    }

    //starts the server listening for new requests
    private void StartListening()
    {
        server.BeginAcceptTcpClient(AcceptTcpClient, server);
    }

    //checks if a client is connected still
    private bool IsConnected(TcpClient c)
    {
        try
        {
            if (c != null && c.Client != null && c.Client.Connected)
            {
                if (c.Client.Poll(0, SelectMode.SelectRead))
                {
                    return !(c.Client.Receive(new byte[1], SocketFlags.Peek) == 0);
                }
                return true;
            }
            else
            {
                return false;
            }
        }
        catch
        {
            return false;
        }
    }

    //accepts the clients' connection requests
    private void AcceptTcpClient(IAsyncResult ar)
    {
        TcpListener listener = (TcpListener)ar.AsyncState;
        clients.Add(new ServerClient(listener.EndAcceptTcpClient(ar)));
        Debug.Log("New Client Connected");
        //request a name from the client that just connected
        if (clients[^1].clientName == "Unknown")
        {
            ToSend.AddLast(("%NAME", clients[clients.Count - 1]));
            //Debug.Log("Requesting Client Name");
        }
        StartListening();
    }

    //used to send data to the clients
    public void Send(string rawData, ServerClient c)
    {
        string data = encrypter.instance.Encrypt(rawData);
        try
        {
            //Debug.Log("Sending data to client");
            StreamWriter writer = new StreamWriter(c.tcp.GetStream());
            writer.WriteLine(data);
            writer.Flush();
            if (rawData.Contains("RECIEVED"))
            {
                if(ToSend.FirstOrDefault().Item2 != null)
                {
                    ToSend.RemoveFirst();
                }
            }
        }
        catch (Exception e) //if there is an error
        {
            Debug.LogError("Write error : " + e.Message + "to client " + c.clientName);
            return;
        }

    }

    //Checks to see if the server can send data
    IEnumerator SendingData()
    {
        while (true)
        {
            //Debug.Log("Server Checking to send");
            if (ToSend.Count != 0)
            {
                //Debug.Log(toSend.Peek().Item1);
                int listLength = ToSend.Count;
                Send(ToSend.First().Item1, ToSend.First().Item2);
                //Send(toSend.Peek().Item1, toSend.Peek().Item2);
                yield return new WaitUntil(() => ToSend.Count != listLength);
                yield return new WaitForEndOfFrame();
            }
            else
            {
                yield return new WaitForEndOfFrame();
            }
        }
    }

    private void OnApplicationQuit()
    {
        server.Stop();
    }
}

//A class to store client details in
public class ServerClient
{
    public TcpClient tcp;
    public string clientName;

    //initilising funciton
    public ServerClient(TcpClient clientSocket)
    {
        clientName = "Unknown";
        tcp = clientSocket;
    }
}
