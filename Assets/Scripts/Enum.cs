public enum PrimitiveType
{
    TRIANGLES,
    LINES
}

public enum ClearMask
{
    COLOR = 1 << 0,
    DEPTH = 1 << 1
}

public enum CullType
{
    None,
    Back,
    Front
}
