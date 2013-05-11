/*
 *  Procedurality4NET Hills
 *  Copyright 2013 Rob "N3X15" Nelson <nexisentertainment@gmail.com>
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
    /// <summary>
    /// Hill "Algorithm"
    /// </summary>
	public class Hill
	{
		private Channel channel;

        /// <summary>
        /// Create a hill positioned on a channel.
        /// </summary>
        /// <param name="size">Size of the channel in pixels</param>
        /// <param name="xp">X position of hill center</param>
        /// <param name="yp">Y position of hill center</param>
        /// <param name="radius">Radius of the hill</param>
		public Hill(int size,int xp,int yp,float radius)
		{
			double r=(double)radius;
			channel = new Channel(size,size);
			double hill_height=5.0d*(radius/40.0d);
			for(int x=0;x<size;x++)
			{
				for(int y=0;y<size;y++)
				{
					double dx = (double)(yp-x);
					double dy = (double)(xp-y);
					double dist = Math.Sqrt(dx*dx + dy*dy);
					if(dist<radius)
					{
						double height = 1.0d-((dx*dx + dy*dy) / (r*r));
						height = (height*hill_height);
						channel.putPixel(x,y,Convert.ToSingle(height));//$radius^2 + (($x-$hx)^2 - ($y-(256-$hy))^2);
					}
					//channel.putPixel(x,y,(radius*radius) + (((x-dx)*(x-dx)) - ((y-dy)*(y-dy))));
				}
			}
			channel.normalize();
		}
		
		public Channel toChannel()
		{
			return channel;
		}
		public Layer toLayer() {
			return new Layer(channel, channel.copy(), channel.copy());
		}
	}
}
