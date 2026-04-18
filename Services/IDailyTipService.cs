using M1ndLink.Models;

namespace M1ndLink.Services;

public interface IDailyTipService
{
    Task<DailyTip> GetTipOfTheDayAsync();
}
