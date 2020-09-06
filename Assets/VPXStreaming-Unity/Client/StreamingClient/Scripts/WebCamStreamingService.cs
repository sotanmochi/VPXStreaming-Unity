using System;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using VPXVideoCompression.NativePlugin;

namespace VPXStreaming.Client
{
    public class WebCamStreamingService : MonoBehaviour
    {
        ITextureStreamingClient _textureStreamingCLient;
        WebCamTexture _webCamTexture;
        Texture2D _texture2D;
        float _intervalTimeMillisec;
        bool _initialized = false;
        IDisposable _disposable;
    
        int _frameCount;

        VP8Encoder _Encoder;
        byte[] _VP8EncodedData;
        int _Width, _Height;

        public void Initialize(WebCamTexture webCamTexture, ITextureStreamingClient textureStreamingClient, float intervalTimeMillisec = 100)
        {
            _webCamTexture = webCamTexture;
            _textureStreamingCLient = textureStreamingClient;
            _intervalTimeMillisec = intervalTimeMillisec;
            _texture2D = new Texture2D(_webCamTexture.width, _webCamTexture.height);

            _Width = _webCamTexture.width;
            _Height = _webCamTexture.height;
            _Encoder = new VP8Encoder(_Width, _Height);
            _VP8EncodedData = new byte[4 * _Width * _Height];

            _initialized = true;
        }

        public void StartStreaming()
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            if (_initialized)
            {
                Debug.Log("***** StartStream *****");
                Debug.Log(" Interval time: " + _intervalTimeMillisec + "[ms]");
                StopStreaming();
                _disposable = this.UpdateAsObservable()
                                .ThrottleFirst(TimeSpan.FromMilliseconds(_intervalTimeMillisec))
                                .Subscribe(_ => 
                                {
                                    _texture2D.SetPixels32(_webCamTexture.GetPixels32());

                                    // sw.Start();

                                    byte[] colorFrame = _texture2D.GetRawTextureData();
                                    _Encoder.EncodeFromRgba(ref colorFrame, _Width, _Height, false, ref _VP8EncodedData);
                                    _textureStreamingCLient.BroadcastRawTextureData(_VP8EncodedData, _Width, _Height, ++_frameCount);

                                    // sw.Stop();
                                    // Debug.Log("FrameCount: " + _frameCount + ", Processing time: " + sw.ElapsedMilliseconds + "[ms]");
                                    // sw.Reset();
                                });
            }
            else
            {
                Debug.LogError("WebCamStreamer has not been initialized.");
            }
        }

        public void StopStreaming()
        {
            if (_disposable != null)
            {
                _disposable.Dispose();
                Debug.Log("***** StopStream *****");
            }
        }
    }
}
