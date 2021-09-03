namespace PicoGraffiti.Framework
{
    class WaveFileHeader
    {
        public uint riff_ckid = 0x46464952;    // "RIFF"
        public uint riff_cksize;               // これ以下のバイト数 (= ファイルサイズ - 8)
        public uint fccType = 0x45564157;      // "WAVE"
        public uint fmt_ckid = 0x20746d66;     // "fmt "
        public uint fmt_cksize = 16;           // WAVEfmt欄のバイト数
        public ushort wFormatTag = 0x0001;     // WAVE_FORMAT_PCM
        public ushort nChannels;               // チャネル数 (モノ: 01 00 ステレオ: 02 00)
        public uint nSamplesPerSec;            // サンプリングレート (44100Hz なら 44 AC 00 00)
        public uint nAvgBytesPerSec;           // バイト／秒 (44100Hz ステレオ 16ビット なら 10 B1 02 00)
        public ushort nBlockAlign;             // バイト／サンプル×チャネル数 (ステレオ 16ビット なら 04 00)
        public ushort wBitsPerSample;          // ビット／サンプル (16ビット なら 10 00)
        public uint data_ckid = 0x61746164;    // "data"
        public uint data_cksize;               // 4バイト この欄のバイト数 n
    }
}