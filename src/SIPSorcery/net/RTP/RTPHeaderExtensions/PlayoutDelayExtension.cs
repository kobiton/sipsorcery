using System;

namespace SIPSorcery.Net
{
    // Code reference: http://www.webrtc.org/experiments/rtp-hdrext/playout-delay
    //    0                   1                   2                   3
    //    0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
    //   +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    //   |  ID   | len=2 |       MIN delay       |       MAX delay       |
    //   +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    public class PlayoutDelayExtension : RTPHeaderExtension
    {
        public class PlayoutDelay
        {
            public int MinMilliseconds;
            public int MaxMilliseconds;
        }

        public const string RTP_HEADER_EXTENSION_URI = "http://www.webrtc.org/experiments/rtp-hdrext/playout-delay";
        internal const int RTP_HEADER_EXTENSION_SIZE = 3;

        private const int GRANULARITY_MILLISECONDS = 10;
        private const int MAX_DELAY_MILLISECONDS = 0xFFF * GRANULARITY_MILLISECONDS;

        private PlayoutDelay _playoutDelay;

        // Defaults to a minimum and maximum of zero, which asks the receiver to render each frame as soon as it is decoded.
        public PlayoutDelayExtension(int id) : base(id, RTP_HEADER_EXTENSION_URI, RTP_HEADER_EXTENSION_SIZE, RTPHeaderExtensionType.OneByte, Net.SDPMediaTypesEnum.video)
        {
            _playoutDelay = new PlayoutDelay();
        }

        public override void Set(Object value)
        {
            if (value is PlayoutDelay playoutDelay)
            {
                _playoutDelay = playoutDelay;
            }
        }

        public override byte[] Marshal()
        {
            var min = Units(_playoutDelay.MinMilliseconds);
            var max = Units(_playoutDelay.MaxMilliseconds);

            return new[]
            {
                (byte)((Id << 4) | ExtensionSize - 1),
                (byte)(min >> 4),
                (byte)(((min & 0x0F) << 4) | (max >> 8)),
                (byte)(max & 0xFF)
            };
        }

        public override Object Unmarshal(RTPHeader header, byte[] data)
        {
            if (data?.Length == ExtensionSize)
            {
                _playoutDelay = new PlayoutDelay
                {
                    MinMilliseconds = ((data[0] << 4) | (data[1] >> 4)) * GRANULARITY_MILLISECONDS,
                    MaxMilliseconds = (((data[1] & 0x0F) << 8) | data[2]) * GRANULARITY_MILLISECONDS
                };
            }
            return _playoutDelay;
        }

        private static int Units(int milliseconds)
        {
            return Math.Min(Math.Max(milliseconds, 0), MAX_DELAY_MILLISECONDS) / GRANULARITY_MILLISECONDS;
        }
    }
}
