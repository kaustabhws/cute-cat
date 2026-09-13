using CuteCat.Core;

internal static class PersistenceChecks
{
    public static async Task Run(Action<string,bool> check)
    {
        var entered=new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var release=new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        List<string> written=[];int active=0,maxActive=0;
        var writer=new CoalescedStateWriter(async state=>
        {
            maxActive=Math.Max(maxActive,Interlocked.Increment(ref active));
            if(state.Settings.Nickname=="first"){entered.SetResult();await release.Task;}
            written.Add(state.Settings.Nickname);Interlocked.Decrement(ref active);
        });
        writer.Queue(new(){Settings=new(){Nickname="first"}});var flush=writer.FlushAsync();await entered.Task.WaitAsync(TimeSpan.FromSeconds(3));
        for(int i=0;i<100;i++)writer.Queue(new(){Settings=new(){Nickname="edit"+i}});
        check("pending saves coalesce without blocking the caller",written.Count==0&&!flush.IsCompleted);
        release.SetResult();await writer.FlushAsync();
        check("single background writer preserves the newest snapshot",written.SequenceEqual(new[]{"first","edit99"})&&maxActive==1&&writer.Writes==2);
        writer.Queue(new(){Settings=new(){Nickname="shutdown"}});await writer.FlushAsync();
        check("shutdown flush waits for the final state",written.Last()=="shutdown");
        bool fail=true;var recovery=new CoalescedStateWriter(_=>fail?Task.FromException(new IOException("fixture")):Task.CompletedTask);
        recovery.Queue(new());await recovery.FlushAsync();check("save failure is reported without blocking future input",recovery.HasError);
        fail=false;recovery.Queue(new());await recovery.FlushAsync();check("a later save recovers from a transient write failure",!recovery.HasError&&recovery.Writes==1);
        check("startup is the new default and explicit off remains representable",new Preferences().Startup&&StateStore.Normalize(new(){Settings=new(){Startup=false,StartupInitialized=true}}).Settings is{Startup:false,StartupInitialized:true});
        var imported=SettingsTransfer.Review(SettingsTransfer.Export(new Preferences()),new(){Startup=false,StartupInitialized=true});
        check("settings import preserves a recorded startup opt-out",!imported.Startup&&imported.StartupInitialized);
    }
}
