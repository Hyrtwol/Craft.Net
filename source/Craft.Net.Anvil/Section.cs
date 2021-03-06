using Craft.Net.Common;
using fNbt.Serialization;

namespace Craft.Net.Anvil
{
    public class Section
    {
        public const byte Width = 16, Height = 16, Depth = 16;
        const int size = Width * Height * Depth;

        public byte[] Blocks { get; set; }
        [TagName("Data")]
        public NibbleArray Metadata { get; set; }
        public NibbleArray BlockLight { get; set; }
        public NibbleArray SkyLight { get; set; }
        [IgnoreOnNull]
        public NibbleArray Add { get; set; }
        public byte Y { get; set; }

        private int nonAirCount;

        public Section()
        {
        }

        public Section(byte y)
        {
            this.Y = y;
            Blocks = new byte[size];
            Metadata = new NibbleArray(size);
            BlockLight = new NibbleArray(size);
            SkyLight = new NibbleArray(size);
            for (int i = 0; i < size; i++)
                SkyLight[i] = 0xFF;
            Add = null; // Only used when needed
            nonAirCount = 0;
        }

        [NbtIgnore]
        public bool IsAir
        {
            get { return nonAirCount == 0; }
        }

        public short GetBlockId(Coordinates3D coordinates)
        {
            int index = coordinates.X + Width * (coordinates.Z + Height * coordinates.Y);
            short value = Blocks[index];
            if (Add != null)
                value |= (short)(Add[index] << 8);
            return value;
        }

        public byte GetMetadata(Coordinates3D coordinates)
        {
            int index = coordinates.X + Width * (coordinates.Z + Height * coordinates.Y);
            return Metadata[index];
        }

        public byte GetSkyLight(Coordinates3D coordinates)
        {
            int index = coordinates.X + Width * (coordinates.Z + Height * coordinates.Y);
            return SkyLight[index];
        }

        public byte GetBlockLight(Coordinates3D coordinates)
        {
            int index = coordinates.X + Width * (coordinates.Z + Height * coordinates.Y);
            return BlockLight[index];
        }

        public void SetBlockId(Coordinates3D coordinates, short value)
        {
            int index = coordinates.X + Width * (coordinates.Z + Height * coordinates.Y);
            if (value == 0)
            {
                if (Blocks[index] != 0)
                    nonAirCount--;
            }
            else
            {
                if (Blocks[index] == 0)
                    nonAirCount++;
            }
            Blocks[index] = (byte)value;
            if ((value & ~0xFF) != 0)
            {
                if (Add == null) Add = new NibbleArray(size);
                Add[index] = (byte)((ushort)value >> 8);
            }
        }

        public void SetMetadata(Coordinates3D coordinates, byte value)
        {
            int index = coordinates.X + Width * (coordinates.Z + Height * coordinates.Y);
            Metadata[index] = value;
        }

        public void SetSkyLight(Coordinates3D coordinates, byte value)
        {
            int index = coordinates.X + Width * (coordinates.Z + Height * coordinates.Y);
            SkyLight[index] = value;
        }

        public void SetBlockLight(Coordinates3D coordinates, byte value)
        {
            int index = coordinates.X + Width * (coordinates.Z + Height * coordinates.Y);
            BlockLight[index] = value;
        }

        public void ProcessSection()
        {
            // TODO: Schedule updates
            nonAirCount = 0;
            for (int i = 0; i < Blocks.Length; i++)
            {
                if (Blocks[i] != 0)
                    nonAirCount++;
            }
        }
    }
}