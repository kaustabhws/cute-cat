using System.Media;

namespace CuteCat.App;

public static class MeowSound
{
    // A very quiet, original synthesized chirp. No audio file or network dependency.
    private static readonly byte[] Wave=Create();
    private static SoundPlayer? _player;
    private static MemoryStream? _stream;
    public static void Play()
    {
        _player?.Stop();_player?.Dispose();_stream?.Dispose();
        _stream=new(Wave,false);_player=new(_stream);_player.Play();
    }
    private static byte[] Create()
    {
        const int rate=22050,count=15435;
        using var stream=new MemoryStream();using var writer=new BinaryWriter(stream);
        writer.Write("RIFF"u8);writer.Write(36+count*2);writer.Write("WAVEfmt "u8);writer.Write(16);writer.Write((short)1);writer.Write((short)1);
        writer.Write(rate);writer.Write(rate*2);writer.Write((short)2);writer.Write((short)16);writer.Write("data"u8);writer.Write(count*2);
        double phase=0;
        for(int i=0;i<count;i++)
        {
            double t=i/(double)count;double frequency=530+210*Math.Sin(Math.PI*t)-230*t;
            phase+=2*Math.PI*frequency/rate;double envelope=Math.Pow(Math.Sin(Math.PI*t),2);
            writer.Write((short)(1600*envelope*(Math.Sin(phase)+.21*Math.Sin(phase*2))));
        }
        return stream.ToArray();
    }
}
