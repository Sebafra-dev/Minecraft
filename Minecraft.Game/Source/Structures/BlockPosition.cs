namespace Minecraft.Source.Structures
{
    public struct BlockPosition
    {
        private readonly ushort _position;

        public BlockPosition(int x, int y, int z)
        {
            _position = (ushort)(((x & 0xF) << 12) | ((y & 0xFF) << 4) | (z & 0xF));
        }

        public readonly IntPosition ToIntPosition()
        {
            int x = (_position >> 12) & 0xF;
            int y = (_position >> 4) & 0xFF;
            int z = _position & 0xF;

            return new IntPosition(x, y, z);
        }
    }
}
