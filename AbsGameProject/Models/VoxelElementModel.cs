using System.Text.Json.Serialization;

namespace AbsGameProject.Models;

[Flags]
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum CullFaceDirection
{
    None = 1 << 0,
    North = 1 << 1,
    East = 1 << 2,
    South = 1 << 3,
    West = 1 << 4,
    Up = 1 << 5,
    Down = 1 << 6,
    All = North | East | South | West | Up | Down
}

public class VoxelElementModelFace
{
    [JsonPropertyName("uv")]
    public float[] UV { get; init; }

    [JsonPropertyName("texture")]
    public string Texture { get; init; } = "";

    [JsonPropertyName("tintindex")]
    public int? TintIndex { get; init; } = null;

    [JsonPropertyName("cullface")]
    public CullFaceDirection? CullFace { get; init; }

    public VoxelElementModelFace()
    {
        UV = Array.Empty<float>();

        if (CullFace == null)
            CullFace = CullFaceDirection.All;
    }
}

public class VoxelElementModel
{
    [JsonPropertyName("from")]
    public float[] From { get; init; }
    [JsonPropertyName("to")]
    public float[] To { get; init; }
    [JsonPropertyName("faces")]
    public Dictionary<CullFaceDirection, VoxelElementModelFace> Faces { get; init; } = new();
}
