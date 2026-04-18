using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using M1ndLink.Models;
using M1ndLink.Services;

namespace M1ndLink.ViewModels;

public partial class EditProfileViewModel : BaseViewModel
{
    private readonly IDatabaseService _db;
    private readonly INotificationService _notifications;
    private readonly IProfileService _profileService;

    [ObservableProperty] private string _name         = string.Empty;
    [ObservableProperty] private string _personalGoal = string.Empty;
    [ObservableProperty] private string? _avatarPath  = null;
    [ObservableProperty] private bool   _hasAvatar    = false;
    [ObservableProperty] private ImageSource? _avatarSource = null;

    public EditProfileViewModel(IDatabaseService db, INotificationService notifications, IProfileService profileService)
    {
        _db            = db;
        _notifications = notifications;
        _profileService = profileService;
        Title          = "Edit Profile";
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        var profile = await _db.GetByIdAsync<UserProfile>(1);
        if (profile == null) return;

        Name         = profile.Name;
        PersonalGoal = profile.PersonalGoal;
        AvatarPath   = profile.AvatarPath;
        RefreshAvatar();
    }

    private void RefreshAvatar()
    {
        if (!string.IsNullOrEmpty(AvatarPath))
        {
            if (AvatarPath.StartsWith("http"))
            {
                AvatarSource = ImageSource.FromUri(new Uri(AvatarPath));
                HasAvatar    = true;
            }
            else if (File.Exists(AvatarPath))
            {
                AvatarSource = ImageSource.FromFile(AvatarPath);
                HasAvatar    = true;
            }
            else
            {
                AvatarSource = null;
                HasAvatar    = false;
            }
        }
        else
        {
            AvatarSource = null;
            HasAvatar    = false;
        }
    }

    // ─── Photo picker ────────────────────────────────────────────────────────
    [RelayCommand]
    public async Task PickPhotoAsync()
    {
        try
        {
            var result = await MediaPicker.PickPhotoAsync(new MediaPickerOptions
            {
                Title = "Select profile photo"
            });
            if (result == null) return;
            await SavePhotoAsync(result);
        }
        catch (PermissionException)
        {
            await Shell.Current.DisplayAlert("Permission Required",
                "Photo library access is needed to pick a photo.", "OK");
        }
    }

    [RelayCommand]
    public async Task TakePhotoAsync()
    {
        try
        {
            if (!MediaPicker.Default.IsCaptureSupported)
            {
                await Shell.Current.DisplayAlert("Not Supported",
                    "Camera is not available on this device.", "OK");
                return;
            }

            var result = await MediaPicker.CapturePhotoAsync();
            if (result == null) return;
            await SavePhotoAsync(result);
        }
        catch (PermissionException)
        {
            await Shell.Current.DisplayAlert("Permission Required",
                "Camera access is required to take a photo.", "OK");
        }
    }

    private async Task SavePhotoAsync(FileResult result)
    {
        // Copy to app data so it persists across sessions
        var destDir  = Path.Combine(FileSystem.AppDataDirectory, "avatars");
        Directory.CreateDirectory(destDir);
        var destPath = Path.Combine(destDir, $"avatar_{DateTime.Now:yyyyMMddHHmmss}.jpg");

        using var src  = await result.OpenReadAsync();
        using var dest = File.OpenWrite(destPath);
        await src.CopyToAsync(dest);

        AvatarPath   = destPath;
        AvatarSource = ImageSource.FromFile(destPath);
        HasAvatar    = true;
    }

    // ─── Photo source choice sheet ────────────────────────────────────────────
    [RelayCommand]
    public async Task ChoosePhotoSourceAsync()
    {
        var action = await Shell.Current.DisplayActionSheet(
            "Profile Photo", "Cancel", null,
            "📷  Take Photo", "🖼️  Choose from Library");

        if (action == "📷  Take Photo")
            await TakePhotoAsync();
        else if (action == "🖼️  Choose from Library")
            await PickPhotoAsync();
    }

    // ─── Save ─────────────────────────────────────────────────────────────────
    [RelayCommand]
    public async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            await Shell.Current.DisplayAlert("Validation", "Name cannot be empty.", "OK");
            return;
        }

        IsBusy = true;
        try
        {
            // 1. If AvatarPath is a local physical file (not start with http), upload it
            string? remoteAvatarUrl = AvatarPath;
            if (!string.IsNullOrEmpty(AvatarPath) && !AvatarPath.StartsWith("http"))
            {
                var uploadRes = await _profileService.UploadAvatarAsync(AvatarPath);
                if (uploadRes.Url != null)
                {
                    remoteAvatarUrl = uploadRes.Url;
                }
            }

            // 2. Await push to Supabase Profiles directly (no Task.Run)
            var cloudSaveRes = await _profileService.SaveProfileAsync(new ProfileData(Name.Trim(), PersonalGoal.Trim(), remoteAvatarUrl));
            if (!cloudSaveRes.Success)
            {
                // Silently swallow, or maybe log? We still want local to save even if offline
                Console.WriteLine($"Cloud Profile Save Failed: {cloudSaveRes.ErrorMessage}");
            }

            // 3. Save to Local SQLite
            var existingProfile = await _db.GetByIdAsync<UserProfile>(1);
            var profile = new UserProfile
            {
                Id           = 1,
                Name         = Name.Trim(),
                PersonalGoal = PersonalGoal.Trim(),
                AvatarPath   = remoteAvatarUrl, // Ensure we save the cloud link
                DailyNotifications = existingProfile?.DailyNotifications ?? false,
                CreatedAt = existingProfile?.CreatedAt ?? DateTime.Now
            };
            await _db.SaveAsync(profile);
            Preferences.Set("user_name",   profile.Name);
            Preferences.Set("avatar_path", profile.AvatarPath ?? string.Empty);

            // Log notification
            await _notifications.AddAsync(new AppNotification
            {
                Title   = "Profile Updated",
                Message = $"Your display name was changed to \"{profile.Name}\".",
                Icon    = "👤",
                Category = NotificationCategory.Profile
            });

            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Save Error", $"An error occurred preventing save: {ex.Message}", "OK");
        }
        finally 
        { 
            IsBusy = false; 
        }
    }
}
