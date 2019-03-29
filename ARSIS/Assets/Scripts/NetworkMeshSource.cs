using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Collections.Concurrent;

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

public class NetworkMeshSource : MonoBehaviour
{

    private static NetworkMeshSource networkMeshSourceSingleton = null;
    public static NetworkMeshSource getSingleton() { return networkMeshSourceSingleton; }

    public int serverPort = 32123;
    public int targetPort = 32123;
    public string targetIP = "192.168.137.1";
    public volatile bool connected = false;
    private ConcurrentQueue<messagePackage> outgoingQueue = null;
#if !UNITY_EDITOR
    //public DatagramSocket udpClient = null;
    public StreamSocket tcpClient = null;
    public Windows.Storage.Streams.IOutputStream outputStream = null;
    DataWriter writer = null;//new DataWriter(outputStream);

#endif

    private UnityEngine.XR.WSA.WebCam.PhotoCapture photoCaptureObject = null;
    private Texture2D targetTexture = null;
    private Vector3 textureLocation = Vector3.zero;
    private Quaternion textureRotation = Quaternion.identity;

    private Vector3 cameraStartPosition = Vector3.zero;
    private Vector3 cameraEndPosition = Vector3.zero;
    private Quaternion cameraStartRotation = Quaternion.identity;
    private Quaternion cameraEndRotation = Quaternion.identity;



    private class messagePackage
    {
        public byte[] bytes = null;
        public messagePackage(byte[] b) { bytes = b; }
    }


    //public Mesh testMesh = null;
    // Start is called before the first frame update
    void Start()
    {
        if (networkMeshSourceSingleton != null)
        {
            Destroy(this);
            return;
        }
        outgoingQueue = new ConcurrentQueue<messagePackage>();
        networkMeshSourceSingleton = this;
        setupSocket();
    }

