using System;

namespace chession.Models;

/// <summary>
/// Represents stored token data including the token and storage timestamp.
/// </summary>
public record TokenData(string Token, DateTimeOffset StoredAt);
