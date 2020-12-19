using System;
using UnityEngine;
using LiteNetLib;
using LiteNetLib.Utils;
using LiteNetLibExtension;
using VPXVideoCompression.NativePlugin;

namespace VPXStreaming.Client.LiteNetLib
{
    public class TextureReceiverClientLogic : MonoBehaviour, ITextureReceiverClient
    {
        [SerializeField] LiteNetLibClientMain _liteNetLibClient;
        [SerializeField] int _Width, _Height;

        VP8Decoder _Decoder;
        byte[] _EncodedData, _RGBColorData;
        int _FrameCount = 0;
        int _KeyFrameInterval = 30;

        public int FrameCount { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public byte[] RawTextureData { get; private set;  }

        NetDataWriter _dataWriter;
        public int ClientId { get; private set; }

        void Awake()
        {
            ClientId = -1;
            _dataWriter = new NetDataWriter();
            _liteNetLibClient.OnNetworkReceived += OnNetworkReceived;
            // _liteNetLibClient.StartClient();

            _Decoder = new VP8Decoder();
            _EncodedData = new byte[4 * _Width * _Height];
            _RGBColorData = new byte[3 * _Width * _Height];
        }

        public bool StartClient(string address, int port)
        {
            return _liteNetLibClient.StartClient(address, port);
        }

        public void StopClient()
        {
            _liteNetLibClient.StopClient();
            ClientId = -1;
        }

        void OnNetworkReceived(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
        {
            if (reader.UserDataSize >= 4)
            {
                NetworkDataType networkDataType = (NetworkDataType)reader.GetInt();
                if (networkDataType == NetworkDataType.ReceiveOwnCliendId)
                {
                    ClientId = reader.GetInt();
                    Debug.Log("Own Client ID : " + ClientId);
                }
                else if (networkDataType == NetworkDataType.ReceiveTexture)
                {
                    OnReceivedRawTextureData(peer, reader);
                }
            }
        }

        void OnReceivedRawTextureData(NetPeer peer, NetPacketReader reader)
        {
            int frameCount = reader.GetInt();
            int width = reader.GetInt();
            int height = reader.GetInt();

            if (_Width != width || _Height != height)
            {
                _RGBColorData = new byte[3 * width * height];
                _Width = width;
                _Height = height;
            }

            int dataLength = reader.GetInt();
            Array.Resize(ref _EncodedData, dataLength);
            reader.GetBytes(_EncodedData, dataLength);

            _Decoder.Decode(ref _EncodedData, _EncodedData.Length, ref _RGBColorData);

            OnReceivedRawTextureData(frameCount, width, height, _RGBColorData);
        }

        public void OnReceivedRawTextureData(int frameCount, int width, int height, byte[] rawTextureData)
        {
            FrameCount = frameCount;
            Width = width;
            Height = height;
            RawTextureData = rawTextureData;
        }

        public void RegisterTextureReceiver(int streamingClientId)
        {
            _dataWriter.Reset();
            _dataWriter.Put((int)NetworkDataType.RegisterTextureReceiver);
            _dataWriter.Put(streamingClientId);
            _liteNetLibClient.SendData(_dataWriter, DeliveryMethod.ReliableOrdered);
        }

        public void UnregisterTextureReceiver(int streamingClientId)
        {
            _dataWriter.Reset();
            _dataWriter.Put((int)NetworkDataType.UnregisterTextureReceiver);
            _dataWriter.Put(streamingClientId);
            _liteNetLibClient.SendData(_dataWriter, DeliveryMethod.ReliableOrdered);
        }
    }
}
