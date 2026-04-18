using M1ndLink.Models;

namespace M1ndLink.Services;

public class DailyTipService : IDailyTipService
{
    private readonly Supabase.Client _supabase;

    public DailyTipService(Supabase.Client supabase)
    {
        _supabase = supabase;
    }

    public async Task<DailyTip> GetTipOfTheDayAsync()
    {
        try
        {
            var response = await _supabase.From<SupabaseDailyTip>().Get();
            var remoteTips = response.Models
                .Select(model => new DailyTip
                {
                    Title = model.Title,
                    Content = model.Content,
                    Category = model.Category,
                    Icon = string.IsNullOrWhiteSpace(model.Icon) ? "💡" : model.Icon
                })
                .Where(tip => !string.IsNullOrWhiteSpace(tip.Title) && !string.IsNullOrWhiteSpace(tip.Content))
                .ToList();

            if (remoteTips.Count > 0)
                return remoteTips[IndexForToday(remoteTips.Count)];
        }
        catch
        {
        }

        var fallback = new List<DailyTip>
        {
            new() { Title = "Reset Your Shoulders", Content = "Drop your shoulders away from your ears and unclench your jaw before the next task.", Category = "Self-Care", Icon = "🧘" },
            new() { Title = "Name One Win", Content = "Even on hard days, identify one thing you handled better than yesterday.", Category = "Motivation", Icon = "✨" },
            new() { Title = "Lengthen the Exhale", Content = "A longer exhale tells your body that it is safe enough to slow down.", Category = "Breathing", Icon = "🫁" },
            new() { Title = "Check Your Battery", Content = "Ask yourself whether you need food, water, movement, or rest before pushing harder.", Category = "Mental Health", Icon = "🔋" }
        };

        return fallback[IndexForToday(fallback.Count)];
    }

    private static int IndexForToday(int count) =>
        Math.Abs(DateTime.Today.DayOfYear) % Math.Max(count, 1);
}
