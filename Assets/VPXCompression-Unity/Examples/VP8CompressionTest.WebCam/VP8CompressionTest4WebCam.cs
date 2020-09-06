// This code is licensed under CC0.
// http://creativecommons.org/publicdomain/zero/1.0/deed.ja
// https://creativecommons.org/publicdomain/zero/1.0/deed.en

using UnityEngine;
using UnityEngine.UI;

namespace VPXVideoCompression.Test
{
    public class VP8CompressionTest4WebCam : MonoBehaviour
    {
        [SerializeField] bool _JpegCompressionTest = false;
        [SerializeField] int _DeviceNumber = 0;
        [SerializeField] Material _UnlitTextureMaterial;
        [SerializeField] GameObject _ColorImageObject;
        [SerializeField] GameObject _DecodedColorImageObject;
        [SerializeField] GameObject _DecodedColorImageObject2;

        [SerializeField] Text _ColorImageSize;
        [SerializeField] Text _CompressedColorImageSize;
        [SerializeField] Text _CompressedColorImageSize2;
        [SerializeField] Text _ProcessingTime;

        Texture2D _ColorImageTexture;
        Texture2D _DecodedColorImageTexture;
        Texture2D _DecodedColorImageTexture2;

        NativePlugin.VP8Encoder _Encoder;
        NativePlugin.VP8Decoder _Decoder;
        byte[] _VP8EncodedData, _RGBColorData;
        int _Width, _Height;
        int _FrameCount = 0;
        int _KeyFrameInterval = 30;

        System.Diagnostics.Stopwatch _Stopwatch = new System.Diagnostics.Stopwatch();

        bool _Initialized = false;
        WebCamTexture _WebCamTexture;

        bool InitializeCamera()
        {
            var devices = WebCamTexture.devices;
            if (_DeviceNumber >= devices.Length)
            {
                return false;
            }

            _WebCamTexture = new WebCamTexture(devices[_DeviceNumber].name);
            _WebCamTexture.Play();

            _Width = _WebCamTexture.width;
            _Height = _WebCamTexture.height;

            return true;
        }

        void Start()
        {
            if (InitializeCamera())
            {
                _ColorImageTexture = new Texture2D(_Width, _Height, TextureFormat.RGBA32, false);
                _DecodedColorImageTexture = new Texture2D(_Width, _Height, TextureFormat.RGB24, false);
                _DecodedColorImageTexture2 = new Texture2D(_Width, _Height, TextureFormat.RGB24, false);

                MeshRenderer colorMeshRenderer = _ColorImageObject.GetComponent<MeshRenderer>();
                colorMeshRenderer.sharedMaterial = new Material(_UnlitTextureMaterial);
                colorMeshRenderer.sharedMaterial.SetTexture("_MainTex", _ColorImageTexture);

                MeshRenderer decodedColorMeshRenderer = _DecodedColorImageObject.GetComponent<MeshRenderer>();
                decodedColorMeshRenderer.sharedMaterial = new Material(_UnlitTextureMaterial);
                decodedColorMeshRenderer.sharedMaterial.SetTexture("_MainTex", _DecodedColorImageTexture);

                MeshRenderer decodedColorMeshRenderer2 = _DecodedColorImageObject2.GetComponent<MeshRenderer>();
                decodedColorMeshRenderer2.sharedMaterial = new Material(_UnlitTextureMaterial);
                decodedColorMeshRenderer2.sharedMaterial.SetTexture("_MainTex", _DecodedColorImageTexture2);

                _Encoder = new NativePlugin.VP8Encoder(_Width, _Height);
                _Decoder = new NativePlugin.VP8Decoder();

                _VP8EncodedData = new byte[4 * _Width * _Height];
                _RGBColorData = new byte[3 * _Width * _Height];

                _Initialized = true;
            }
        }

        void Update()
        {
            if (_Initialized)
            {
                Color32[] pixels = _WebCamTexture.GetPixels32();
                int originalColorImageSize = 4 * pixels.Length;

                _ColorImageTexture.SetPixels32(pixels);
                _ColorImageTexture.Apply();

                byte[] colorFrame = _ColorImageTexture.GetRawTextureData();

                _Stopwatch.Reset();
                _Stopwatch.Start();

                // VP8 Compression
                bool keyFrame = false;
                // _Encoder.EncodeFromBgra(ref colorFrame, _Width, _Height, keyFrame, ref _VP8EncodedData);
                _Encoder.EncodeFromRgba(ref colorFrame, _Width, _Height, keyFrame, ref _VP8EncodedData);

                _Stopwatch.Stop();
                long vp8EncodingTimeMillseconds = _Stopwatch.ElapsedMilliseconds;

                _Stopwatch.Reset();
                _Stopwatch.Start();

                // VP8 Decode
                _Decoder.Decode(ref _VP8EncodedData, _VP8EncodedData.Length, ref _RGBColorData);

                _Stopwatch.Stop();
                long vp8DecodingTimeMillseconds = _Stopwatch.ElapsedMilliseconds;

                // Visualize decoded color image
                _DecodedColorImageTexture.LoadRawTextureData(_RGBColorData);
                _DecodedColorImageTexture.Apply();

                // Display info
                int vp8CompressedDataSize = _VP8EncodedData.Length;
                float vp8CompressionRatio = originalColorImageSize / vp8CompressedDataSize;

                _ColorImageSize.text = string.Format("Size: {2:#,0} [bytes]  Resolution: {0}x{1}",
                                                     _ColorImageTexture.width, _ColorImageTexture.height, originalColorImageSize);
                _CompressedColorImageSize.text = string.Format("Size: {0:#,0} [bytes]  Data compression ratio: {1:F1}",
                                                               vp8CompressedDataSize, vp8CompressionRatio);
                _ProcessingTime.text = string.Format("Processing time:\n Encode: {0} [ms]\n Decode: {1} [ms]",
                                                     vp8EncodingTimeMillseconds, vp8DecodingTimeMillseconds);

                if (_JpegCompressionTest)
                {
                    byte[] jpegEncodedData = ImageConversion.EncodeToJPG(_ColorImageTexture);

                    _DecodedColorImageTexture2.LoadImage(jpegEncodedData);
                    _DecodedColorImageTexture2.Apply();

                    int jpegCompressedDataSize = jpegEncodedData.Length;
                    float jpegCompressionRatio = originalColorImageSize / jpegCompressedDataSize;

                    _CompressedColorImageSize2.text = string.Format("Size: {0:#,0} [bytes]  Data compression ratio: {1:F1}",
                                                                jpegCompressedDataSize, jpegCompressionRatio);
                }
            }
        }
    }
}
