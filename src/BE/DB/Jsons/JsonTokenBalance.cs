﻿using System.Text.Json.Serialization;

namespace Chats.BE.DB.Jsons;

public record JsonTokenBalance
{
    [JsonPropertyName("modelId")]
    public required Guid ModelId { get; init; }

    [JsonPropertyName("tokens"), JsonConverter(typeof(NumberToStringConverter))]
    public required string Tokens { get; init; }

    [JsonPropertyName("counts"), JsonConverter(typeof(NumberToStringConverter))]
    public required string Counts { get; init; }

    [JsonPropertyName("expires")]
    public required string Expires { get; init; }

    [JsonPropertyName("enabled")]
    public required bool Enabled { get; init; }
}