    public async void setupSocket()
    {

#if !UNITY_EDITOR
        //udpClient = new DatagramSocket();
        //udpClient.Control.DontFragment = true;
        tcpClient = new Windows.Networking.Sockets.StreamSocket();
        tcpClient.Control.OutboundBufferSizeInBytes = 1500;
        tcpClient.Control.NoDelay = false;
        tcpClient.Control.KeepAlive = false;
        tcpClient.Control.OutboundBufferSizeInBytes = 1500;
        while(!connected)
        {
            try
            {
                //await udpClient.BindServiceNameAsync("" + targetPort);
                await tcpClient.ConnectAsync(new HostName(targetIP), "" + targetPort);
            
                outputStream = tcpClient.OutputStream;
                writer = new DataWriter(outputStream);
                connected = true;
                //outputStream = await udpClient.GetOutputStreamAsync(new HostName(targetIP), "" + targetPort);
            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
                return;
            }
        }
#endif
    }
    /*
#if !UNITY_EDITOR
    public void captureImageData()
    {

        Resolution cameraResolution = UnityEngine.XR.WSA.WebCam.PhotoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).First();
        targetTexture = new Texture2D(cameraResolution.width, cameraResolution.height);

        // Create a PhotoCapture object
        UnityEngine.XR.WSA.WebCam.PhotoCapture.CreateAsync(false, delegate (UnityEngine.XR.WSA.WebCam.PhotoCapture captureObject) {
            photoCaptureObject = captureObject;
            UnityEngine.XR.WSA.WebCam.CameraParameters cameraParameters = new UnityEngine.XR.WSA.WebCam.CameraParameters();
            cameraParameters.hologramOpacity = 0.0f;
            cameraParameters.cameraResolutionWidth = cameraResolution.width;
            cameraParameters.cameraResolutionHeight = cameraResolution.height;
            cameraParameters.pixelFormat = UnityEngine.XR.WSA.WebCam.CapturePixelFormat.BGRA32;


            cameraStartPosition = Camera.main.transform.position;
            cameraStartRotation = Camera.main.transform.rotation;

            // Activate the camera
            photoCaptureObject.StartPhotoModeAsync(cameraParameters, delegate (UnityEngine.XR.WSA.WebCam.PhotoCapture.PhotoCaptureResult result) {
            // Take a picture
            photoCaptureObject.TakePhotoAsync(OnCapturedPhotoToMemory);

            });
        });
    }

    void OnCapturedPhotoToMemory(UnityEngine.XR.WSA.WebCam.PhotoCapture.PhotoCaptureResult result, UnityEngine.XR.WSA.WebCam.PhotoCaptureFrame photoCaptureFrame)
    {
        // Copy the raw image data into the target texture
        photoCaptureFrame.UploadImageDataToTexture(targetTexture);
        photoCaptureObject.StopPhotoModeAsync(OnStoppedPhotoMode);
        cameraEndPosition = Camera.main.transform.position;
        cameraEndRotation = Camera.main.transform.rotation;
    }

    void OnStoppedPhotoMode(UnityEngine.XR.WSA.WebCam.PhotoCapture.PhotoCaptureResult result)
    {
        // Shutdown the photo capture resource
        photoCaptureObject.Dispose();
        photoCaptureObject = null;
    }
#endif*/
#if !UNITY_EDITOR
    public async void sendImage(Texture2D tex, Vector3 location, Quaternion rotation)
    {

        if (!connected)
        return;

        try
        {
        byte[] image = ImageConversion.EncodeToJPG(tex, 50);

        byte[] bytes = new byte[36]; // 4 bytes per float
        System.Buffer.BlockCopy(BitConverter.GetBytes(36 + image.Length), 0, bytes, 0, 4);
        System.Buffer.BlockCopy(BitConverter.GetBytes(3), 0, bytes, 4, 4);//type of packet
        System.Buffer.BlockCopy(BitConverter.GetBytes(location.x), 0, bytes, 8, 4);
        System.Buffer.BlockCopy(BitConverter.GetBytes(location.y), 0, bytes, 12, 4);
        System.Buffer.BlockCopy(BitConverter.GetBytes(location.z), 0, bytes, 16, 4);
        System.Buffer.BlockCopy(BitConverter.GetBytes(rotation.x), 0, bytes, 20, 4);
        System.Buffer.BlockCopy(BitConverter.GetBytes(rotation.y), 0, bytes, 24, 4);
        System.Buffer.BlockCopy(BitConverter.GetBytes(rotation.z), 0, bytes, 28, 4);
        System.Buffer.BlockCopy(BitConverter.GetBytes(rotation.w), 0, bytes, 32, 4);
        byte[] sendData = Combine(bytes, image);
        if(sendData.Length>0)
            enqueueOutgoing(sendData);

        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
            return;
        }
    }


    public async void sendMesh(Mesh m, Vector3 location, Quaternion rotation)
    {
        if (!connected)
            return;

        
        try
        {
            SendHeadsetLocation();
            List<Mesh> meshes = new List<Mesh>();
            meshes.Add(m);
            byte[] meshData =  SimpleMeshSerializer.Serialize(meshes);
            byte[] bytes = new byte[36]; // 4 bytes per float
            System.Buffer.BlockCopy(BitConverter.GetBytes(36 + meshData.Length), 0, bytes, 0, 4);
            System.Buffer.BlockCopy(BitConverter.GetBytes(1), 0, bytes, 4, 4);//type of packet
            System.Buffer.BlockCopy(BitConverter.GetBytes(location.x), 0, bytes, 8, 4);
            System.Buffer.BlockCopy(BitConverter.GetBytes(location.y), 0, bytes, 12, 4);
            System.Buffer.BlockCopy(BitConverter.GetBytes(location.z), 0, bytes, 16, 4);
            System.Buffer.BlockCopy(BitConverter.GetBytes(rotation.x), 0, bytes, 20, 4);
            System.Buffer.BlockCopy(BitConverter.GetBytes(rotation.y), 0, bytes, 24, 4);
            System.Buffer.BlockCopy(BitConverter.GetBytes(rotation.z), 0, bytes, 28, 4);
            System.Buffer.BlockCopy(BitConverter.GetBytes(rotation.w), 0, bytes, 32, 4);
            byte[] sendData = Combine(bytes, meshData);
            if(sendData.Length>0)
                enqueueOutgoing(sendData);
            SendHeadsetLocation();
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
            return;
        }
        //Debug.Log("Sent: " + sendData.Length + " bytes");

}
#endif
#if !UNITY_EDITOR
    public static byte[] Compress(byte[] raw)
    {
        using (MemoryStream memory = new MemoryStream())
        {
            using (GZipStream gzip = new GZipStream(memory, CompressionMode.Compress, true))
            {
                gzip.Write(raw, 0, raw.Length);
            }
            return memory.ToArray();
        }
    }

