using CuteCat.Core;

namespace CuteCat.App;

// Ephemeral measurements, exported only by explicit diagnostic/test runs. No notification text.
public sealed class PawJourneyMetrics
{
    public bool Practice { get; set; }
    public V2 Start { get; set; }
    public V2 Destination { get; set; }
    public double Started { get; set; }
    public double FirstMovement { get; set; }
    public double Arrival { get; set; }
    public double Contact { get; set; }
    public double Finished { get; set; }
    public double ContactError { get; set; }
    public double MaxFrameStep { get; set; }
    public int Frames { get; set; }
    public bool Dismissed { get; set; }
    public bool Cancelled { get; set; }
    internal V2 Previous { get; set; }
}
