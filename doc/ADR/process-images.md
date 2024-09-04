# How to process images

## Status

Accepted (2024-09-04)

## Context

We have to load images from the file system and send them to the Stream Deck. Additionally, we want to modify these images - e.g. write some text (SimHub values) over the image. Therefore, we need a library to read, decode, modify and encode images.

Requirements:
- Support SVG at least for loading (because Elgato recommends to use SVG images, which makes sense)
- Support animated GIF files. Stream Deck supports animated GIFs. It would be awesome if we could display animated GIFs and print some text/values on top of it.

Candidates are:
- SkiaSharp
  - MIT license 
  - Cross-platform 
  - Support of SVG via Svg.Skia
  - No support for GIF at all
- SixLabors.ImageSharp
  - Apache License for our project, otherwise a commercial license 
  - Cross-platform
  - No support for SVG
  - Supports reading and encoding of animated GIFs
- System.Drawing
  - MIT license
  - Platform-dependent
  - No support for SVG
  - Supports reading of animated GIFs, but not encoding

Issues:
- As of 2024-09, the Stream Deck does not support animated GIFs, when they are sent from a plugin. Only "Set [Icon] from File" does support animated GIFs.


## Decision

We use SixLabors.ImageSharp, because it has a lean and simple API. And because it supports decoding and encoding of GIFs.

In order to support SVG, we leave Svg.Skia and its dependency SkiaSharp in the project. But SkiaSharp should not be used directly any more.


## Consequences

There will be two image processing libraries in the project. Although SkiaSharp is available for imports, it should not be used directly.
