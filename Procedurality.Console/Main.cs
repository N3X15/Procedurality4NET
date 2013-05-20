/*
 *  Procedurality Commandline Engine (Interface with Procedurality)
 *  Copyright 2009 Nexis Entertainment
 *
 *  Procedurality.CLI is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2 of the License, or
 *  (at your option) any later version.
 *
 *
 *  Procedurality.CLI is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *
 *  You should have received a copy of the GNU General Public License
 *  along with Procedurality.CLI; if not, write to the Free Software
 *  Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA
 */
using System;
using System.Drawing;
using System.IO;

namespace Procedurality
{

    public class PCMain
    {
        struct Point
        {
            public uint X;
            public uint Y;
        }
        class Size
        {
            public uint Height;
            public uint Width;
            public Size() { }
            public Size(uint H, uint W)
            {
                this.Height = H;
                this.Width = W;
            }
        }
        private static bool IsRunningOnMono()
        {
            return Type.GetType("Mono.Runtime") != null;
        }

        public static void Main(String[] args)
        {
            Console.WriteLine("Procedurality CLI - (c)2009-2013 Rob \"N3X15\" Nelson");
            Console.WriteLine("_______________________________________________________________\n");
            Console.WriteLine("For full licensing information, please see COPYING.");

            Arguments CommandLine = new Arguments(args);

            String file = "in.png";
            String ofile = "out";
            long seed = DateTime.Now.Millisecond;
            Size size = new Size();
            size.Height = 256;
            size.Width = 256;
            bool FlipV = false;
            if (CommandLine["flip-v"] != null)
                FlipV = true;

            // --help
            if (CommandLine["help"] != null)
                Help();

            // --seed=1234567890
            if (CommandLine["seed"] != null)
                seed = long.Parse(CommandLine["seed"]);

            Console.WriteLine(" * Random seed: " + seed.ToString());

            if (CommandLine["size"] != null)
            {
                // --size=256x256
                string[] sc = CommandLine["size"].Split('x');
                if (sc.Length == 1)
                {
                    size.Height = size.Width = uint.Parse(sc[0]);
                }
                else if (sc.Length == 2)
                {
                    size.Height = uint.Parse(sc[0]);
                    size.Width = uint.Parse(sc[1]);
                }
            }

            Channel terrain;
            if (CommandLine["perlin"] != null)
            {
                Console.Write("Generating terrain with perlin noise...");
                terrain = new Mountain((int)size.Height, (int)size.Width, Utils.powerOf2Log2((int)size.Width) - 6, 0.5f, seed).toChannel();
                Console.WriteLine(" DONE!");
            }
            else if (CommandLine["in"] != null)
            {
                Console.Write("Loading " + file + "...");
                terrain = LoadTerrain(CommandLine["in"]);
                if (FlipV) terrain = terrain.flipV();
                Console.WriteLine(" Done!");

                if (CommandLine["tiledir"] != null && CommandLine["tilesize"] != null)
                {
                    string tiledir = CommandLine["tiledir"];
                    if (!Directory.Exists(tiledir))
                        Directory.CreateDirectory(tiledir);
                    MakeTiles(terrain, uint.Parse(CommandLine["tilesize"]), tiledir);
                }
            }
            else if (CommandLine["hills"] != null)
            {
                Console.Write("Generating " + CommandLine["hills"] + " hills...");
                terrain = HillsAlgo((int)size.Height, (int)size.Width, int.Parse(CommandLine["hills"]), (int)seed);
                Console.WriteLine(" DONE!");
            }
            else if (CommandLine["craters"] != null)
            {
                Console.Write("Generating " + CommandLine["craters"] + " craters...");
                terrain = CratersAlgo((int)size.Height, (int)size.Width, int.Parse(CommandLine["craters"]), (int)seed);
                Console.WriteLine(" DONE!");
            }
            else
            {
                Console.WriteLine("Please use --perlin, --in, --hills, or --craters to load some terrain.");
                return;
            }

            ofile = args[args.Length - 1];


            if (CommandLine["addcliffs"] != null)
            {
                int features = 4;
                if (CommandLine["features"] != null)
                    features = int.Parse(CommandLine["features"]);

                Console.WriteLine("Adding cliffs.");
                // add mountain peaks
                Voronoi voronoi = new Voronoi((int)size.Height, (int)size.Width, 4, 4, 1, 1f, seed);
                Channel cliffs = voronoi.getDistance(-1f, 1f, 0f).brightness(1.5f).multiply(0.33f);
                terrain.multiply(0.67f).channelAdd(cliffs);
                terrain.channelSubtract(voronoi.getDistance(1f, 0f, 0f).gamma(.5f).flipV().rotate(90));
            }
            if (CommandLine["iceage"] != null)
            {
                float hills = 0.5f;
                if (CommandLine["hillratio"] != null)
                    hills = float.Parse(CommandLine["hillratio"]);

                Console.WriteLine("Ice age erosion in progress.");
                terrain.perturb(new Midpoint((int)size.Height, (int)size.Width, 2, hills, seed).toChannel(), 0.25f);
            }

            if (CommandLine["thermal"] != null)
            {
                float hills = 0.5f;
                if (CommandLine["hillratio"] != null)
                    hills = float.Parse(CommandLine["hillratio"]);

                terrain.erodeThermal((24f - hills * 12f) / (int)size.Height, (int)size.Height >> 2);
            }

            // Hydraulic Erosion
            if (CommandLine["hydraulic"] != null)
            {
                Console.WriteLine("Hydraulic erosion in progress.");

                // let it rain for a couple of thousand years
                float hills = 0.5f;
                if (CommandLine["hillratio"] != null)
                    hills = float.Parse(CommandLine["hillratio"]);

                terrain.erode((24f - hills * 12f) / (int)size.Height, (int)size.Height >> 2);
            }
            if (CommandLine["slow-hydro"] != null)
            {
                terrain = ErosionHydraulic.erode1(terrain, terrain.copy().multiply(0.01f), 0.1f, 1, 100);
                terrain = ErosionHydraulic.erode2(terrain, terrain.copy().multiply(0.01f), 0.1f, 1, 100);
                terrain = ErosionHydraulic.erode3(terrain, terrain.copy().multiply(0.01f), 0.1f, 1, 100);
                terrain = ErosionHydraulic.erode4(terrain, 0.01f, 0.1f, 1, 100);
                terrain = ErosionHydraulic.erode5(terrain, terrain.copy().multiply(0.01f), 0.1f, 0.1f, 0.1f, 20f, 0.5f, 1, 100);
            }
            if (CommandLine["add-silt"] != null)
            {
                float wl = 20f / 256f;
                wl = float.Parse(CommandLine["add-silt"]);
                terrain = terrain.silt(wl, true);
            }
            /*
			if(CommandLine["add-river"]!=null)
			{
				terrain.normalize(1f,0.3f);
				List<PathFinderNode> path;
				terrain=(new RiverBuilder(terrain)).GenerateRiver(20f,seed,out path).toChannel();
				terrain.normalize();
				string rpath="";
				foreach(PathFinderNode p in path)
				{
					rpath+=string.Format("{0},{1}\n",p.X,p.Y);
				}
				File.WriteAllText(ofile+".river.txt",rpath);
			}
            */
            terrain.smooth(1);

            float nm = 0f;
            float nM = 1f;
            if (CommandLine["range-max"] != null)
                nM = float.Parse(CommandLine["range-max"]);

            if (CommandLine["range-min"] != null)
                nm = float.Parse(CommandLine["range-min"]);

            if (nm > 1f || nM > 1f)
            {
                nm = nm / 256f;
                nM = nM / 256f;
            }

            terrain.normalize(nm, nM).toLayer().saveAsPNG(ofile);
        }

