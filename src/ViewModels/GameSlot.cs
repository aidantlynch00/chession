using chession.Models;

namespace chession.ViewModels;

/// <summary>
/// Represents a slot for displaying a game result in the UI.
/// </summary>
public class GameSlot
{
    /// <summary>
    /// Gets the game result for this slot, if any.
    /// </summary>
    public GameResult? Result { get; }

    /// <summary>
    /// Initializes a new instance of the GameSlot class.
    /// </summary>
    /// <param name="result">The game result, or null for an empty slot.</param>
    public GameSlot(GameResult? result)
    {
        Result = result;
    }
}
