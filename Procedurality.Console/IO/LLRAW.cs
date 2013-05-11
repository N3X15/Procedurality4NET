/*
 * Copyright (c) Contributors, http://opensimulator.org/
 * See CONTRIBUTORS.TXT for a full list of copyright holders.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *     * Redistributions of source code must retain the above copyright
 *       notice, this list of conditions and the following disclaimer.
 *     * Redistributions in binary form must reproduce the above copyright
 *       notice, this list of conditions and the following disclaimer in the
 *       documentation and/or other materials provided with the distribution.
 *     * Neither the name of the OpenSimulator Project nor the
 *       names of its contributors may be used to endorse or promote products
 *       derived from this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE DEVELOPERS ``AS IS'' AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE CONTRIBUTORS BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.IO;
using Procedurality;

namespace Procedurality.IO
{
    public class LLRAW
    {
        public struct HeightmapLookupValue : IComparable<HeightmapLookupValue>
        {
            public int Index;
            public float Value;

            public HeightmapLookupValue(int index, float value)
            {
                Index = index;
                Value = value;
            }

            public int CompareTo(HeightmapLookupValue val)
            {
                return Value.CompareTo(val.Value);
            }
        }

        /// <summary>Lookup table to speed up terrain exports</summary>
        HeightmapLookupValue[] LookupHeightTable;

        public LLRAW()
        {
            LookupHeightTable = new HeightmapLookupValue[256 * 256];

            for (int i = 0; i < 256; i++)
            {
                for (int j = 0; j < 256; j++)
                {
                    LookupHeightTable[i + (j * 256)] = new HeightmapLookupValue(i + (j * 256), ((float)i * ((float)j / 128.0f)));
                }
            }
            Array.Sort<HeightmapLookupValue>(LookupHeightTable);
        }

        #region ITerrainLoader Members

        public Channel LoadFile(string filename)
        {
            FileInfo file = new FileInfo(filename);
            FileStream s = file.Open(FileMode.Open, FileAccess.Read);
            Channel retval = LoadStream(s);

            s.Close();

            return retval;
        }

        public Channel LoadFile(string filename, int offsetX, int offsetY, int fileWidth, int fileHeight, int sectionWidth, int sectionHeight)
        {
            Channel retval = new Channel(sectionWidth, sectionHeight);

            FileInfo file = new FileInfo(filename);
            FileStream s = file.Open(FileMode.Open, FileAccess.Read);
            BinaryReader bs = new BinaryReader(s);

            int currFileYOffset = fileHeight - 1;

            // if our region isn't on the first Y section of the areas to be landscaped, then
            // advance to our section of the file
            while (currFileYOffset > offsetY)
            {
                // read a whole strip of regions
                int heightsToRead = sectionHeight * (fileWidth * sectionWidth);
                bs.ReadBytes(heightsToRead * 13); // because there are 13 fun channels
                currFileYOffset--;
            }

            // got to the Y start offset within the file of our region
            // so read the file bits associated with our region
            int y;
            // for each Y within our Y offset
            for (y = sectionHeight - 1; y >= 0; y--)
            {
                int currFileXOffset = 0;

                // if our region isn't the first X section of the areas to be landscaped, then
                // advance the stream to the X start pos of our section in the file
                // i.e. eat X upto where we start
                while (currFileXOffset < offsetX)
                {
                    bs.ReadBytes(sectionWidth * 13);
                    currFileXOffset++;
                }

                // got to our X offset, so write our regions X line
                int x;
                for (x = 0; x < sectionWidth; x++)
                {
                    // Read a strip and continue
                    retval.putPixel(x, y, bs.ReadByte() * (bs.ReadByte() / 128.0f));
                    bs.ReadBytes(11);
                }
                // record that we wrote it
                currFileXOffset++;

                // if our region isn't the last X section of the areas to be landscaped, then
                // advance the stream to the end of this Y column
                while (currFileXOffset < fileWidth)
                {
                    // eat the next regions x line
                    bs.ReadBytes(sectionWidth * 13); //The 13 channels again
                    currFileXOffset++;
                }
            }

            bs.Close();
            s.Close();

			float maxh,minh;
			minh=retval.findMin()/256f;
			maxh=retval.findMax()/256f;
            return retval.normalize(minh,maxh);
        }

        public Channel LoadStream(Stream s)
        {
            Channel retval = new Channel(256,256);

            BinaryReader bs = new BinaryReader(s);
            int y;
            for (y = 0; y < retval.getHeight(); y++)
            {
                int x;
                for (x = 0; x < retval.getWidth(); x++)
                {
                    retval.putPixel(x, (retval.getHeight() - 1) - y, bs.ReadByte() * (bs.ReadByte() / 128.0f));
                    bs.ReadBytes(11); // Advance the stream to next bytes.
                }
            }

            bs.Close();
			
			float maxh,minh;
			minh=retval.findMin()/512f;
			maxh=retval.findMax()/512f;
            return retval.normalize(minh,maxh);
        }

        public void SaveFile(string filename, Channel map)
        {
            FileInfo file = new FileInfo(filename);
            FileStream s = file.Open(FileMode.CreateNew, FileAccess.Write);
            SaveStream(s, map);

            s.Close();
        }

        public void SaveStream(Stream s, Channel map)
        {
            BinaryWriter binStream = new BinaryWriter(s);

            // Output the calculated raw
            for (int y = 0; y < map.Height; y++)
            {
                for (int x = 0; x < map.Width; x++)
                {
                    float t = map.getPixel(x, (map.getHeight() - 1) - y)*512f;
                    //if height is less than 0, set it to 0 as
                    //can't save -ve values in a LLRAW file
                    if (t < 0f)
                    {
                        t = 0f;
                    }

                    int index = 0;

                    // The lookup table is pre-sorted, so we either find an exact match or
                    // the next closest (smaller) match with a binary search
                    index = Array.BinarySearch<HeightmapLookupValue>(LookupHeightTable, new HeightmapLookupValue(0, t));
                    if (index < 0)
                        index = ~index - 1;

                    index = LookupHeightTable[index].Index;

                    byte red = (byte) (index & 0xFF);
                    byte green = (byte) ((index >> 8) & 0xFF);
                    const byte blue = 20;
                    const byte alpha1 = 0;
                    const byte alpha2 = 0;
                    const byte alpha3 = 0;
                    const byte alpha4 = 0;
                    const byte alpha5 = 255;
                    const byte alpha6 = 255;
                    const byte alpha7 = 255;
                    const byte alpha8 = 255;
                    byte alpha9 = red;
                    byte alpha10 = green;

                    binStream.Write(red);
                    binStream.Write(green);
                    binStream.Write(blue);
                    binStream.Write(alpha1);
                    binStream.Write(alpha2);
                    binStream.Write(alpha3);
                    binStream.Write(alpha4);
                    binStream.Write(alpha5);
                    binStream.Write(alpha6);
                    binStream.Write(alpha7);
                    binStream.Write(alpha8);
                    binStream.Write(alpha9);
                    binStream.Write(alpha10);
                }
            }

            binStream.Close();
        }

        public string FileExtension
        {
            get { return ".raw"; }
        }

        #endregion

        public override string ToString()
        {
            return "LL/SL RAW";
        }
    }
}