        private static void Help()
        {
            /*
            Procedurality CLI - (c)2009 Rob "N3X15" Nelson
            _______________________________________________________________

            Usage help:

                Procedurality.exe [--help] [--hydraulic] [--thermal] [--iceage] 
                 [--addcliffs] [--seed=[#]] [--addcrater=x,y] [--addhill=x,y]
                 [--perlin] [--hills=#OfHills] [--craters=#OfCraters] [--features=#]
                 [--hillratio=#.#] [--maxheight=#.#] [--maxdepth=#.#] [--size=#]
                 [--in=InputFile.png] OutputFile.png
            */
            Console.WriteLine("Usage help:\n");
            Console.WriteLine("\tProcedurality.exe [--help] [--erode=(hydraulic|thermal|iceage)]");
            Console.WriteLine("\t  [--addcliffs] [--seed=[#]] [--addcrater=x,y] [--addhill=x,y]");
            Console.WriteLine("\t  [--perlin] [--hills=#OfHills] [--craters=#OfCraters] [--features=#]");
            Console.WriteLine("\t  [--hillratio=#.#] [--maxheight=#.#] [--maxdepth=#.#] [--size=#[x#]]");
            Console.WriteLine("\t  [--in=InputFile.png] OutputFile.png");

        }

        public static Channel LoadTerrain(String file)
        {
            using (FileStream stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                Bitmap bitmap = new Bitmap(stream);
                try
                {
                    Channel terrain = new Channel(bitmap.Width, bitmap.Height);
                    Console.WriteLine("LOADED {0}x{1} BITMAP!", bitmap.Height, bitmap.Width);
                    for (int x = 0; x < bitmap.Width; x++)
                    {
                        for (int y = 0; y < bitmap.Height; y++)
                        {
                            terrain.putPixel(x, y, bitmap.GetPixel(x, y).GetBrightness());
                        }
                    }
                    Console.WriteLine("LOADED {0}x{1} CHANNEL!", terrain.getHeight(), terrain.getWidth());
                    return terrain;//.invert();
                }
                catch (IOException)
                {
                    Console.WriteLine("Cannot find " + file + ", using blank channel.");
                    return new Channel(256, 256);
                }
            }
        }

