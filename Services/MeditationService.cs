using M1ndLink.Models;

namespace M1ndLink.Services;

public class MeditationService : IMeditationService
{
    private readonly IReadOnlyList<Meditation> _meditations = new List<Meditation>
    {
        new()
        {
            Id = 1,
            Title = "Morning Reset",
            Subtitle = "A gentle start for busy days",
            Description = "Settle your breathing, release overnight tension, and start the day with intention.",
            Category = "Focus",
            Difficulty = "Beginner",
            IconEmoji = "🌅",
            DurationMinutes = 5,
            GradientStart = Color.FromArgb("#DBEAFE"),
            GradientEnd = Color.FromArgb("#BFDBFE"),
            Benefits = new[] { "Grounds your attention", "Reduces morning stress", "Builds a calm routine" },
            Script = new[]
            {
                "Welcome. Let yourself arrive fully in this moment.",
                "Sit comfortably and soften your shoulders.",
                "Take one slow breath in through the nose.",
                "Exhale gently and let the jaw unclench.",
                "Notice the contact between your body and the surface beneath you.",
                "With each inhale, invite steadiness.",
                "With each exhale, release urgency.",
                "Bring to mind one thing that matters most today.",
                "You do not need to do everything at once.",
                "You only need the next calm step.",
                "Take one final breath and carry this steadiness into your day."
            }
        },
        new()
        {
            Id = 2,
            Title = "Body Scan Unwind",
            Subtitle = "Release tension from head to toe",
            Description = "A guided body scan that helps you notice and soften held tension.",
            Category = "Relaxation",
            Difficulty = "Beginner",
            IconEmoji = "🌊",
            DurationMinutes = 8,
            GradientStart = Color.FromArgb("#E0E7FF"),
            GradientEnd = Color.FromArgb("#C7D2FE"),
            Benefits = new[] { "Reduces physical tension", "Supports rest", "Builds body awareness" },
            Script = new[]
            {
                "Welcome. Find a position that feels supported.",
                "Allow your breath to move at its natural pace.",
                "Bring attention to the top of your head.",
                "Soften the forehead and the space around the eyes.",
                "Release any tension in the jaw, tongue, and throat.",
                "Let the shoulders drop away from the ears.",
                "Notice your arms, elbows, wrists, and hands becoming heavy.",
                "Bring awareness to the chest and the steady rhythm of breath.",
                "Let the belly soften and unclench.",
                "Relax through the hips, thighs, knees, calves, and feet.",
                "Feel the whole body resting together.",
                "Take a slow breath in, and exhale with ease."
            }
        },
        new()
        {
            Id = 3,
            Title = "Sleep Preparation",
            Subtitle = "Slow the mind before bed",
            Description = "Ease racing thoughts and prepare your body for deeper rest.",
            Category = "Sleep",
            Difficulty = "Intermediate",
            IconEmoji = "🌙",
            DurationMinutes = 10,
            GradientStart = Color.FromArgb("#EFF6FF"),
            GradientEnd = Color.FromArgb("#DBEAFE"),
            Benefits = new[] { "Quiets mental chatter", "Supports sleep", "Encourages slower breathing" },
            Script = new[]
            {
                "This is your time to slow down.",
                "Let the day begin to loosen its grip.",
                "Breathe in slowly for four.",
                "Exhale longer than you inhale.",
                "There is nothing to solve right now.",
                "If thoughts appear, let them drift by without following them.",
                "Feel the bed or chair carrying your weight.",
                "Allow the muscles in your face and neck to soften.",
                "Let your breathing become quieter and smoother.",
                "Tell yourself: I can rest now.",
                "Tell yourself: I am safe enough to be still.",
                "Take one final breath and let the body settle."
            }
        },
        new()
        {
            Id = 4,
            Title = "Confidence Reset",
            Subtitle = "Re-center before a difficult moment",
            Description = "Use this session before a meeting, exam, or stressful conversation.",
            Category = "Confidence",
            Difficulty = "Intermediate",
            IconEmoji = "✨",
            DurationMinutes = 6,
            GradientStart = Color.FromArgb("#FEF3C7"),
            GradientEnd = Color.FromArgb("#FDE68A"),
            Benefits = new[] { "Calms nerves", "Sharpens focus", "Builds self-trust" },
            Script = new[]
            {
                "Pause before the next moment begins.",
                "Plant your feet and lengthen your spine.",
                "Breathe in slowly and fully.",
                "Exhale and let the breath carry tension out of your body.",
                "Notice that your body knows how to steady itself.",
                "You do not need to feel perfect to move forward.",
                "Bring your attention to what is within your control.",
                "Choose one clear intention for the next few minutes.",
                "Let that intention anchor you.",
                "Take one more breath and step in with calm confidence."
            }
        }
    };

    public IReadOnlyList<Meditation> GetMeditations() => _meditations;

    public Meditation? GetById(int id) => _meditations.FirstOrDefault(m => m.Id == id);
}