    static byte[] Decompress(byte[] gzip)
    {
        using (GZipStream stream = new GZipStream(new MemoryStream(gzip), CompressionMode.Decompress))
        {
            const int size = 4096;
            byte[] buffer = new byte[size];
            using (MemoryStream memory = new MemoryStream())
            {
                int count = 0;
                do
                {
                    count = stream.Read(buffer, 0, size);
                    if (count > 0)
                    {
                        memory.Write(buffer, 0, count);
                    }
                }
                while (count > 0);
                return memory.ToArray();
            }
        }
    }
#endif
    // Update is called once per frame
    void Update()
    {

    }




    public async void SendHeadsetLocation()
    {
#if !UNITY_EDITOR
        if (!connected)
            return;
        try
        {
            Vector3 location = new Vector3();
            Quaternion rotation = new Quaternion();
            byte[] bytes = new byte[36]; // 4 bytes per float
            System.Buffer.BlockCopy(BitConverter.GetBytes(36), 0, bytes, 0, 4);
            System.Buffer.BlockCopy(BitConverter.GetBytes(2), 0, bytes, 4, 4);//type of packet
            System.Buffer.BlockCopy(BitConverter.GetBytes(location.x), 0, bytes, 8, 4);
            System.Buffer.BlockCopy(BitConverter.GetBytes(location.y), 0, bytes, 12, 4);
            System.Buffer.BlockCopy(BitConverter.GetBytes(location.z), 0, bytes, 16, 4);
            System.Buffer.BlockCopy(BitConverter.GetBytes(rotation.x), 0, bytes, 20, 4);
            System.Buffer.BlockCopy(BitConverter.GetBytes(rotation.y), 0, bytes, 24, 4);
            System.Buffer.BlockCopy(BitConverter.GetBytes(rotation.z), 0, bytes, 28, 4);
            System.Buffer.BlockCopy(BitConverter.GetBytes(rotation.w), 0, bytes, 32, 4);
            if(bytes.Length>0)
                enqueueOutgoing(bytes);
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
            return;
        }
#endif
    }




#if !UNITY_EDITOR
    void FixedUpdate()
    {
        if(!outgoingQueue.IsEmpty)
        {
            messagePackage mp = null;
            outgoingQueue.TryDequeue(out mp);
            if(mp!=null)
            {
                sendOutgoingPacket(mp);
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
        try{
        if (sendData.bytes.Length>1000000)
            {
                Debug.Log("Packet of length " + sendData.bytes.Length + " waiting to go out... But can't.. Because it is probably too huge...");
                return;
            }
            lock(outputStream)
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
            Debug.Log(e.ToString());
            return;
        }
    }


    private void enqueueOutgoing(byte[] bytes)
    {
        outgoingQueue.Enqueue(new messagePackage(bytes));
    }

#endif


    void OnDestroy()
    {
#if !UNITY_EDITOR
        //if (tcpClient != null)
        //{
            //tcpClient.Close();
       //     tcpClient = null;
        //}

        //if (udpClient != null)
        //{
            //udpClient.Close();
        //    udpClient = null;
        //}
#endif
    }

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
