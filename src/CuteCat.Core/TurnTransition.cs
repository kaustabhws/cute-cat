namespace CuteCat.Core;

public readonly record struct CatOrientation(double Body, double Head, double Tail);

/// <summary>A grounded turn, through the front view, with a leading look and trailing tail.</summary>
public sealed class TurnTransition
{
    public const double Duration = .86;
    public CatOrientation Current { get; private set; } = new(1, 1, 1);
    public bool Active { get; private set; }
    public int Target { get; private set; } = 1;
    private CatOrientation _from;
    private double _started, _duration=Duration;

    public bool Start(int direction, double now,double duration=Duration)
    {
        direction = direction < 0 ? -1 : 1;
        if (!Active && Math.Abs(Current.Body-direction)<.001 && Math.Abs(Current.Head-direction)<.001 && Math.Abs(Current.Tail-direction)<.001)
            return false;
        _from = Current;
        Target = direction;
        _started = now;
        _duration=Math.Clamp(duration,.3,2);
        Active = true;
        return true;
    }

    public void Advance(double now)
    {
        if (!Active || now<_started) return;
        double age = AnimationAge(now);
        Current = new(
            Ease.Mix(_from.Body,Target,Ease.Smooth((age-.10)/.65)),
            Ease.Mix(_from.Head,Target,Ease.Smooth(age/.55)),
            Ease.Mix(_from.Tail,Target,Ease.Smooth((age-.14)/.72)));
        if (age>=Duration)
        {
            Current = new(Target,Target,Target);
            Active = false;
        }
    }

    // Interrupted turns keep their actual orientation. No hidden mirror or delayed completion.
    public void Cancel() => Active = false;
    public void Look(double head)=>Current=Current with{Head=Math.Clamp(head,-1,1)};
    public double AnimationAge(double now)=>Math.Max(0,now-_started)*Duration/_duration;
}
