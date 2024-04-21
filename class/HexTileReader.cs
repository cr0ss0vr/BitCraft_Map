using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitcraftWebMap.@class
{
    public class HexTileReader
    {
        public HexTileReader() { }

        public static HexTileCacheData ReadFromFile(string filename)
        {
            var Data = new HexTileCacheData();
            var fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            var BinaryReader = new BinaryReader(fileStream);

            Data.FileName = filename;
            Data.Magic = BinaryReader.ReadInt64();
            Data.ChunkX = ReadReversedInt(BinaryReader);
            Data.ChunkY = ReadReversedInt(BinaryReader);
            Data.ChunkZ = ReadReversedInt(BinaryReader);


            Data.Biomes = ReadUIntArray(BinaryReader, 32 * 32);
            Data.OriginalElevations = ReadShortArray(BinaryReader, 32 * 32);
            Data.Elevations = ReadShortArray(BinaryReader, 32 * 32);
            Data.UnknownBytes = BinaryReader.ReadBytes(32 * 32);
            Data.WaterLevels = ReadShortArray(BinaryReader, 32 * 32);
            Data.BiomeDensity = ReadUIntArray(BinaryReader, 32 * 32);

            BinaryReader.Close();
            fileStream.Close();
            return Data;
        }

        public static uint[] ReadUIntArray(BinaryReader Reader, int Count)
        {
            var List = new uint[Count];
            int x = 0;
            while (x < Count)
            {
                List[x] = Reader.ReadUInt32();
                x++;
            }
            return List;
        }

        public static short[] ReadShortArray(BinaryReader Reader, int Count)
        {
            var List = new short[Count];
            int x = 0;
            while (x < Count)
            {
                List[x] = Reader.ReadInt16();
                x++;
            }
            return List;
        }

        public static int ReadReversedInt(BinaryReader Reader)
        {
            var Bytes = Reader.ReadBytes(4);
            Bytes.Reverse();

            return BitConverter.ToInt32(Bytes, 0);
        }
    }
}
