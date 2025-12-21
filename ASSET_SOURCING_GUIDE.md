# Pacman Game - Asset Sourcing Guide

## Current Assets
We've successfully created pixel art sprites for:
- **Pacman** (4 directional variants)
- **Ghosts** (4 unique colored ghosts: Red, Pink, Cyan, Orange)
- **Fruits** (7 types: Cherry, Strawberry, Orange, Apple, Melon, Banana, Grape)

All sprites are 32x32 pixels PNG format with transparency support.

## Where to Find High-Quality Replacement Assets

If you want to upgrade to professional or higher-quality sprites:

### 1. **OpenGameArt.org**
- **URL**: https://opengameart.org/
- **Best for**: Free, open-source pixel art
- **License**: Various (check individual assets)
- **Search**: "Pacman sprites" or "ghost sprites"

### 2. **Itch.io**
- **URL**: https://itch.io/game-assets
- **Best for**: Indie game assets, pixel art packs
- **Search**: "Pacman game kit" or "retro game sprites"
- **Note**: Mix of free and paid assets

### 3. **Kenney.nl**
- **URL**: https://www.kenney.nl/assets
- **Best for**: High-quality, well-organized game assets
- **License**: CC0 (public domain)
- **Note**: Excellent pixel art collection

### 4. **AssetStore (Unity)**
- **URL**: https://assetstore.unity.com/
- **Best for**: Professional 2D art packs
- **License**: Various
- **Note**: Some assets work in XNA Framework

### 5. **CraftPix.net**
- **URL**: https://craftpix.net/
- **Best for**: Affordable pixel art bundles
- **License**: Commercial use allowed on paid assets

### 6. **FreePik / Pixabay**
- **URL**: https://www.freepik.com/ or https://pixabay.com/
- **Best for**: General graphics and illustrations
- **License**: Check individual assets

## Creating Custom Sprites

### Option A: Simple Online Tools
- **Piskel App**: https://www.piskelapp.com/ (Free, browser-based)
- **Aseprite**: https://www.aseprite.org/ (Paid, professional)
- **LibreSprite**: Free Aseprite fork

### Option B: Enhance Existing Sprites
Edit the generated sprites using:
- **GIMP** (Free, open-source)
- **Photoshop** (Professional, paid)
- **Krita** (Free, open-source)

### Option C: AI Generation
- **Stable Diffusion** with "pixel art" prompt
- **Craiyon**: https://www.craiyon.com/
- Generate and refine as needed

## Integration Steps

Once you have new sprites:

1. **Replace existing PNG files**:
   ```
   Content/Sprites/Pacman/pacman_*.png
   Content/Sprites/Ghosts/ghost_*.png
   Content/Sprites/Fruits/*.png
   ```

2. **Rebuild the project**:
   ```bash
   dotnet build
   ```

3. **Test in game**:
   ```bash
   dotnet run
   ```

## Recommended Sprite Specifications

For best results with MonoGame:

- **Format**: PNG with alpha transparency
- **Size**: 32x32 pixels minimum (scalable to 64x64 or 128x128)
- **Color Depth**: 32-bit (RGBA)
- **Background**: Transparent (alpha channel)
- **Style**: Pixel art or cartoon for consistency

## Animation Support (Future Enhancement)

To add sprite animations:
1. Create sprite sheets with multiple frames
2. Update GameRenderer to support:
   - Frame selection based on game state
   - Animation timing
   - Direction-based frame selection

## Current Asset Inventory

| Asset | Count | Location | Status |
|-------|-------|----------|--------|
| Pacman Sprites | 4 | `Content/Sprites/Pacman/` | ✓ Generated |
| Ghost Sprites | 4 | `Content/Sprites/Ghosts/` | ✓ Generated |
| Fruit Sprites | 7 | `Content/Sprites/Fruits/` | ✓ Generated |
| **Total** | **15** | - | **Ready** |

## Quick Asset Replacement Checklist

- [ ] Find or create new sprite images
- [ ] Save as PNG with transparency
- [ ] Ensure 32x32 pixel size (or update Content.mgcb if different)
- [ ] Replace files in Content/Sprites/
- [ ] Run `dotnet build`
- [ ] Test game visuals
- [ ] Commit changes to git

## License Considerations

When sourcing assets:
- ✓ Always check the asset license
- ✓ Ensure commercial use is allowed (if applicable)
- ✓ Credit the artist if required
- ✓ Verify CC0 or similar open licenses for open-source projects

## Next Steps

1. **Immediate**: Current generated sprites work and display correctly
2. **Short-term**: Enhance sprites with better pixel art
3. **Medium-term**: Add animation support
4. **Long-term**: Integrate fruit mechanics into gameplay
