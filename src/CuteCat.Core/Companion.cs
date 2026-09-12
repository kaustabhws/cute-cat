namespace CuteCat.Core;

public sealed class Companion
{
    public V2 Position { get; private set; }
    public V2 Velocity { get; private set; }
    public int Facing { get; private set; } = 1;
    public CatAction Action { get; private set; } = CatAction.Idle;
    public double ActionAge => Math.Max(0, _now-_started);
    public double Phase { get; private set; }
    public double Scale { get; private set; } = 1;
    public Area WorkArea { get; private set; }
    public bool Hidden { get; private set; }
    public bool Quiet { get; private set; }
    public bool ReducedMotion { get; private set; }
    public bool IsTurning => _turn.Active;
    public bool IsTravelling => _destination is not null;
    public bool Arrived => _destination is null && !_turn.Active;
    public CatPose Pose { get; private set; }
    public int SuggestedFps => Hidden ? 0 : ReducedMotion ? 1 : Action is CatAction.Idle or CatAction.Sleep ? 15 : 60;
    private double _now, _last=-1, _started, _next=5, _blendAt;
    private uint _random=0x71C4A29u;
    public ActivityLevel Activity { get; set; }=ActivityLevel.Balanced;
    public PetAccessory Accessory { get; set; }
    public string AccessoryColor { get; set; }="Sage";
    public bool Angry { get; set; }
    public bool AutonomyPaused { get; set; }
    private double _anger;
    private bool _holdPaw;
    private V2? _destination;
    private CatPose _blendFrom;
    private bool _resumeWalk, _roaming, _holdAtDestination;
    private int? _arrivalFacing;
    private readonly TurnTransition _turn = new();
    private CatAction _afterTurn = CatAction.Idle;
    private bool _urgent;
    private V2 _tripStart, _pawEnd=CatRig.Contact;
    private double _tripElapsed,_tripSeconds,_strideScale=1;
    private double _lookFrom;
    private double _lookTo=1;

    public Companion(Area area, double scale=1)
    {
        WorkArea=area; Scale=scale;
        Position=new(area.Right-180*scale,area.Bottom-18*scale);
        Pose=CatRig.Evaluate(Action,0,0,0); _blendFrom=Pose;
    }

    public void Configure(Area area, double scale, bool quiet, bool reduced, double now)
    {
        if (!area.IsValid || !double.IsFinite(scale) || scale<=0) throw new ArgumentOutOfRangeException(nameof(area));
        if (WorkArea==area && Scale==scale && Quiet==quiet && ReducedMotion==reduced) return;
        bool sleeping=Action==CatAction.Sleep;
        WorkArea=area; Scale=scale; Quiet=quiet; ReducedMotion=reduced;
        Position=Clamp(Position); Stop(now); _next=now+4;
        if(sleeping)Perform(CatAction.Sleep,now);
    }

    public V2 Clamp(V2 p) => WorkArea.Clamp(p,158*Scale,230*Scale,20*Scale);
    public void SetVisible(bool visible, double now) { Hidden=!visible; Stop(now); }
    public void Park(double now) { Position=new(WorkArea.Right-174*Scale,WorkArea.Bottom-20*Scale); Stop(now); _next=now+20; }
    public void Stop(double now)
    {
        CancelTravel(); _resumeWalk=false; SetAction(CatAction.Idle,now); _next=now+5;
    }

    public void Perform(CatAction action, double now)
    {
        CancelTravel();
        _resumeWalk=action is CatAction.Meow or CatAction.Land or CatAction.Wake;
        if (action is CatAction.Walk or CatAction.Run) { Roam(now,action); return; }
        if (action==CatAction.Turn)
        {
            if (!ReducedMotion) BeginTurn(-Facing,CatAction.Idle,now);
            else Stop(now);
            return;
        }
        SetAction(action,now);
        _next=now+(action switch { CatAction.Groom=>5.8,CatAction.Sleep=>double.PositiveInfinity,CatAction.Wake=>1.4,CatAction.Play=>3,
            CatAction.Meow=>1.1,CatAction.Land=>.55,CatAction.Paw=>1.3,CatAction.Celebrate=>1.8,CatAction.Notice=>double.PositiveInfinity,CatAction.Drag=>double.PositiveInfinity,_=>5 });
    }
    public void Notice(int direction,double now)
    {Perform(CatAction.Notice,now);_lookFrom=_turn.Current.Head;int target=direction<0?-1:1;_lookTo=target==Facing?target:Facing*.3;}

