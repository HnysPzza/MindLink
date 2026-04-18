using Android.Content.Res;
using Google.Android.Material.BottomNavigation;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Platform.Compatibility;

namespace M1ndLink.Platforms.Android;

public class MindFlowShellRenderer : ShellRenderer
{
    protected override IShellBottomNavViewAppearanceTracker CreateBottomNavViewAppearanceTracker(ShellItem shellItem)
    {
        return new MindFlowBottomNavAppearanceTracker(this, shellItem);
    }
}

public class MindFlowBottomNavAppearanceTracker : ShellBottomNavViewAppearanceTracker
{
    public MindFlowBottomNavAppearanceTracker(IShellContext shellContext, ShellItem shellItem) : base(shellContext, shellItem)
    {
    }

    public override void SetAppearance(BottomNavigationView bottomView, IShellAppearanceElement appearance)
    {
        base.SetAppearance(bottomView, appearance);

        var states = new int[][]
        {
            new int[] { global::Android.Resource.Attribute.StateChecked },
            new int[] { -global::Android.Resource.Attribute.StateChecked }
        };
        var colors = new int[]
        {
            global::Android.Graphics.Color.ParseColor("#2563EB"), // selected
            global::Android.Graphics.Color.ParseColor("#93C5FD")  // unselected
        };
        bottomView.ItemIconTintList = new ColorStateList(states, colors);
        bottomView.ItemTextColor    = new ColorStateList(states, colors);
    }
}
