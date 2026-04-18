using Postgrest.Models;
using Postgrest.Attributes;

namespace M1ndLink.Services;

// ── Postgrest model matching the public.profiles table ─────────────────────
[Table("profiles")]
public class SupabaseProfile : BaseModel
{
    [PrimaryKey("id", false)]
    public string Id { get; set; } = string.Empty;

    [Column("email")]
    public string? Email { get; set; }

    [Column("full_name")]
    public string? FullName { get; set; }

    [Column("bio")]
    public string? Bio { get; set; }

    [Column("avatar_url")]
    public string? AvatarUrl { get; set; }

    [Column("is_setup_complete")]
    public bool IsSetupComplete { get; set; }
}

// ── Service ─────────────────────────────────────────────────────────────────
public class SupabaseProfileService : IProfileService
{
    private readonly Supabase.Client _supabase;

    public SupabaseProfileService(Supabase.Client supabase)
    {
        _supabase = supabase;
    }

    public async Task<(bool Success, string ErrorMessage)> SaveProfileAsync(ProfileData profile)
    {
        try
        {
            var userId = _supabase.Auth.CurrentUser?.Id;
            var email  = _supabase.Auth.CurrentUser?.Email;

            if (string.IsNullOrEmpty(userId))
                return (false, "Not authenticated.");

            var row = new SupabaseProfile
            {
                Id        = userId,
                Email     = email,
                FullName  = profile.FullName,
                Bio       = profile.Bio,
                AvatarUrl = profile.AvatarUrl,
                IsSetupComplete = true
            };

            await _supabase
                .From<SupabaseProfile>()
                .Upsert(row);

            return (true, string.Empty);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<ProfileData?> GetProfileAsync()
    {
        try
        {
            var userId = _supabase.Auth.CurrentUser?.Id;
            if (string.IsNullOrEmpty(userId)) return null;

            var response = await _supabase
                .From<SupabaseProfile>()
                .Where(p => p.Id == userId)
                .Single();

            if (response == null) return null;

            return new ProfileData(
                response.FullName ?? string.Empty,
                response.Bio,
                response.AvatarUrl
            );
        }
        catch
        {
            return null;
        }
    }

    public async Task<(string? Url, string? ErrorMessage)> UploadAvatarAsync(string localFilePath)
    {
        try
        {
            var userId = _supabase.Auth.CurrentUser?.Id;
            if (string.IsNullOrEmpty(userId) || !File.Exists(localFilePath)) 
                return (null, "User ID missing or file does not exist.");

            byte[] bytes = await File.ReadAllBytesAsync(localFilePath);
            string extension = Path.GetExtension(localFilePath);
            string remoteFileName = $"{userId}_{Guid.NewGuid()}{extension}"; // GUID ensures it bypasses CDN/image caching

            // The user explicitly named the bucket "Cassie Profike"
            var storage = _supabase.Storage.From("Cassie Profike");
            
            await storage.Upload(bytes, remoteFileName, new Supabase.Storage.FileOptions { Upsert = true });
            
            string publicUrl = storage.GetPublicUrl(remoteFileName);
            return (publicUrl, null);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"UploadAvatarAsync Error: {ex.Message}");
            return (null, ex.Message);
        }
    }
}
