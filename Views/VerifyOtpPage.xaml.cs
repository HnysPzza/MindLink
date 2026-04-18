using M1ndLink.ViewModels;

namespace M1ndLink.Views;

public partial class VerifyOtpPage : ContentPage
{
    private readonly VerifyOtpViewModel _viewModel;

    // Track all box/entry pairs for easy iteration
    private Border[] _boxes = null!;
    private Entry[] _entries = null!;

    public VerifyOtpPage(VerifyOtpViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        _boxes = [OtpBox1, OtpBox2, OtpBox3, OtpBox4, OtpBox5, OtpBox6];
        _entries = [OtpEntry1, OtpEntry2, OtpEntry3, OtpEntry4, OtpEntry5, OtpEntry6];

        // Set initial opacity to 0 for animation
        IconBorder.Opacity = 0;
        IconBorder.TranslationY = -20;
        TitleLabel.Opacity = 0;
        SubtitleLabel.Opacity = 0;
        EmailLabel.Opacity = 0;
        OtpRow.Opacity = 0;
        VerifyButton.Opacity = 0;

        await Task.Delay(80);

        // Icon drops in from top
        await Task.WhenAll(
            IconBorder.FadeTo(1, 350, Easing.CubicOut),
            IconBorder.TranslateTo(0, 0, 350, Easing.CubicOut)
        );

        // Text fades in staggered
        await TitleLabel.FadeTo(1, 280, Easing.CubicOut);
        await Task.WhenAll(
            SubtitleLabel.FadeTo(1, 220, Easing.CubicOut),
            EmailLabel.FadeTo(1, 220, Easing.CubicOut)
        );

        // OTP boxes slide up slightly + fade in
        OtpRow.TranslationY = 12;
        await Task.WhenAll(
            OtpRow.FadeTo(1, 300, Easing.CubicOut),
            OtpRow.TranslateTo(0, 0, 300, Easing.CubicOut)
        );

        // Button fades in dimmed (disabled until all 6 filled)
        await VerifyButton.FadeTo(0.6, 250, Easing.CubicOut);

        // Auto-focus first box to show keyboard immediately
        await Task.Delay(150);
        OtpEntry1.Focus();
    }

    // -----------------------------------------------------------------------
    //  OTP Digit Handling
    // -----------------------------------------------------------------------

    private void OnOtpDigitChanged(object? sender, TextChangedEventArgs e)
    {
        var entry = (Entry)sender;
        int index = int.Parse(entry.ClassId) - 1; // 0-based

        // Guard: block multi-char paste
        if (e.NewTextValue.Length > 1)
        {
            entry.Text = e.NewTextValue[^1].ToString();
            return;
        }

        if (e.NewTextValue.Length == 1)
        {
            // Apply "filled" visual state to this box
            SetBoxState(_boxes[index], BoxState.Filled);

            // Auto-advance to next box
            if (index < 5)
                _entries[index + 1].Focus();
            else
                entry.Unfocus();
        }
        else
        {
            // Digit cleared — reset box to default
            SetBoxState(_boxes[index], BoxState.Default);
        }

        // Hide error label when user re-types anything
        ErrorLabel.IsVisible = false;

        CheckAllFilled();
    }

    private void CheckAllFilled()
    {
        bool allFilled = _entries.All(e => e.Text?.Length == 1);
        VerifyButton.IsEnabled = allFilled;
        VerifyButton.Opacity = allFilled ? 1.0 : 0.6;
    }

    // -----------------------------------------------------------------------
    //  Verify Button
    // -----------------------------------------------------------------------

    private async void OnVerifyClicked(object? sender, EventArgs e)
    {
        string otp = string.Concat(_entries.Select(e => e.Text ?? ""));

        if (otp.Length < 6)
            return;

        // Push OTP into the ViewModel's property so the command uses it
        _viewModel.OtpCode = otp;

        VerifyButton.IsEnabled = false;
        VerifyButton.Text = "Verifying...";

        // Directly await on the UI thread — avoids Android Looper crash from Task.Run
        bool success = await _viewModel.VerifyCodeAsync();

        if (!success)
        {
            VerifyButton.Text = "Verify Email";
            VerifyButton.IsEnabled = true;
            SetAllBoxesState(BoxState.Error);
            ErrorLabel.IsVisible = true;
        }
    }

    // -----------------------------------------------------------------------
    //  Resend Code
    // -----------------------------------------------------------------------

    private async void OnResendTapped(object? sender, EventArgs e)
    {
        ResendLabel.TextColor = Color.FromArgb("#7A92A8");
        ResendLabel.Text = "Sending…";

        // Clear all boxes and reset state
        foreach (var entry in _entries)
            entry.Text = string.Empty;
        SetAllBoxesState(BoxState.Default);
        ErrorLabel.IsVisible = false;
        CheckAllFilled();

        // TODO: wire up Supabase Resend OTP when the SDK method is confirmed
        await Task.Delay(1500);

        ResendLabel.Text = "Resend";
        ResendLabel.TextColor = Color.FromArgb("#4A90D9");

        OtpEntry1.Focus();
    }

    // -----------------------------------------------------------------------
    //  Visual State Helpers
    // -----------------------------------------------------------------------

    private enum BoxState { Default, Filled, Error, Success }

    private static void SetBoxState(Border box, BoxState state)
    {
        switch (state)
        {
            case BoxState.Default:
                box.Stroke = Color.FromArgb("#D0E4F5");
                box.StrokeThickness = 1.5;
                box.BackgroundColor = Colors.White;
                break;
            case BoxState.Filled:
                box.Stroke = Color.FromArgb("#6BB8C4");
                box.StrokeThickness = 2;
                box.BackgroundColor = Color.FromArgb("#F0FAFA");
                break;
            case BoxState.Error:
                box.Stroke = Color.FromArgb("#E05C5C");
                box.StrokeThickness = 2;
                box.BackgroundColor = Color.FromArgb("#FFF5F5");
                break;
            case BoxState.Success:
                box.Stroke = Color.FromArgb("#4CAF88");
                box.StrokeThickness = 2;
                box.BackgroundColor = Color.FromArgb("#F0FFF7");
                break;
        }
    }

    private void SetAllBoxesState(BoxState state)
    {
        foreach (var box in _boxes)
            SetBoxState(box, state);
    }
}
