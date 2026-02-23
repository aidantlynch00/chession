using chession.Models;

namespace chession.ViewModels;

public class GameSlot
{
    public GameResult? Result { get; }

    public GameSlot(GameResult? result)
    {
        Result = result;
    }
}
