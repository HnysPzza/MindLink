using M1ndLink.Models;

namespace M1ndLink.Services;

public interface IMeditationService
{
    IReadOnlyList<Meditation> GetMeditations();
    Meditation? GetById(int id);
}
