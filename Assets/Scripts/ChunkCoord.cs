namespace MyCraft
{
    public readonly struct ChunkCoord
    {
        public readonly int x;
        public readonly int z;

        public ChunkCoord(int x, int z)
        {
            this.x = x;
            this.z = z;
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj.GetType() != GetType()) return false;

            var obj_ = (ChunkCoord)obj;
            return (x == obj_.x) && (z == obj_.z);
        }

        public override int GetHashCode() => (x, z).GetHashCode();

        public override string ToString() => $"ChunkCoord: {(x, z).ToString()}";

        public static bool operator ==(ChunkCoord a, ChunkCoord b) => a.Equals(b);
        public static bool operator !=(ChunkCoord a, ChunkCoord b) => !(a == b);
    }
}
