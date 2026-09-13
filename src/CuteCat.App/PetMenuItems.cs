using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Controls.Primitives;
using System.Windows.Controls;
using System.Windows.Input;

namespace CuteCat.App;

// MenuBase preserves context-style item roles in a normal Window. A Menu would
// turn the same entries into menu-bar items with different submenu behavior.
public sealed class PetMenuItems : MenuBase
{
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if(e.Handled||!ReferenceEquals(Keyboard.FocusedElement,this)||e.Key is not (Key.Down or Key.Up))return;
        var items=Items.OfType<MenuItem>().Where(i=>i.IsEnabled);
        var next=e.Key==Key.Down?items.FirstOrDefault():items.LastOrDefault();
        if(next is not null){next.Focus();e.Handled=true;}
    }
    protected override AutomationPeer OnCreateAutomationPeer()=>new PetMenuPeer(this);
    private sealed class PetMenuPeer(PetMenuItems owner):FrameworkElementAutomationPeer(owner)
    {
        protected override AutomationControlType GetAutomationControlTypeCore()=>AutomationControlType.Menu;
        protected override string GetClassNameCore()=>nameof(PetMenuItems);
    }
}
