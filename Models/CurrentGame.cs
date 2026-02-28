using System.Collections.Generic;
using System.Text.Json.Serialization;
using LichessSharp.Models.Games;
using LichessSharp.Models.Enums;

namespace chession.Models;

public class CurrentGamesResponse
{
    [JsonPropertyName("nowPlaying")]
    public IReadOnlyList<CurrentGame>? NowPlaying { get; init; }
}

public class CurrentGame
{
    [JsonPropertyName("fullId")]
    public required string FullId { get; init; }

    [JsonPropertyName("gameId")]
    public required string GameId { get; init; }

    [JsonPropertyName("fen")]
    public required string Fen { get; init; }

    [JsonPropertyName("color")]
    public Color Color { get; init; }

    [JsonPropertyName("lastMove")]
    public string? LastMove { get; init; }

    [JsonPropertyName("source")]
    public string? Source { get; init; }

    [JsonPropertyName("status")]
    public required GameStatusWrapper Status { get; init; }

    [JsonPropertyName("variant")]
    public required VariantWrapper Variant { get; init; }

    [JsonPropertyName("speed")]
    public Speed Speed { get; init; }

    [JsonPropertyName("perf")]
    public string? Perf { get; init; }

    [JsonPropertyName("rated")]
    public bool Rated { get; init; }

    [JsonPropertyName("hasMoved")]
    public bool HasMoved { get; init; }

    [JsonPropertyName("opponent")]
    public OngoingGameOpponent? Opponent { get; init; }

    [JsonPropertyName("isMyTurn")]
    public bool IsMyTurn { get; init; }

    [JsonPropertyName("secondsLeft")]
    public int SecondsLeft { get; init; }
}

public class GameStatusWrapper
{
    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("name")]
    public required GameStatus Name { get; init; }
}

public class VariantWrapper
{
    [JsonPropertyName("key")]
    public required Variant Key { get; init; }

    [JsonPropertyName("name")]
    public required string Name { get; init; }
}
