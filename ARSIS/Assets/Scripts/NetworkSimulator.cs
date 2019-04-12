using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Collections.Concurrent;
using System.Threading.Tasks;
#if !UNITY_EDITOR
using Windows.Networking;
using Windows.Networking.Connectivity;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using System.IO;
using System.IO.Compression;
using System.Linq;
using UnityEngine.XR.WSA;
using UnityEngine.XR.WSA.WebCam;

#endif


public class NetworkSimulator : MonoBehaviour
{
    public static NetworkSimulator NetworkSimulatorSingleton = null;
    public int serverPort = 32123;
    public int targetPort = 32123;
    public string targetIP = "";
    public bool targetIPReady = false;
    public bool socketStarted = false;
    public volatile bool connected = false;
    private ConcurrentQueue<messagePackage> outgoingQueue = null;
    public UnityEngine.UI.Text outputText = null;
    public string currentOutput = "";
    private byte[] incomingBuffer = null;
    private Stack<LineRenderer> lineRenderers = null;
    private ConcurrentQueue<lrStruct> incomingLineRenderers = null;
    private bool undoLineRenderer = false;
    public Material LineRendererDefaultMaterial = null;
    public InputField Derp = null;
    public string debugText = "";
    public bool trashUDP = false;
#if !UNITY_EDITOR

    public StreamSocket tcpClient = null;
    public Windows.Storage.Streams.IOutputStream outputStream = null;
    public Windows.Storage.Streams.IInputStream inputStream = null;
    DataWriter writer = null;
    DataReader reader = null;
    
    //udp broadcast listening
    DatagramSocket listenerSocket = null;
    const string udpPort = "32124";
#endif

    private struct lrStruct
    {
        public float r, g, b, a, pc, sw, ew;
        public Vector3[] verts;
    }

    private class messagePackage
    {
        public byte[] bytes = null;
        public messagePackage(byte[] b) { bytes = b; }
    }

    //public Mesh testMesh = null;
    // Start is called before the first frame update
    void Start()
    {
        if (NetworkSimulatorSingleton != null)
        {
            Destroy(this);
            return;
        }
        lineRenderers = new Stack<LineRenderer>();
        incomingLineRenderers = new ConcurrentQueue<lrStruct>();
        outgoingQueue = new ConcurrentQueue<messagePackage>();
        NetworkSimulatorSingleton = this;
        
#if !UNITY_EDITOR
        Listen();
#endif
    }

    public void doSocketSetup()
    {
        //Task t = 
            setupSocket();
        //t.Start();
    }

    public async Task setupSocket()
    {

#if !UNITY_EDITOR
        tcpClient = new Windows.Networking.Sockets.StreamSocket();
        tcpClient.Control.NoDelay = false;
        tcpClient.Control.KeepAlive = false;
        tcpClient.Control.OutboundBufferSizeInBytes = 1500;
        
       
        while (!connected)
        {
            try
            {
                textOut("Connecting to " + targetIP + " " + targetPort);
                await tcpClient.ConnectAsync(new HostName(targetIP), "" + targetPort);
                textOut("Connected!");
                
               outputStream = tcpClient.OutputStream;
                inputStream = tcpClient.InputStream;
                writer = new DataWriter(outputStream);
                reader = new DataReader(inputStream);
                reader.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;
                reader.ByteOrder = Windows.Storage.Streams.ByteOrder.LittleEndian;
                reader.InputStreamOptions = InputStreamOptions.Partial;
                connected = true;
                
                while (connected)
                {
                    await reader.LoadAsync(8192);
                    if (reader.UnconsumedBufferLength>4)
                    {
                        textOut("Reading....");
                        int incomingSize = reader.ReadInt32();
                        if(incomingSize>0&&incomingSize < 100000)
                        {
                            //await reader.LoadAsync((uint)incomingSize);//preloads the buffer with the data which makes the following not needed.
                            
                            while (reader.UnconsumedBufferLength<incomingSize)
                            {
                                System.Threading.Tasks.Task.Delay(100).Wait();
                                await reader.LoadAsync(8192);
                            }
                            
                            textOut("Getting new Line!");
                            int packetType = reader.ReadInt32();
                            float r = reader.ReadSingle();
                            float g = reader.ReadSingle();
                            float b = reader.ReadSingle();
                            float a = reader.ReadSingle();
                            int count = reader.ReadInt32();// this is actually just for padding...
                            float sw = reader.ReadSingle();
                            float ew = reader.ReadSingle();
                            byte[] packet = new byte[incomingSize-36];
                            //reader.ReadBytes(packet);
                            if(packetType==4&&packet.Length>0)
                            {
                                lrStruct l = new lrStruct
                                {
                                    r = r,
                                    g = g,
                                    b = b,
                                    a = a,
                                    pc = count,
                                    sw = sw,
                                    ew = ew,
                                    verts = new Vector3[count]
                                };
                                textOut("" + count + " " + r + " " + g + " " + b + " " + a + " " + sw + " " + ew + "\n" + "Count Suggested Bytes:"+(count*4*3)+" Preloaded Package Size:"+packet.Length);

                                for (int i = 0; i < count; i++)//Dan Simplified this. Probably not bugged.
                                {                 
                                    l.verts[i]=new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                                }
                                incomingLineRenderers.Enqueue(l);
                                textOut("Line Renderer Enqueued");
                            }
                            if (packetType == 5)
                                undoLineRenderer = true;


                        }
                        else
                        {
                            //TODO Handle it.
                        }

                    }
                    
                }


                //outputStream = await udpClient.GetOutputStreamAsync(new HostName(targetIP), "" + targetPort);
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
                connected = false;
                socketStarted = false;
                return;
            }
        }
#endif
    }

