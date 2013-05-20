/*
 *  Procedurality4NET Craters
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
    /// Craters "Algorithm"
    /// </summary>
	public class Crater
	{
		private Channel channel;

        /// <summary>
        /// Create a new crater.
        /// </summary>
        /// <param name="size">Size of the channel</param>
        /// <param name="xp">X position</param>
        /// <param name="yp">Y position</param>
        /// <param name="radius">Radius, sans falloff radius</param>
		public Crater(int sizeX,int sizeY, int xp,int yp,double radius)
		{
			channel = new Channel(sizeX,sizeY);
			double ratio=1f;
			double craterdepth = 10.0f*ratio;
			double rimheight = 3.0f*ratio;
			double falloff = 60.0f*ratio;
			
			BuildCrater(xp,yp,radius,rimheight,craterdepth,falloff);
		}
		
		public void BuildCrater(int x, int y, double radius, double rimheight, double craterdepth, double falloff)
		{	
			// 128-28 = 100
			int minX=x-(int)Math.Round(radius+falloff);
			int minY=y-(int)Math.Round(radius+falloff);
			// 128+28 = 156
			int maxX=x+(int)Math.Round(radius+falloff);
			int maxY=y+(int)Math.Round(radius+falloff);
			
			// Clamp 
			minX=(minX<0) ? 0 : minX;
			minY=(minY<0) ? 0 : minY;
			maxX=(maxX>256) ? 256 : maxX;
			maxY=(maxY>256) ? 256 : maxY;
			
			//Console.WriteLine(" * Building Crater @ ({0},{1}), BB @ [({2},{3}),({4},{5})]",x,y,minX,minY,maxX,maxY);
			for(int i=minX;i<maxX;i++)
			{
				for(int j=minY;j<maxY;j++)
				{
					double dx = (double)(x-i);
					double dy = (double)(y-j);
					double dist = Math.Sqrt(dx*dx + dy*dy);
					double height=0.0d;
					if(dist<radius){
						height = 1.0d-((dx*dx + dy*dy) / (radius*radius));
						height = rimheight - (height*craterdepth);
					} else if((dist-radius) < falloff) {
						double fallscale = (dist-radius)/falloff;
						height = (1.0d-fallscale) * rimheight;
					}else{
						height = 0.0d;
					}
					if(height > 1d) height = 1d;
					if(height <-1d) height =-1d;
					
					channel.putPixel(i,j,(float)height);
				}
			}
			channel.normalize();
		}
		
		public Channel toChannel()
		{
			return channel;
		}
		public Layer toLayer() {
			return new Layer(channel.copy(), channel.copy(), channel.copy());
		}
	}
}