    public void MoveTo(V2 position, double now, bool dragging=false)
    {
        Position=Clamp(position); CancelTravel();
        if (dragging && Action!=CatAction.Drag) Perform(CatAction.Drag,now);
    }

    public void Approach(V2 bottomCenter, int facing, double now,double travelSeconds=1.55)
    {
        CancelTravel();
        _urgent=true;_tripStart=Position;_tripElapsed=0;_tripSeconds=Math.Clamp(travelSeconds,.25,2);
        _arrivalFacing=facing<0?-1:1;
        StartTravel(bottomCenter,CatAction.Run,now,false,true);
    }

    public void ReachTo(V2 endpoint,double now)
    {
        if(!NotificationApproach.IsReachable(endpoint))throw new ArgumentOutOfRangeException(nameof(endpoint));
        Perform(CatAction.Paw,now);_pawEnd=endpoint;
    }
    public void HoldPawContact(){if(Action==CatAction.Paw){_holdPaw=true;_next=double.PositiveInfinity;}}

    public void ReturnTo(V2 position,double now)
    {
        CancelTravel(); StartTravel(Clamp(position),CatAction.Run,now,false,false);
    }

    public void Tick(double now)
    {
        if (!double.IsFinite(now) || now < _now) return;
        _now=now;
        double dt=_last<0?0:Math.Clamp(now-_last,0,.05); _last=now;
        if (Hidden) return;
        if (_turn.Active)
        {
            _turn.Advance(now);
            if (!_turn.Active)
            {
                Facing=_turn.Target;
                SetAction(_afterTurn,now);
                if (_afterTurn==CatAction.Idle) _next=now+5;
            }
        }
        if (!_turn.Active && _destination is V2 target)
        {
            V2 delta=target-Position;
            double distance=delta.Length;
            if (distance<1.2*Scale) Arrive(target,now);
            else if(_urgent)
            {
                _tripElapsed=Math.Min(_tripSeconds,_tripElapsed+dt);
                double u=_tripElapsed/_tripSeconds;
                V2 next=V2.Lerp(_tripStart,target,u*u*(3-2*u));
                V2 move=next-Position;Position=next;
                Velocity=dt>0?move*(1/dt):default;AdvanceGait(move.Length,dt);
                if(_tripElapsed>=_tripSeconds)Arrive(target,now);
            }
            else
            {
                double max=(Action==CatAction.Run?340:84)*Scale;
                double speed=Math.Min(max,Math.Sqrt(2*980*Scale*distance));
                Velocity=V2.Lerp(Velocity,delta*(speed/distance),1-Math.Exp(-11*dt));
                V2 move=Velocity*dt;
                if (move.Length>distance) move=delta;
                Position+=move;
                AdvanceGait(move.Length,dt);
            }
        }
        else if (!AutonomyPaused && !_turn.Active && _destination is null && now>=_next && Action!=CatAction.Drag)
        {
            if (_resumeWalk && !Quiet && !ReducedMotion) { _resumeWalk=false; Roam(now,CatAction.Walk); }
            else if (Action!=CatAction.Idle) Stop(now);
            else if (!Quiet && !ReducedMotion)
            {
                _random^=_random<<13;_random^=_random>>17;_random^=_random<<5;
                int pick=(int)(_random%10);
                if(pick<(Activity==ActivityLevel.Calm?4:2))Perform(CatAction.Groom,now);
                else if(pick==4)Perform(CatAction.Meow,now);
                else if(pick==5&&Activity!=ActivityLevel.Calm)Perform(CatAction.Play,now);
                else Roam(now,pick>=(Activity==ActivityLevel.Playful?7:9)?CatAction.Run:CatAction.Walk);
            }
        }
        var targetPose=CatRig.Evaluate(Action,Action==CatAction.Turn?_turn.AnimationAge(now):ActionAge,Phase,now,ReducedMotion,_strideScale) with{PawEnd=_pawEnd};
        if(_holdPaw)targetPose=targetPose with{Reach=1};
        _anger=Ease.Mix(_anger,Angry?1:0,1-Math.Exp(-12*dt));
        if(Action==CatAction.Notice)_turn.Look(Ease.Mix(_lookFrom,_lookTo,Ease.Smooth(ActionAge/.22)));
        var orientation=_turn.Current;
        Pose=CatPose.Blend(_blendFrom,targetPose,Ease.Smooth((now-_blendAt)/.32)) with
        { BodyYaw=orientation.Body,HeadYaw=orientation.Head,TailYaw=orientation.Tail,Anger=_anger,Accessory=Accessory,AccessoryColor=AccessoryColor };
    }

