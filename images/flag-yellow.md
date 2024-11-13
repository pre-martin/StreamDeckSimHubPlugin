# Creation of icons

Svg.Skia has two problems with the text:

- The text is not aligned correctly (baseline)
- The embedded font is ignored

Thus, we use inkscape to create PNG files:

    inkscape -w 72 -h 72 images\flag-yellow-s1.svg -o StreamDeckSimHub.Plugin\images\custom\flags\flag-yellow-s1.png
    inkscape -w 72 -h 72 images\flag-yellow-s2.svg -o StreamDeckSimHub.Plugin\images\custom\flags\flag-yellow-s2.png
    inkscape -w 72 -h 72 images\flag-yellow-s3.svg -o StreamDeckSimHub.Plugin\images\custom\flags\flag-yellow-s3.png

    inkscape -w 144 -h 144 images\flag-yellow-s1.svg -o StreamDeckSimHub.Plugin\images\custom\flags\flag-yellow-s1@2x.png
    inkscape -w 144 -h 144 images\flag-yellow-s2.svg -o StreamDeckSimHub.Plugin\images\custom\flags\flag-yellow-s2@2x.png
    inkscape -w 144 -h 144 images\flag-yellow-s3.svg -o StreamDeckSimHub.Plugin\images\custom\flags\flag-yellow-s3@2x.png
