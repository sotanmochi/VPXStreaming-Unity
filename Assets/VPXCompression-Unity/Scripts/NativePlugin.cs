// Copyright (c) 2020 Soichiro Sugimoto
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Runtime.InteropServices;

namespace VPXVideoCompression.NativePlugin
{
    public class Plugin
    {
        [DllImport("VPXCompressionNativePlugin")]
        public static extern IntPtr create_vp8_encoder(int width, int height);
        [DllImport("VPXCompressionNativePlugin")]
        public static extern void delete_vp8_encoder(IntPtr encoderPtr);
        [DllImport("VPXCompressionNativePlugin")]
        public static extern IntPtr create_vp8_decoder();
        [DllImport("VPXCompressionNativePlugin")]
        public static extern void delete_vp8_decoder(IntPtr decoderPtr);
        [DllImport("VPXCompressionNativePlugin")]
        public static extern int vp8_encoder_encode_from_bgra(IntPtr encoderPtr, byte[] bgraFrame, int width, int height, bool keyFrame, byte[] vp8FrameOutput);
        [DllImport("VPXCompressionNativePlugin")]
        public static extern int vp8_encoder_encode_from_rgba(IntPtr encoderPtr, byte[] bgraFrame, int width, int height, bool keyFrame, byte[] vp8FrameOutput);
        [DllImport("VPXCompressionNativePlugin")]
        public static extern int vp8_decoder_decode(IntPtr decoderPtr, byte[] vp8Frame, int frameDataSize, byte[] rgbFrameOutput);
    }

    public class VP8Encoder
    {
        private IntPtr _Ptr;

        public VP8Encoder(int width, int height)
        {
            _Ptr = Plugin.create_vp8_encoder(width, height);
        }

        ~VP8Encoder()
        {
            Plugin.delete_vp8_encoder(_Ptr);
        }

        public int EncodeFromBgra(ref byte[] bgraFrame, int width, int height, bool keyframe, ref byte[] output)
        {
            Array.Resize(ref output, width * height);
            int size = Plugin.vp8_encoder_encode_from_bgra(_Ptr, bgraFrame, width, height, keyframe, output);
            Array.Resize(ref output, size);
            return size;
        }

        public int EncodeFromRgba(ref byte[] rgbaFrame, int width, int height, bool keyframe, ref byte[] output)
        {
            Array.Resize(ref output, width * height);
            int size = Plugin.vp8_encoder_encode_from_rgba(_Ptr, rgbaFrame, width, height, keyframe, output);
            Array.Resize(ref output, size);
            return size;
        }
    }

    public class VP8Decoder
    {
        private IntPtr _Ptr;

        public VP8Decoder()
        {
            _Ptr = Plugin.create_vp8_decoder();
        }

        ~VP8Decoder()
        {
            Plugin.delete_vp8_decoder(_Ptr);
        }

        public int Decode(ref byte[] vp8Frame, int frameDataSize, ref byte[] rgbOutput)
        {
            int size = Plugin.vp8_decoder_decode(_Ptr, vp8Frame, frameDataSize, rgbOutput);
            return size;
        }
    }
}
