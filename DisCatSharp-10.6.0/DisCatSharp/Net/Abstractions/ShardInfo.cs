using System;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DisCatSharp.Net.Abstractions;

/// <summary>
/// Represents data for identify payload's shard info.
/// </summary>
[JsonConverter(typeof(ShardInfoConverter))]
internal sealed class ShardInfo
{
	/// <summary>
	/// Gets or sets this client's shard id.
	/// </summary>
	public int ShardId { get; set; }

	/// <summary>
	/// Gets or sets the total shard count for this token.
	/// </summary>
	public int ShardCount { get; set; }
}

/// <summary>
/// Represents a shard info converter.
/// </summary>
internal sealed class ShardInfoConverter : JsonConverter
{
	/// <summary>
	/// Writes the json.
	/// </summary>
	/// <param name="writer">The writer.</param>
	/// <param name="value">The value.</param>
	/// <param name="serializer">The serializer.</param>
	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		var sinfo = value as ShardInfo;
		var obj = new object[] { sinfo.ShardId, sinfo.ShardCount };
		serializer.Serialize(writer, obj);
	}

	/// <summary>
	/// Reads the json.
	/// </summary>
	/// <param name="reader">The reader.</param>
	/// <param name="objectType">The object type.</param>
	/// <param name="existingValue">The existing value.</param>
	/// <param name="serializer">The serializer.</param>
	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		var arr = this.ReadArrayObject(reader, serializer);
		return new ShardInfo
		{
			ShardId = (int)arr[0],
			ShardCount = (int)arr[1]
		};
	}

	/// <summary>
	/// Reads the array object.
	/// </summary>
	/// <param name="reader">The reader.</param>
	/// <param name="serializer">The serializer.</param>
	private JArray ReadArrayObject(JsonReader reader, JsonSerializer serializer) =>
		serializer.Deserialize<JToken>(reader) is not JArray arr || arr.Count != 2
			? throw new JsonSerializationException("Expected array of length 2")
			: arr;

	/// <summary>
	/// Whether this can be converted.
	/// </summary>
	/// <param name="objectType">The object type.</param>
	public override bool CanConvert(Type objectType) => objectType == typeof(ShardInfo);
}
