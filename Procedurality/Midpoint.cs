/*
 *  Procedurality v. 0.1 rev. 20070307
 *  Copyright 2007 Oddlabs ApS
 *
 *
 *  This file is part of Procedurality.
 *  Procedurality is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2 of the License, or
 *  (at your option) any later version.
 *
 *
 *  Procedurality is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *
 *  You should have received a copy of the GNU General Public License
 *  along with Foobar; if not, write to the Free Software
 *  Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA
 */

using System;

namespace Procedurality
{
	public class Midpoint {
	
		private Random random;
		public Channel channel;

        public Midpoint(int sizeX, int sizeY, int base_freq, float pers, long seed)
        {
            if (!Utils.isPowerOf2(sizeX))
                throw new Exception("sizeX must be power of 2");
            if (!Utils.isPowerOf2(sizeY))
                throw new Exception("sizeY must be power of 2");
            int iterations = Math.Max(Utils.powerOf2Log2(sizeX),Utils.powerOf2Log2(sizeY));
            int sizeMax = Math.Max(sizeX, sizeY);
			base_freq = Math.Max(base_freq, 0);
			base_freq = Math.Min(base_freq, iterations);
			random = new Random((int)seed);
			channel = new Channel(sizeX, sizeY);
	
			
			int x_block, y_block, x, y;
			
			if (base_freq > 0) {
                int block_size = sizeMax >> base_freq;
				for (x_block = 0; x_block < (1<<base_freq); x_block++) {
					for (y_block = 0; y_block < (1<<base_freq); y_block++) {
						x = x_block*block_size;
						y = y_block*block_size;
						channel.putPixel(x, y, (float)random.NextDouble());
					}
				}
			}
	
			float v1, v2, v3, v4, v5, v6, v7, v8, v9;
	
			for (int i = base_freq; i < iterations; i++) {
                int block_size = sizeMax >> i;
                int block_size_half = sizeMax >> (i + 1);
				float amp = (float)Math.Pow(pers, i - base_freq);
				float amp_half = 0.5f*amp;
				// calculate center midpoints
				if (i < 2) {
					for (x_block = 0, x = 0; x_block < (1<<i); x_block++) {
						for (y_block = 0, y = 0; y_block < (1<<i); y_block++) {
							v1 = channel.getPixel(x, y);
                            v2 = channel.getPixel((x + block_size) % sizeX, y);
                            v3 = channel.getPixel(x, (y + block_size) % sizeY);
                            v4 = channel.getPixel((x + block_size) % sizeX, (y + block_size) % sizeY);
							v5 = 0.25f*(v1 + v2 + v3 + v4) + (float)random.NextDouble()*amp - amp_half;
							channel.putPixel(x + block_size_half, y + block_size_half, v5);
							y+= block_size;
						}
						x+= block_size;
					}
				} else {
					// safe blocks
					for (x_block = 1, x = block_size; x_block < (1<<i) - 1; x_block++) {
						for (y_block = 1, y = block_size; y_block < (1<<i) - 1; y_block++) {
							v1 = channel.getPixel(x, y);
							v2 = channel.getPixel(x + block_size, y);
							v3 = channel.getPixel(x, y + block_size);
							v4 = channel.getPixel(x + block_size, y + block_size);
							v5 = 0.25f*(v1 + v2 + v3 + v4) + (float)random.NextDouble()*amp - amp_half;
							channel.putPixel(x + block_size_half, y + block_size_half, v5);
							y+= block_size;
						}
						x+= block_size;
					}
					// left and right edge blocks
					for (x_block = 0; x_block < (1<<i); x_block+= (1<<i) - 1) {
						x = x_block*block_size;
						for (y_block = 0, y = 0; y_block < (1<<i); y_block++) {
							v1 = channel.getPixel(x, y);
                            v2 = channel.getPixel((x + block_size) % sizeX, y);
                            v3 = channel.getPixel(x, (y + block_size) % sizeY);
                            v4 = channel.getPixel((x + block_size) % sizeX, (y + block_size) % sizeY);
							v5 = 0.25f*(v1 + v2 + v3 + v4) + (float)random.NextDouble()*amp - amp_half;
							channel.putPixel(x + block_size_half, y + block_size_half, v5);
							y+= block_size;
						}
					}
					// top and bottom edge blocks
					for (x_block = 1, x = block_size; x_block < (1<<i) - 1; x_block++) {
						for (y_block = 0; y_block < (1<<i); y_block+= (1<<i) - 1) {
							y = y_block*block_size;
							v1 = channel.getPixel(x, y);
                            v2 = channel.getPixel((x + block_size) % sizeX, y);
                            v3 = channel.getPixel(x, (y + block_size) % sizeY);
                            v4 = channel.getPixel((x + block_size) % sizeX, (y + block_size) % sizeY);
							v5 = 0.25f*(v1 + v2 + v3 + v4) + (float)random.NextDouble()*amp - amp_half;
							channel.putPixel(x + block_size_half, y + block_size_half, v5);
						}
						x+= block_size;
					}
				}
				// calculate left and bottom edge midpoints
				if (i < 2) {
					for (x_block = 0, x = 0; x_block < (1<<i); x_block++) {
						for (y_block = 0, y = 0; y_block < (1<<i); y_block++) {
							v1 = channel.getPixel(x, y);
							v5 = channel.getPixel(x + block_size_half, y + block_size_half);
                            v2 = channel.getPixel((x + block_size) % sizeX, y);
                            v3 = channel.getPixel(x, (y + block_size) % sizeY);
                            v6 = channel.getPixel(((x - block_size_half) + sizeX) % sizeX, (y + block_size_half) % sizeY);
                            v7 = channel.getPixel((x + block_size_half) % sizeX, ((y - block_size_half) + sizeY) % sizeY);
							v8 = 0.25f*(v1 + v3 + v5 + v6) + (float)random.NextDouble()*amp - amp_half;
							v9 = 0.25f*(v1 + v2 + v5 + v7) + (float)random.NextDouble()*amp - amp_half;
							channel.putPixel(x, y + block_size_half, v8);
							channel.putPixel(x + block_size_half, y, v9);
							y+= block_size;
						}
						x+= block_size;
					}
				} else {
					// safe blocks
					for (x_block = 1, x = block_size; x_block < (1<<i) - 1; x_block++) {
						for (y_block = 1, y = block_size; y_block < (1<<i) - 1; y_block++) {
							v1 = channel.getPixel(x, y);
							v5 = channel.getPixel(x + block_size_half, y + block_size_half);
							v2 = channel.getPixel(x + block_size, y);
							v3 = channel.getPixel(x, y + block_size);
							v6 = channel.getPixel(x - block_size_half, y + block_size_half);
							v7 = channel.getPixel(x + block_size_half, y - block_size_half);
							v8 = 0.25f*(v1 + v3 + v5 + v6) + (float)random.NextDouble()*amp - amp_half;
							v9 = 0.25f*(v1 + v2 + v5 + v7) + (float)random.NextDouble()*amp - amp_half;
							channel.putPixel(x, y + block_size_half, v8);
							channel.putPixel(x + block_size_half, y, v9);
							y+= block_size;
						}
						x+= block_size;
					}
					// left and right edge blocks
					for (x_block = 0; x_block < (1<<i); x_block+= (1<<i) - 1) {
						x = x_block*block_size;
						for (y_block = 0, y = 0; y_block < (1<<i); y_block++) {
							v1 = channel.getPixel(x, y);
							v5 = channel.getPixel(x + block_size_half, y + block_size_half);
                            v2 = channel.getPixel((x + block_size) % sizeX, y);
                            v3 = channel.getPixel(x, (y + block_size) % sizeY);
                            v6 = channel.getPixel(((x - block_size_half) + sizeX) % sizeX, (y + block_size_half) % sizeY);
                            v7 = channel.getPixel((x + block_size_half) % sizeX, ((y - block_size_half) + sizeY) % sizeY);
							v8 = 0.25f*(v1 + v3 + v5 + v6) + (float)random.NextDouble()*amp - amp_half;
							v9 = 0.25f*(v1 + v2 + v5 + v7) + (float)random.NextDouble()*amp - amp_half;
							channel.putPixel(x, y + block_size_half, v8);
							channel.putPixel(x + block_size_half, y, v9);
							y+= block_size;
						}
					}
					// top and bottom edge blocks
					for (x_block = 1, x = block_size; x_block < (1<<i) - 1; x_block++) {
						for (y_block = 0; y_block < (1<<i); y_block+= (1<<i) - 1) {
							y = y_block*block_size;
							v1 = channel.getPixel(x, y);
							v5 = channel.getPixel(x + block_size_half, y + block_size_half);
                            v2 = channel.getPixel((x + block_size) % sizeX, y);
                            v3 = channel.getPixel(x, (y + block_size) % sizeY);
                            v6 = channel.getPixel(((x - block_size_half) + sizeX) % sizeX, (y + block_size_half) % sizeY);
                            v7 = channel.getPixel((x + block_size_half) % sizeX, ((y - block_size_half) + sizeY) % sizeY);
							v8 = 0.25f*(v1 + v3 + v5 + v6) + (float)random.NextDouble()*amp - amp_half;
							v9 = 0.25f*(v1 + v2 + v5 + v7) + (float)random.NextDouble()*amp - amp_half;
							channel.putPixel(x, y + block_size_half, v8);
							channel.putPixel(x + block_size_half, y, v9);
						}
						x+= block_size;
					}
				}
			}
			channel.normalize();
		}
	
		public Layer toLayer() {
			return new Layer(channel, channel.copy(), channel.copy());
		}
	
		public Channel toChannel() {
			return channel;
		}
	
	}
}