        public static String RemoveExtension(String s)
        {
            String separator = ".";
            String filename;

            // Remove the path upto the filename.
            int lastSeparatorIndex = s.LastIndexOf(separator);
            if (lastSeparatorIndex == -1)
            {
                filename = s;
            }
            else
            {
                filename = s.Substring(lastSeparatorIndex + 1);
            }

            // Remove the extension.
            int extensionIndex = filename.LastIndexOf(".");
            if (extensionIndex == -1)
                return filename;

            return filename.Substring(0, extensionIndex);
        }

        public static Channel HillsAlgo(int sizeX, int sizeY, int numHills, int seed)
        {
            Console.WriteLine();
            Channel chan = new Channel(sizeX, sizeY);
            Random rand = new Random(seed);
            for (int i = 0; i < numHills; )
            {
                int x = rand.Next(0, sizeX);
                int y = rand.Next(0, sizeY);

                double radius = ((rand.NextDouble() * 84.0) - 20);
                Channel crater = (new Hill(sizeX, sizeY, x, y, (float)radius)).toChannel();
                if (crater.findMax() != 1.0)
                {
                    continue;
                }
                i++;
                drawTextProgressBar(i, numHills);
                //crater.toLayer().saveAsPNG("../sims/crater_debug.png");
                chan.channelAdd(crater.normalize(0f, 0.01f));
            }
            chan.normalize();
            Console.WriteLine("\nRange [{0},{1}]", chan.findMin(), chan.findMax());
            return chan;
        }

        Point Global2Local(uint x, uint y, Point corner)
        {
            Point p = new Point();
            p.X = x - corner.X;
            p.Y = y - corner.Y;
            return p;
        }

        Point Local2Global(uint x, uint y, Point corner)
        {
            Point p = new Point();
            p.X = x + corner.X;
            p.Y = y + corner.Y;
            return p;
        }

        public static Channel CratersAlgo(int sizeX, int sizeY, int numCraters, int seed)
        {
            Channel chan = new Channel(sizeX, sizeY);
            Console.WriteLine();
            Random rand = new Random(seed);
            for (int i = 0; i < numCraters; )
            {
                int x = rand.Next(0, sizeX);
                int y = rand.Next(0, sizeY);
                double radius = (rand.NextDouble() * 84.0) - 20;

                // Clamp
                //x=(x<0) ? 0 : ((x>size-1) ? size-1 : x);
                //x=(y<0) ? 0 : ((y>size-1) ? size-1 : y);
                //radius=(radius<20.0) ? 20.0 : ((radius>84.0) ? 84.0 : radius);

                Channel crater = (new Crater(sizeX, sizeY, x, y, radius)).toChannel();
                if (crater.findMax() != 1.0)
                {
                    continue;
                }
                //if(crater.findMin()!=0.0)
                //{
                //	Console.Write("!");
                //	continue;
                //}
                i++;
                drawTextProgressBar(i, numCraters);
                //crater.toLayer().saveAsPNG("../sims/crater_debug.png");
                chan.channelAdd(crater.normalize(-0.01f, 0.01f));
            }
            chan.normalize();
            Console.WriteLine("\nRange [{0},{1}]", chan.findMin(), chan.findMax());
            return chan;
        }

        /// <summary>
        /// Draw a progress bar at the current cursor position.
        /// Be careful not to Console.WriteLine or anything whilst using this to show progress!
        /// </summary>
        /// <param name="progress">The position of the bar</param>
        /// <param name="total">The amount it counts</param>

        private static void drawTextProgressBar(int progress, int total)
        {
            //draw empty progress bar
            Console.CursorLeft = 0;
            Console.Write("["); //start
            Console.CursorLeft = 32;
            Console.Write("]"); //end
            Console.CursorLeft = 1;
            float onechunk = 30.0f / total;

            //draw filled part
            int position = 1;
            for (int i = 0; i < onechunk * progress; i++)
            {
                //	Console.BackgroundColor = ConsoleColor.Gray;
                Console.CursorLeft = position++;
                Console.Write("=");
            }

            //draw unfilled part
            for (int i = position; i <= 31; i++)
            {
                //	Console.BackgroundColor = ConsoleColor.Black;
                Console.CursorLeft = position++;
                Console.Write(" ");
            }

            //draw totals
            Console.CursorLeft = 35;
            //Console.BackgroundColor = ConsoleColor.Black;
            Console.Write(progress.ToString() + " of " + total.ToString() + "    "); //blanks at the end remove any excess
        }

        private static void MakeTiles(Channel terrain, uint tsize, string path)
        {
            uint xmax = (uint)Math.Floor((float)terrain.Width / (float)tsize);
            uint ymax = (uint)Math.Floor((float)terrain.Height / (float)tsize);
            uint complete = 0;
            for (uint y = 0; y < ymax; y++)
            {
                for (uint x = 0; x < xmax; x++)
                {
                    Channel tile = new Channel((int)tsize, (int)tsize);
                    tile = terrain.copy().crop((int)(x * tsize), (int)(y * tsize), (int)((x + 1) * tsize), (int)((y + 1) * tsize));
                    tile.toLayer().saveAsPNG(string.Format("{0}/SimTerrain-{1}-{2}.png"));
                    complete++;
                    drawTextProgressBar((int)complete, (int)(xmax + ymax));
                }
            }
        }
    }
}