using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitcraftWebMap.@class
{
    public class HexTileCacheData
    {
        public string FileName;

        public long Magic;
        public int ChunkX;
        public int ChunkY;
        public int ChunkZ;

        public uint[] Biomes;
        public short[] WaterLevels;
        public short[] OriginalElevations;
        public byte[] UnknownBytes;
        public short[] Elevations;
        public uint[] BiomeDensity;

        public void Display()
        {
            Console.WriteLine("Biomes: ");
            DisplayArray(Biomes, 32, 32);

            Console.WriteLine("Biome Density?: ");
            DisplayArray(BiomeDensity, 32, 32);

            Console.WriteLine("WaterLevels: ");
            DisplayArray(WaterLevels, 32, 32);

            Console.WriteLine("OriginalElevations: ");
            DisplayArray(OriginalElevations, 32, 32);

            Console.WriteLine("Elevations: ");
            DisplayArray(Elevations, 32, 32);

        }

        public void DisplayArray<T>(T[] Array, int x, int y)
        {
            Console.WriteLine();


            for (int i = 0; i < Array.Length; i++)
            {
                Console.Write(Array[i] + " ");

                if (i % 32 == 0)
                {
                    Console.WriteLine();
                }
            }

            Console.WriteLine();
        }
    }
}