    private void Arrive(V2 target,double now)
    {
        Position=target; Velocity=default; _destination=null;
        if (_arrivalFacing is int facing)
        {
            _arrivalFacing=null;
            BeginTurn(facing,CatAction.Run,now);
            return;
        }
        if (_holdAtDestination) return;
        bool atEdge=Position.X<=WorkArea.Left+166*Scale || Position.X>=WorkArea.Right-166*Scale;
        if (_roaming && atEdge) Roam(now,Action);
        else Stop(now);
    }

    private void Roam(double now, CatAction gait)
    {
        if (ReducedMotion) { Stop(now); return; }
        double left=WorkArea.Left+165*Scale,right=WorkArea.Right-165*Scale;
        if (right<=left) { Stop(now); return; }
        int direction=Facing;
        if ((direction>0 && Position.X>=right-2*Scale) || (direction<0 && Position.X<=left+2*Scale)) direction=-direction;
        double next=Math.Clamp(Position.X+direction*260*Scale,left,right);
        StartTravel(new(next,WorkArea.Bottom-20*Scale),gait,now,true,false);
    }

    private void StartTravel(V2 destination,CatAction gait,double now,bool roaming,bool hold)
    {
        _destination=destination; _roaming=roaming; _holdAtDestination=hold; _resumeWalk=false;
        int direction=Math.Abs(destination.X-Position.X)>.1 ? Math.Sign(destination.X-Position.X) : _arrivalFacing??Facing;
        _next=hold?double.PositiveInfinity:now+12;
        BeginTurn(direction,gait,now);
    }

    private void BeginTurn(int direction,CatAction afterwards,double now)
    {
        _afterTurn=afterwards; Velocity=default;
        if (_turn.Start(direction,now,_urgent?.42:TurnTransition.Duration)) SetAction(CatAction.Turn,now);
        else { Facing=direction; SetAction(afterwards,now); }
    }

    private void CancelTravel()
    {
        _turn.Cancel();
        if (Math.Abs(_turn.Current.Body)>.001) Facing=Math.Sign(_turn.Current.Body);
        _destination=null; Velocity=default; _roaming=false; _holdAtDestination=false; _arrivalFacing=null;
        _urgent=false;_strideScale=1;_holdPaw=false;
    }

    private void AdvanceGait(double distance,double dt)
    {
        if(dt<=0)return;
        double cycle=(Action==CatAction.Run?52:34)/.62*Scale;
        _strideScale=_urgent?Math.Clamp(distance/(cycle*6*dt),1,2):1;
        Phase=(Phase+Math.Min(distance/(cycle*_strideScale),6*dt))%1;
    }

    private void SetAction(CatAction action,double now)
    {
        _blendFrom=Pose; _blendAt=now; Action=action; _started=now;
    }
}
