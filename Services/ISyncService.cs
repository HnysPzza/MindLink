namespace M1ndLink.Services;

public interface ISyncService
{
    /// <summary>
    /// Connects to Supabase, pulls all historic Moods, Assessments, and Exercises for this account,
    /// forcefully clears the local SQLite caches for these tables, and repopulates them locally.
    /// Blocks navigation until fully complete.
    /// </summary>
    Task PullFromCloudAsync();

    /// <summary>
    /// Automatically uploads any modified or un-synced local data to the Supabase Cloud.
    /// Intended to be run silently in the background (fire and forget).
    /// </summary>
    Task PushToCloudAsync();
}
