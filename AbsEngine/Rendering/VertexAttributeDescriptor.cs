namespace AbsEngine.Rendering;

public enum VertexAttributeFormat
{
    Float32,
    Float16,
    UNorm8,
    SNorm8,
    UNorm16,
    SNorm16,
    UInt8,
    SInt8,
    UInt16,
    SInt16,
    UInt32,
    SInt32,
}

public class VertexAttributeDescriptor
{
    public VertexAttributeFormat Format { get; private set; }
    public int Dimension { get; private set; }

    public VertexAttributeDescriptor(VertexAttributeFormat format, int dimension)
    {
        Format = format;
        Dimension = dimension;
    }

    internal int SizeOf()
    {
        switch (Format)
        {
            case VertexAttributeFormat.Float32:
                return 4;
            case VertexAttributeFormat.Float16:
                return 2;
            case VertexAttributeFormat.UNorm8:
                return 1;
            case VertexAttributeFormat.SNorm8:
                return 1;
            case VertexAttributeFormat.UNorm16:
                return 2;
            case VertexAttributeFormat.SNorm16:
                return 2;
            case VertexAttributeFormat.UInt8:
                return 1;
            case VertexAttributeFormat.SInt8:
                return 1;
            case VertexAttributeFormat.UInt16:
                return 2;
            case VertexAttributeFormat.SInt16:
                return 2;
            case VertexAttributeFormat.UInt32:
                return 4;
            case VertexAttributeFormat.SInt32:
                return 4;
        }

        throw new InvalidDataException($"Unknown VertexAttributeFormat: {Format}");
    }
}
