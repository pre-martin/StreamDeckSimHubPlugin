# Creation of on/off icon

- Image 144 x 144 with transparency
- new layer
- select tool - ellipse select
  - position 37 / 37, size 70 x 70
- Menu "Select" - "To Path"
- Set color to white
- Menu "Edit" - "Stroke Path"
  - Solid color
  - Anti aliasing
  - Line width: 9 px
- Menu "Image" - "Guides" - "New Guide (by percent")
  - Vertical
  - 50%
- Menu "Image" - "Guides" - "New Guide..."
  - Horizontal
  -  37 + 22 = 59 
  - 107 - 22 = 85
- Menu "View" - "Snap to guides"
- Paintbrush tool
  - Size 9
  
Fill background layer
- Green: 1a841a
- Red  : d31800


Resize
- magick on@2x.png -resize 72x72 on.png
- magick off@2x.png -resize 72x72 off.png
