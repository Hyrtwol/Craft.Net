using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Craft.Net.Common;
using Craft.Net.TerrainGeneration;
using NUnit.Framework;
using fNbt;
using fNbt.Serialization;

namespace Craft.Net.Anvil.Test
{
    [TestFixture]
    public class LevelTest
    {
        [Test]
        public void TestSaveAndLoad()
        {
            var t = typeof(FlatlandGenerator);
            var level = new Level();
            level.LevelName = "Example";
            level.Save("test.dat");
            var loaded = Level.Load("test.dat");
            Assert.AreEqual(level.LevelName, loaded.LevelName);
        }


        //[Test]
        //public void TestLoad()
        //{
        //    var t = typeof (FlatlandGenerator);
        //    var fileName = @"C:\Users\tlc\AppData\Roaming\.minecraft\saves\Empty\level.dat";
        //    var level = Level.Load(fileName);
        //    Assert.IsNotNull(level);
        //    Debug.WriteLine(level.LevelName);
        //}

        //[Test]
        //public void TestLoad2()
        //{
        //    var t = typeof(FlatlandGenerator);
        //    var file = @"C:\Users\tlc\AppData\Roaming\.minecraft\saves\Empty\level.dat";

        //    var serializer = new NbtSerializer(typeof(Level));
        //    var nbtFile = new NbtFile(file);
        //    var level = (Level)serializer.Deserialize(nbtFile.RootTag["Data"]);
        //    //level.DatFile = file;
        //    //level.BaseDirectory = Path.GetDirectoryName(file);

        //}

        [TestCase("Empty")]
        [TestCase("SkyBlock2.1")]
        [TestCase("World172")]
        public void LoadLevel(string worldName)
        {
            var t = typeof(FlatlandGenerator);
            var level = Level.LoadSavedLevel(worldName);
            Assert.IsNotNull(level);
            DumpLevel(level);
        }

        private static void DumpLevel(Level level)
        {
            Debug.Print("LevelName     = {0}", level.LevelName);
            Debug.Print("GeneratorName = {0}", level.GeneratorName);
            Debug.Print("GameMode      = {0}", level.GameMode);
            Debug.Print("Spawn         = {0}", level.Spawn);
            Debug.WriteLine("Worlds:");
            Debug.Indent();
            foreach (var world in level.Worlds)
            {
                Debug.Print("Name = {0}", world.Name);
                Debug.Print("BaseDirectory = {0}", world.BaseDirectory);
                Debug.WriteLine("Regions:");
                Debug.Indent();
                foreach (var region in world.Regions.Values)
                {
                    Debug.Print("  Position = {0}", region.Position);
                    Debug.Print("  Chunks Count = {0}", region.Chunks.Values.Count);

                    
                }
                Debug.Unindent();
            }
            Debug.Unindent();
        }

