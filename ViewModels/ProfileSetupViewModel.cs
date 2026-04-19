using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using M1ndLink.Models;
using M1ndLink.Services;
using M1ndLink.Views;

namespace M1ndLink.ViewModels;

public partial class ProfileSetupViewModel : BaseViewModel
{
    private readonly IProfileService _profileService;
    private readonly IDatabaseService _db;
    private readonly IAuthService _authService;
    private readonly IServiceProvider _serviceProvider;

    [ObservableProperty] private string _fullName = string.Empty;
    [ObservableProperty] private string _bio = string.Empty;
    [ObservableProperty] private string? _avatarPath = null;
    [ObservableProperty] private ImageSource? _avatarSource = null;
    [ObservableProperty] private bool _hasAvatar = false;

    // Validation
    [ObservableProperty] private bool _hasNameError = false;

    public string UserEmail => _authService.GetCurrentUserEmail() ?? string.Empty;

    public ProfileSetupViewModel(
        IProfileService profileService,
        IDatabaseService db,
        IAuthService authService,
        IServiceProvider serviceProvider)
    {
        _profileService = profileService;
        _db = db;
        _authService = authService;
        _serviceProvider = serviceProvider;
        Title = "Set Up Your Profile";
    }

    [RelayCommand]
    private async Task PickAvatarAsync()
    {
        try
        {
            var result = await MediaPicker.PickPhotoAsync(new MediaPickerOptions
            {
                Title = "Choose a profile photo"
            });

            if (result == null) return;

            // Copy to app data directory with a unique name to bypass MAUI's ImageSource cache
            var destPath = Path.Combine(FileSystem.AppDataDirectory, $"avatar_{Guid.NewGuid():N}.jpg");
            using var stream = await result.OpenReadAsync();
            using var dest   = File.OpenWrite(destPath);
            await stream.CopyToAsync(dest);

            AvatarPath   = destPath;
            AvatarSource = ImageSource.FromFile(destPath);
            HasAvatar    = true;

            // Notice we only Cache it here temporarily for display 
            Preferences.Set("avatar_path", destPath);
        }
        catch (Exception ex)
        {
            await Application.Current?.MainPage?.DisplayAlert("Error", $"Could not pick photo: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    private async Task SaveProfileAsync()
    {
        // Reset validation
        HasNameError = false;

        if (string.IsNullOrWhiteSpace(FullName))
        {
            HasNameError = true;
            return;
        }

        IsBusy = true;

        string? avatarUrl = null;

        // Upload avatar to Supabase Storage if the user picked one
        if (HasAvatar && !string.IsNullOrEmpty(AvatarPath))
        {
            var uploadResult = await _profileService.UploadAvatarAsync(AvatarPath);
            if (!string.IsNullOrEmpty(uploadResult.ErrorMessage))
            {
                IsBusy = false;
                await Application.Current?.MainPage?.DisplayAlert("Avatar Upload Failed", $"Could not upload avatar: {uploadResult.ErrorMessage}\n\nPlease check your Supabase Storage bucket name and RLS policies.", "OK");
                return;
            }
            avatarUrl = uploadResult.Url;
        }

        // 1. Save to Supabase profiles table
        var result = await _profileService.SaveProfileAsync(new ProfileData(
            FullName: FullName.Trim(),
            Bio: string.IsNullOrWhiteSpace(Bio) ? null : Bio.Trim(),
            AvatarUrl: avatarUrl 
        ));

        // 2. Override/update local SQLite UserProfile so the rest of the app sees it
        await _db.SaveAsync(new UserProfile
        {
            Id          = 1,
            Name        = FullName.Trim(),
            PersonalGoal = string.IsNullOrWhiteSpace(Bio) ? "Feel better every day" : Bio.Trim(),
            AvatarPath  = AvatarPath,
            DailyNotifications = false,
            CreatedAt   = DateTime.Now
        });

        // 3. Cache name for quick access across the app
        Preferences.Set("user_name", FullName.Trim());

        IsBusy = false;

        if (!result.Success)
        {
            // Non-critical failure — log but still let the user through
            // (local DB already saved, so the app will work offline)
            await Application.Current?.MainPage?.DisplayAlert(
                "Error Saving to Cloud",
                $"Profile saved locally. Cloud sync failed because: {result.ErrorMessage}",
                "OK");
        }

        // Navigate into the main app
        Application.Current!.MainPage = _serviceProvider.GetRequiredService<AppShell>();
    }
}
