public readonly struct ChunkCoord
{
    public readonly int x;
    public readonly int z;

    public ChunkCoord(int x, int z)
    {
        this.x = x;
        this.z = z;
    }

    public bool Equals(in ChunkCoord temp)
    {
        if (temp == null)
            return false;
        return (x == temp.x) && (z == temp.z);
    }
    public static bool operator ==(ChunkCoord a, ChunkCoord b) => a.Equals(b);
    public static bool operator !=(ChunkCoord a, ChunkCoord b) => !(a == b);
}