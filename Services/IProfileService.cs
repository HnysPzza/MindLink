namespace M1ndLink.Services;

public record ProfileData(
    string FullName,
    string? Bio,
    string? AvatarUrl
);

public interface IProfileService
{
    /// <summary>Upserts the profile row in Supabase for the current user.</summary>
    Task<(bool Success, string ErrorMessage)> SaveProfileAsync(ProfileData profile);

    /// <summary>Loads the profile row from Supabase for the current user.</summary>
    Task<ProfileData?> GetProfileAsync();

    /// <summary>Uploads the avatar to Supabase Storage and returns the public URL.</summary>
    Task<(string? Url, string? ErrorMessage)> UploadAvatarAsync(string localFilePath);
}