    public void forceLocalConnection()
    {
        targetIP = Derp.text;
        targetIPReady = true;
    }

#if !UNITY_EDITOR

    private async Task Listen()
    {
        listenerSocket = new DatagramSocket();
        listenerSocket.MessageReceived += udpMessageReceived;
        await listenerSocket.BindServiceNameAsync(udpPort);
        textOut("Listening for udp broadcast.");
    }


    async void udpMessageReceived(DatagramSocket socket, DatagramSocketMessageReceivedEventArgs args)
    {
        if (!targetIPReady&&!connected&&!trashUDP)
        {
            trashUDP = true;
            DataReader reader = args.GetDataReader();
            uint len = reader.UnconsumedBufferLength;
            string msg = reader.ReadString(len);
            string remoteHost = args.RemoteAddress.DisplayName;
            targetIP = msg;
            targetIPReady = true;
            textOut("" + msg);
            await listenerSocket.CancelIOAsync();
            listenerSocket.MessageReceived -= udpMessageReceived;
            listenerSocket.Dispose();
            listenerSocket = null;//new since working
            


            //socket.Dispose();
            /* //exception smashing test
            reader.Dispose();
           
            await Windows.ApplicationModel.Core.CoreApplication.GetCurrentView().CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                targetIP = msg;
                targetIPReady = true;
                textOut("UDP Set up. "+targetIP+" "+targetIPReady);
            });
            */
        }
    }
#endif



