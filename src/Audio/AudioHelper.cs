using System;
using System.Runtime.InteropServices;
using Frostbyte.Audio.Codecs;

namespace Frostbyte.Audio
{
    public struct AudioHelper
    {
        public const int SAMPLE_RATE
            = 48000;

        public const int STEREO_CHANNEL
            = 2;

        public const int MAX_FRAME_SIZE
            = 120 * (SAMPLE_RATE / 1000);

        public const int MAX_SILENCE_FRAMES
            = 10;
        
        public static ReadOnlyMemory<byte> SilenceFrames
            = new byte[] {0xF8, 0xFF, 0xFE};

        public static int GetSampleSize(int duration)
        {
            return duration * STEREO_CHANNEL * (SAMPLE_RATE / 1000) * 2;
        }

        public static int GetSampleDuration(int size)
        {
            return size / (SAMPLE_RATE / 1000) / (STEREO_CHANNEL / 2);
        }

        public static int GetFrameSize(int duration)
        {
            return duration * (SAMPLE_RATE / 1000);
        }

        public static int GetRtpPacketSize(int value)
        {
            return RtpCodec.HEADER_SIZE + value;
        }

        public static void ZeroFill(Span<byte> buffer)
        {
            var zero = 0;
            var i = 0;

            for (; i < buffer.Length / 4; i++)
                MemoryMarshal.Write(buffer, ref zero);

            var remainder = buffer.Length % 4;
            if (remainder == 0)
                return;

            for (; i < buffer.Length; i++)
                buffer[i] = 0;
        }
    }
}