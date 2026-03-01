using System.Text.Json;
using System.Text.Json.Serialization;
using chession.Models;

namespace chession.Models;

[JsonSerializable(typeof(TokenData))]
[JsonSerializable(typeof(CurrentGamesResponse))]
[JsonSerializable(typeof(CurrentGame))]
[JsonSerializable(typeof(GameStatusWrapper))]
[JsonSerializable(typeof(VariantWrapper))]
public partial class ChessionJsonContext : JsonSerializerContext
{
}