    public void SendTestData()
    {
#if !UNITY_EDITOR
        if (!connected)
        {
            textOut("Not connected");
            return;
        }
        try
        {
            Vector3 location = new Vector3();
            Quaternion rotation = new Quaternion();
            byte[] bytes = new byte[4 + 12 + 20]; // 4 bytes per float
            System.Buffer.BlockCopy(BitConverter.GetBytes(36 + (256 * 4)), 0, bytes, 0, 4);
            System.Buffer.BlockCopy(BitConverter.GetBytes(2), 0, bytes, 4, 4);//type of packet
            System.Buffer.BlockCopy(BitConverter.GetBytes(location.x), 0, bytes, 8, 4);
            System.Buffer.BlockCopy(BitConverter.GetBytes(location.y), 0, bytes, 12, 4);
            System.Buffer.BlockCopy(BitConverter.GetBytes(location.z), 0, bytes, 16, 4);
            System.Buffer.BlockCopy(BitConverter.GetBytes(rotation.x), 0, bytes, 20, 4);
            System.Buffer.BlockCopy(BitConverter.GetBytes(rotation.y), 0, bytes, 24, 4);
            System.Buffer.BlockCopy(BitConverter.GetBytes(rotation.z), 0, bytes, 28, 4);
            System.Buffer.BlockCopy(BitConverter.GetBytes(rotation.w), 0, bytes, 32, 4);

            byte[] testBytes = new byte[256 * 4];

            for(int i = 0; i < 256; i++)
            {
                testBytes[i] = (byte)i;
            }
            bytes = Combine(bytes, testBytes);

            if (bytes.Length > 0)
            {
                enqueueOutgoing(bytes);
                textOut("Outgoing data enqueued.");
            }
        }
        catch (Exception e)
        {
            textOut("248"+ e.ToString());
            Debug.Log(e.ToString());
            return;
        }
#endif
    }


    
    public void textOut(string o)
    {
        currentOutput += "\n" + o;
        if(currentOutput.Length>500)
        {
            currentOutput = currentOutput.Substring(currentOutput.Length - 500);
        }
        
    }
#if !UNITY_EDITOR
    void FixedUpdate()
    {

        if(!socketStarted&&targetIPReady)
        {
            socketStarted = true;
            doSocketSetup();
        }
        if(outputText != null)
        {
            outputText.text = currentOutput;
        }
        if (!outgoingQueue.IsEmpty)
        {
            messagePackage mp = null;
            outgoingQueue.TryDequeue(out mp);
            if (mp != null)
            {
                sendOutgoingPacket(mp);
                textOut("Packet Sent.");
            }
        }

        if(!incomingLineRenderers.IsEmpty)
        {
           
            lrStruct l = new lrStruct();
            if(incomingLineRenderers.TryDequeue(out l))
            {
                GameObject go = new GameObject();
                go.transform.parent = this.gameObject.transform;
                LineRenderer lr = go.gameObject.AddComponent<LineRenderer>();
                lr.material = new Material(LineRendererDefaultMaterial);//copy
                lr.material.color = new Color(l.r, l.g, l.b, l.a);
                lr.startWidth = l.sw;
                lr.endWidth = l.ew;
                lr.endColor = lr.startColor = new Color(l.r, l.g, l.b, l.a);
                /* some helpful notes
                LineRenderer lineRenderer = gameObject.AddComponent<LineRenderer>();
                lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
                lineRenderer.widthMultiplier = 0.2f;
                lineRenderer.positionCount = lengthOfLineRenderer;

                // A simple 2 color gradient with a fixed alpha of 1.0f.
                float alpha = 1.0f;
                Gradient gradient = new Gradient();
                gradient.SetKeys(
                    new GradientColorKey[] { new GradientColorKey(c1, 0.0f), new GradientColorKey(c2, 1.0f) },
                    new GradientAlphaKey[] { new GradientAlphaKey(alpha, 0.0f), new GradientAlphaKey(alpha, 1.0f) }
                );
                lineRenderer.colorGradient = gradient;
                */
            }
        }
        //SendHeadsetLocation();
    }

    private async void flush()
    {
        await writer.StoreAsync();
        //await writer.FlushAsync();
    }

    private async void sendOutgoingPacket(messagePackage sendData)
    {
        try
        {
            if (sendData.bytes.Length > 1000000)
            {
                Debug.Log("Packet of length " + sendData.bytes.Length + " waiting to go out... But can't.. Because it is probably too huge...");
                return;
            }
            lock (outputStream)
            {

                writer.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;
                writer.ByteOrder = Windows.Storage.Streams.ByteOrder.LittleEndian;
                writer.WriteBytes(sendData.bytes);
                flush();
                Debug.Log("Sent " + sendData.bytes.Length + " bytes.");
            }


        }
        catch (Exception e)
        {
           textOut("344" + e.ToString());
           Debug.Log(e.ToString());
           return;
        }
    }


    private void enqueueOutgoing(byte[] bytes)
    {
        outgoingQueue.Enqueue(new messagePackage(bytes));
    }

#endif

    

    //stolen useful code.
    public static byte[] Combine(byte[] first, byte[] second)
    {
        byte[] ret = new byte[first.Length + second.Length];
        System.Buffer.BlockCopy(first, 0, ret, 0, first.Length);
        System.Buffer.BlockCopy(second, 0, ret, first.Length, second.Length);
        return ret;
    }
    public static byte[] Combine(byte[] first, byte[] second, byte[] third)
    {
        byte[] ret = new byte[first.Length + second.Length + third.Length];
        System.Buffer.BlockCopy(first, 0, ret, 0, first.Length);
        System.Buffer.BlockCopy(second, 0, ret, first.Length, second.Length);
        System.Buffer.BlockCopy(third, 0, ret, first.Length + second.Length,
                         third.Length);
        return ret;
    }
    
}
