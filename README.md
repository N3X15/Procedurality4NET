Procedurality4NET v. 0.1
====================

Original Procedurality library Copyright 2007 Oddlabs ApS, authored by Jacob Olsen, jacob@oddlabs.com. See http://oddlabs.com/procedurality/ for more information.
Converted to C# and modified by Rob "N3X15" Nelson, nexisentertainment@gmail.com

Procedurality is free software; you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation; either version 2 of the License, or
(at your option) any later version.

Procedurality is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Foobar; if not, write to the Free Software
Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA


What's this?
------------

The Procedurality engine is a spin-off from the development of the cross
platform RTS game Tribal Trouble (see http://tribaltrouble.com/). Like Tribal
Trouble it was written in Java, and some of the tricks it can do has been
described in the paper "Realtime Procedural Terrain Generation" which can be
found on the homepage http://oddlabs.com/procedurality/

Procedurality is made up of two parts: A set of classes that generate various
types of graphics (random noise maps etc.) and a complete image manipulation
library that supports color channels, layers with alpha blending and most of
the functions and filters you would expect to find in software like GIMP (as
well as a few more suited for generating terrain height maps).

Around 2008 or -9, a bored freelance developer converted this wonderful library to C#
and made several modifications for his own purposes.  These changes were never 
intended to be publically released, but things changed, I got employed, and found a 
use for it in a commercial project.  So, this repository will (slowly) be cleaned up.

Requirements
------------

* .NET 4.0 or compatible runtime

Usage
-----

* Prepare to compile using Prebuild*.(sh|bat)
* Compile using your favorite IDE or nant.

Take a look at the Example* classes to see what is going on!


Documentation
-------------

At this point, documentation is almost non-existing. For a general idea about
what this software can do, see the technical paper "Realtime Procedural Terrain
Generation" which can be found on the homepage (see above).

Here is a quick overview of the classes:

Analyzer
Calculates some statistics and graphs for heightmap images

Cellular
Older but more general implementation of Voronoi - generates various types of
Voronoi diagrams

Channel
Internal greyscale image format - contains many image manipulation methods

ErosionHydraulic
Implementations of hydraulic erosion algorithms - they are generally slow and
produce uninteresting effects

ErosionThermal
Implementation of thermal erosion

ErosionVelocity
Experimental (never finished) implementations of erosion based on velocity maps
- pretty useless in their current state

ExampleAnalyzer
Example of how to use the image analysis toolkit

ExampleGrass
Example of how to use the grass texture generator

ExampleHydraulic
Example of how to use the hydraulic erosion algorithm

ExampleTerrain
Example of how to use the library to generate eroded terrain suitable for
computer games

ExampleTexture
Example of how to generate a rock-like texture using noise, Voronoi maps and
image manipulation

GLImage
Support class

GLIntImage
Support class

Hill
Generates simple smooth shapes

Layer
Internal RGB(A) image format - contains many image manipulation methods

MidpointFractalizer
Adds midpoint displacement noise to existing greyscale image

MidpointGaussian
Generates midpoint displacement noise using gaussian distribution of new values
(slow)

Midpoint
Generates midpoint displacement noise using uniform distribution of new values
(fast)

Mountain
Stripped-down version of Midpoint (not sure if there's any speed optimization
involved)

Perlin
Older but more general implementation of spectral synthesis noise (actually,
this is NOT Perlin noise!)

Spectral
Generates spectral synthesis noise (slower than Midpoint, but more flexible
wrt. modifications)

Tools
Interpolation methods and other math stuff used almost everywhere

Utils
Support class

Voronoi
Generates various types of Voronoi diagrams

