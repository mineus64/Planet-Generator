# Planet Generator

A Unity package for generating planets using Perlin Noise given a set of sample textures.

## Installation

Just import these items into your project, no muss no fuss.

## Prerequisites

None that I am aware of.

## How to Use

This package comes with a set of pre-generated textures that you can just slap onto your planets and use to get going. Alternatively, you can slap the Planet Generator script onto one of your planets and set up the settings how you like. Feel free to feed in the planet seed from wherever.

This script will sample from Perlin noise to create a heightmap, and will then pull from sample textures based on the height and latitude of each given point on the object's surface to create a texture for it. In higher quality settings it will also generate additional maps for temperature and rainfall and use those to blend multiple different textures to create a more vibrant and realistic world. At the highest quality setting it will also generate a normalmap to make your planets look even nicer.

## How This Works

This tool uses Perlin noise to sample a set of values for each pixel on the map: Height, Temperature, and Rainfall. The tool takes in an array of sample textures which are used to determine the colour of each pixel in the final texture. Each sample texture represents a single biome, and is coloured based on latitude (increasing from left to right) and height (increasing from top to bottom). Temperature values are broken into two values: High and Low, while Humidity values are broken into three: High, Medium, and Low. These values are used to select which texture is sampled, like so:
0: High Temperature, high Humidity: Tropics
1: High Temperature, medium Humidity: Savannah
2: High Temperature, low Humidity: Desert
3: Low Temperature, high Humidity: Swamp
4: Low Temperature, medium Humidity: Temperate
5: Low Temperature, low Humidity: Tundra
From there, a texture is generated. You can optionally use the heightmap values to deform your sphere if you'd like. This tool can also generate a normalmap to increase the perceived detail of your planet.

## Settings

### General Settings

**Seed:** The seed used for heightmap generation

**Resolution:** The resolution of the output texture. This is how many pixels wide it will be, the height of the texture will be half of this (ie. a Resolution of 1024 produces a 1024x512px texture)

**Texture Quality:** This is a general quality setting stored as an enum that is used to determine the amount of oompf that goes into making textures. Settings are High, Medium, and Low. Medium is the standard setting, in Low the planet only uses a single sample for the entire texture and does not do anything with biomes, and in High the generator will also generate a normalmap.

### Noise Settings

**Sea Level:** This is added to the height of the output points, which is clamped to between 0 and 1. This reduces the altitude of all of the points on the map, effectively increasing the sea level and reducing the amount of mountains.

**Roughness:** This spaces the sample points further apart to create a rougher planet.

**Centre:** The centre of the noise. This allows the planet's noise values to be offset if need be.

### Biome Settings

**Temperature:** A modifier to vary the overall temperature of the planet. This is added to the values in the temperature map, which are clamped to between 0 and 1. Lower values mean a colder planet. Higher values mean a hotter planet.

**Rainfall:** A modifier to vary the overall rainfall on the planet. This is added to the values in the rainfall map, which are clamped to between 0 and 1. Lower values mean a drier, more arid planet. Higher values mean a wetter, more humid planet.

**Variation:** This spaces the sample points for the biome noise out further, which creates wilder and more varied temperature and rainfall maps.

### Texture Generation

**Sample Textures:** The array of six different sample textures to use to create a basic biome map.

**PlanetTex:** This stores the RGB texture of the planet.

**Normalmap:** This stores the planet's normalmap.

Note that there is an if block that limits this script to only working in client builds and the Editor. This is to save performance in server builds which don't need to generate a full texture for every planet (it's a clientside thing).