        [Test]
        public void UpdateEmptyWorld()
        {
            Debug.Print("RegionWidth {0} x ChunkWidth {1} = {2}", Region.Width, Chunk.Width, Region.Width * Chunk.Width);
            var t = typeof (FlatlandGenerator);
            var level = Level.LoadSavedLevel("Modflat");
            Assert.IsNotNull(level);
            DumpLevel(level);
            var world = level.DefaultWorld;
            //World.LoadWorld()

            var labyrinth = new Labyrinth(12);
            var dim = labyrinth.Dimension;
            Debug.Print("Dimension={0}", dim);
            var res = labyrinth.Generate();

            //var p = new Coordinates3D();
            for (int z = 0; z < dim; z++)
            {
                //p.Z = z;
                for (int x = 0; x < dim; x++)
                {
                    //p.X = x;
                    Debug.Write(res[x, z] > 0 ? ' ' : 'X');
                    //for (int y = 60; y < 70; y++)
                    //{
                    //    p.Y = y;
                    //    world.SetBlockId(p, 1);
                    //}
                    //for (int y = 70; y < Chunk.Height; y++)
                    //{
                    //    p.Y = y;
                    //    world.SetBlockId(p, 0);
                    //}

                }
                Debug.WriteLine("");
            }

            /*
            Chunk chunk = null;
            var chunkPos = new Coordinates2D(0, 0);
            Coordinates2D last = new Coordinates2D(-1, -1);
            var p = new Coordinates3D();
            for (int z = 0; z < dim; z++)
            {
                chunkPos.Z = z/16;
                for (int x = 0; x < dim; x++)
                {
                    chunkPos.X = x / 16;
                    Debug.Write(res[x, z] > 0 ? 'X' : ' ');

                    if (last != chunkPos || chunk == null)
                    {
                        chunk = world.GetChunk(chunkPos);
                    }

                    if (chunk != null)
                    {
                        p.X = x & 15;
                        p.Z = z & 15;
                        for (int y = 60; y < 70; y++)
                        {
                            p.Y = y;
                            chunk.SetBlockId(p, 1);
                        }
                        for (int y = 70; y < Chunk.Height; y++)
                        {
                            p.Y = y;
                            chunk.SetBlockId(p, 0);
                        }
                    }
                }
                Debug.WriteLine("");
            }
            */

            short air = 0;
            short stone = 1;

            short floorY = 63;
            short cielY = 68;

            int cc = dim/16;

            var p = new Coordinates3D();
            for (int cz = 0; cz < cc; cz++)
            {
                for (int cx = 0; cx < cc; cx++)
                {
                    int ox = cx*16;
                    int oz = cz*16;
                    var chunkPos = new Coordinates2D(cx, cz);
                    var chunk = world.GetChunk(chunkPos);
                    if (chunk != null)
                    {
                        for (int z = 0; z < Chunk.Depth; z++)
                        {
                            p.Z = z;
                            for (int x = 0; x < Chunk.Width; x++)
                            {
                                p.X = x;
                                var blockId = res[ox + x, oz + z] > 0 ? air : stone;
                                //for (int y = 60; y < 65; y++)
                                {
                                    p.Y = floorY;
                                    chunk.SetBlockId(p, stone);
                                }
                                for (int y = floorY + 1; y < cielY; y++)
                                {
                                    p.Y = y;
                                    chunk.SetBlockId(p, blockId);
                                }
                                for (int y = cielY; y < Chunk.Height; y++)
                                {
                                    p.Y = y;
                                    chunk.SetBlockId(p, 0);
                                }
                            }
                        }
                        //world.Save();
                    }
                }
            }
            world.Save();
            //level.Save();
        }

        [Test]
        public void Jonas()
        {
            var t = typeof(FlatlandGenerator);
            //var level = Level.LoadSavedLevel("Empty");
            var level = new Level();
            level.LevelName = "Jonas";
            level.GeneratorName = "default";
            Assert.IsNotNull(level);
            level.AddWorld("region");
            DumpLevel(level);
        }

        [Test]
        public void GenerateTiles()
        {
            Debug.WriteLine("            var lala = new[,,]");
            Debug.WriteLine("                {");
            var tiles = new int[16,4,4];
            for (int tile = 1; tile < 16; tile++)
            {
                int b = tile & 1, l = (tile >> 1) & 1, t = (tile >> 2) & 1, r = (tile >> 3) & 1;
                tiles[tile, 0, 1] = t;
                tiles[tile, 0, 2] = t;
                tiles[tile, 1, 0] = l;
                tiles[tile, 2, 0] = l;
                tiles[tile, 1, 3] = r;
                tiles[tile, 2, 3] = r;
                tiles[tile, 3, 1] = b;
                tiles[tile, 3, 2] = b;
                // center
                tiles[tile, 1, 1] = 1;
                tiles[tile, 1, 2] = 1;
                tiles[tile, 2, 1] = 1;
                tiles[tile, 2, 2] = 1;
            }

            for (int tile = 0; tile < 16; tile++)
            {
                Debug.WriteLine("                    {{  // tile {0}", tile);
                for (int i = 0; i < 4; i++)
                {
                    Debug.Print("                       {{{0},{1},{2},{3}}}{4}",
                        tiles[tile, i, 0], tiles[tile, i, 1], tiles[tile, i, 2], tiles[tile, i, 3],
                        (i < 3 ? "," : ""));
                }
                Debug.Print("                    }}{0}", (tile < 15 ? "," : ""));
            }
            Debug.WriteLine("                };");
        }

        [Test]
        public void GenerateLabyrinth()
        {
            var labyrinth = new Labyrinth(8);
            var dim = labyrinth.Dimension;
            Debug.Print("Dimension={0}", dim);
            var res = labyrinth.Generate();
            for (int z = 0; z < dim; z++)
            {
                for (int x  = 0; x < dim; x++)
                {
                    Debug.Write(res[x,z] >0?'X':' ');
                }
                Debug.WriteLine("");
            }
        }
    }

    //[StructLayout(LayoutKind.Sequential, Pack = 1, Size = TileSize)]
    //public struct Tile
    //{
    //    private const int TileSize = 32 * 32 * 4;

    //    public unsafe fixed byte data[TileSize];
    //}
}
