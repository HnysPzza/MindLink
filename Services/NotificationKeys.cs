namespace M1ndLink.Services;

public static class NotificationKeys
{
    public const int MorningReminderId = 4101;
    public const int EveningReminderId = 4102;
    public const int StreakReminderId = 4103;
    public const int ExerciseReminderId = 4104;
    public const int TodoReminderBaseId = 7000;

    public static string RoutePayload(string route) => $"route:{route}";
}
