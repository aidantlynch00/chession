using System.Collections.Generic;
using System.Text.Json.Serialization;
using LichessSharp.Models.Games;
using LichessSharp.Models.Enums;

namespace chession.Models;

/// <summary>
/// Response containing the list of currently ongoing games.
/// </summary>
public class CurrentGamesResponse
{
    /// <summary>
    /// Gets or sets the list of games currently being played.
    /// </summary>
    [JsonPropertyName("nowPlaying")]
    public IReadOnlyList<CurrentGame>? NowPlaying { get; init; }
}

/// <summary>
/// Represents an ongoing game on Lichess.
/// </summary>
public class CurrentGame
{
    /// <summary>
    /// Gets or sets the full ID of the game.
    /// </summary>
    [JsonPropertyName("fullId")]
    public required string FullId { get; init; }

    /// <summary>
    /// Gets or sets the ID of the game.
    /// </summary>
    [JsonPropertyName("gameId")]
    public required string GameId { get; init; }

    /// <summary>
    /// Gets or sets the FEN position of the game.
    /// </summary>
    [JsonPropertyName("fen")]
    public required string Fen { get; init; }

    /// <summary>
    /// Gets or sets the color the user is playing.
    /// </summary>
    [JsonPropertyName("color")]
    public Color Color { get; init; }

    /// <summary>
    /// Gets or sets the last move played.
    /// </summary>
    [JsonPropertyName("lastMove")]
    public string? LastMove { get; init; }

    /// <summary>
    /// Gets or sets the source of the game.
    /// </summary>
    [JsonPropertyName("source")]
    public string? Source { get; init; }

    /// <summary>
    /// Gets or sets the status of the game.
    /// </summary>
    [JsonPropertyName("status")]
    public required GameStatusWrapper Status { get; init; }

    /// <summary>
    /// Gets or sets the variant of the game.
    /// </summary>
    [JsonPropertyName("variant")]
    public required VariantWrapper Variant { get; init; }

    /// <summary>
    /// Gets or sets the speed of the game.
    /// </summary>
    [JsonPropertyName("speed")]
    public Speed Speed { get; init; }

    /// <summary>
    /// Gets or sets the performance type.
    /// </summary>
    [JsonPropertyName("perf")]
    public string? Perf { get; init; }

    /// <summary>
    /// Gets or sets whether the game is rated.
    /// </summary>
    [JsonPropertyName("rated")]
    public bool Rated { get; init; }

    /// <summary>
    /// Gets or sets whether the user has moved.
    /// </summary>
    [JsonPropertyName("hasMoved")]
    public bool HasMoved { get; init; }

    /// <summary>
    /// Gets or sets the opponent information.
    /// </summary>
    [JsonPropertyName("opponent")]
    public OngoingGameOpponent? Opponent { get; init; }

    /// <summary>
    /// Gets or sets whether it's the user's turn.
    /// </summary>
    [JsonPropertyName("isMyTurn")]
    public bool IsMyTurn { get; init; }

    /// <summary>
    /// Gets or sets the seconds remaining for the user.
    /// </summary>
    [JsonPropertyName("secondsLeft")]
    public int SecondsLeft { get; init; }
}

/// <summary>
/// Wrapper for game status information.
/// </summary>
public class GameStatusWrapper
{
    /// <summary>
    /// Gets or sets the numeric status ID.
    /// </summary>
    [JsonPropertyName("id")]
    public int Id { get; init; }

    /// <summary>
    /// Gets or sets the status name.
    /// </summary>
    [JsonPropertyName("name")]
    public required GameStatus Name { get; init; }
}

/// <summary>
/// Wrapper for game variant information.
/// </summary>
public class VariantWrapper
{
    /// <summary>
    /// Gets or sets the variant key.
    /// </summary>
    [JsonPropertyName("key")]
    public required Variant Key { get; init; }

    /// <summary>
    /// Gets or sets the variant name.
    /// </summary>
    [JsonPropertyName("name")]
    public required string Name { get; init; }
}
