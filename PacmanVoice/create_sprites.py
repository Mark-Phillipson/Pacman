#!/usr/bin/env python3
"""
Create simple pixel art sprites for Pacman game
Requires: pip install pillow
"""

from PIL import Image, ImageDraw
import os

def create_pacman_sprites():
    """Create Pacman sprites facing different directions with eating animation frames"""
    sprites_dir = "Content/Sprites/Pacman"
    os.makedirs(sprites_dir, exist_ok=True)
    
    size = 32
    
    # Create animation frames for each direction
    # Frame 0: mouth closed (full circle)
    # Frame 1: mouth slightly open
    # Frame 2: mouth wide open
    
    directions = {
        "right": (0, (8, 8, 12, 12)),      # mouth_angle_start, eye_pos
        "left": (180, (size-12, 8, size-8, 12)),
        "up": (270, (8, 18, 12, 22)),      # mouth opens upward (toward top of screen)
        "down": (90, (8, 8, 12, 12))       # mouth opens downward (toward bottom of screen)
    }
    
    for direction_name, (mouth_angle, eye_pos) in directions.items():
        # Frame 0: closed (full circle)
        img = Image.new('RGBA', (size, size), (0, 0, 0, 0))
        draw = ImageDraw.Draw(img)
        draw.ellipse([2, 2, size-2, size-2], fill=(255, 255, 0))
        draw.ellipse(eye_pos, fill=(0, 0, 0))
        img.save(f"{sprites_dir}/pacman_{direction_name}_0.png")
        
        # Frame 1: slightly open (15 degrees)
        img = Image.new('RGBA', (size, size), (0, 0, 0, 0))
        draw = ImageDraw.Draw(img)
        # Draw Pacman with mouth open 15 degrees
        draw.pieslice([2, 2, size-2, size-2], mouth_angle + 7, mouth_angle + 360 - 7, fill=(255, 255, 0))
        draw.ellipse(eye_pos, fill=(0, 0, 0))
        img.save(f"{sprites_dir}/pacman_{direction_name}_1.png")
        
        # Frame 2: wide open (45 degrees)
        img = Image.new('RGBA', (size, size), (0, 0, 0, 0))
        draw = ImageDraw.Draw(img)
        # Draw Pacman with mouth open 45 degrees
        draw.pieslice([2, 2, size-2, size-2], mouth_angle + 22, mouth_angle + 360 - 22, fill=(255, 255, 0))
        draw.ellipse(eye_pos, fill=(0, 0, 0))
        img.save(f"{sprites_dir}/pacman_{direction_name}_2.png")
        
        # Also keep the original format for backward compatibility
        img = Image.new('RGBA', (size, size), (0, 0, 0, 0))
        draw = ImageDraw.Draw(img)
        draw.ellipse([2, 2, size-2, size-2], fill=(255, 255, 0))
        draw.ellipse(eye_pos, fill=(0, 0, 0))
        img.save(f"{sprites_dir}/pacman_{direction_name}.png")
    
    print("✓ Pacman sprites created with animation frames")

def create_ghost_sprites():
    """Create ghost sprites in different colors"""
    sprites_dir = "Content/Sprites/Ghosts"
    os.makedirs(sprites_dir, exist_ok=True)
    
    size = 32
    colors = {
        "red": (255, 0, 0),      # Blinky
        "pink": (255, 184, 255),  # Pinky
        "cyan": (0, 255, 255),    # Inky
        "orange": (255, 184, 82)  # Clyde
    }
    
    for ghost_name, color in colors.items():
        img = Image.new('RGBA', (size, size), (0, 0, 0, 0))
        draw = ImageDraw.Draw(img)
        
        # Ghost body (rounded top with wavy bottom)
        draw.rectangle([4, 4, size-4, 20], fill=color)
        draw.ellipse([4, 2, size-4, 18], fill=color)
        
        # Ghost bottom (wavy effect with small circles)
        for i in range(4):
            x = 6 + (i * 6)
            draw.ellipse([x, 18, x+6, 26], fill=color)
        
        # Eyes (white with black pupils)
        # Left eye
        draw.ellipse([8, 10, 12, 14], fill=(255, 255, 255))
        draw.ellipse([9, 11, 11, 13], fill=(0, 0, 0))
        
        # Right eye
        draw.ellipse([size-12, 10, size-8, 14], fill=(255, 255, 255))
        draw.ellipse([size-11, 11, size-9, 13], fill=(0, 0, 0))
        
        img.save(f"{sprites_dir}/ghost_{ghost_name}.png")
    
    print("✓ Ghost sprites created")

def create_fruit_sprites():
    """Create fruit sprites"""
    sprites_dir = "Content/Sprites/Fruits"
    os.makedirs(sprites_dir, exist_ok=True)
    
    size = 32
    fruits = {
        "cherry": (255, 0, 0),      # Red
        "strawberry": (255, 50, 50), # Dark red
        "orange": (255, 165, 0),    # Orange
        "apple": (0, 200, 0),       # Green
        "melon": (0, 150, 0),       # Dark green
        "banana": (255, 255, 0),    # Yellow
        "grape": (128, 0, 128),     # Purple
    }
    
    for fruit_name, color in fruits.items():
        img = Image.new('RGBA', (size, size), (0, 0, 0, 0))
        draw = ImageDraw.Draw(img)
        
        if fruit_name in ["cherry"]:
            # Two small circles
            draw.ellipse([6, 6, 14, 14], fill=color)
            draw.ellipse([14, 6, 22, 14], fill=color)
            # Stem
            draw.line([(10, 6), (18, 6)], fill=(100, 100, 0), width=2)
        elif fruit_name in ["strawberry"]:
            # Heart-like shape
            draw.ellipse([8, 12, 24, 28], fill=color)
            draw.ellipse([8, 8, 14, 14], fill=color)
            draw.ellipse([18, 8, 24, 14], fill=color)
        elif fruit_name in ["orange"]:
            # Circle with segments
            draw.ellipse([6, 6, 26, 26], fill=color)
            draw.line([(16, 6), (16, 26)], fill=(200, 120, 0), width=1)
            draw.line([(6, 16), (26, 16)], fill=(200, 120, 0), width=1)
        elif fruit_name in ["apple", "melon"]:
            # Circle with stem
            draw.ellipse([6, 8, 26, 28], fill=color)
            draw.rectangle([15, 2, 17, 8], fill=(100, 100, 0))
        elif fruit_name in ["banana"]:
            # Curved shape
            draw.arc([4, 10, 24, 30], 0, 180, fill=color, width=8)
            draw.ellipse([6, 12, 10, 28], fill=color)
            draw.ellipse([22, 12, 26, 28], fill=color)
        elif fruit_name in ["grape"]:
            # Bunch of small circles
            for i in range(3):
                for j in range(3):
                    draw.ellipse([6 + i*8, 6 + j*8, 12 + i*8, 12 + j*8], fill=color)
        
        img.save(f"{sprites_dir}/{fruit_name}.png")
    
    print("✓ Fruit sprites created")

if __name__ == "__main__":
    try:
        create_pacman_sprites()
        create_ghost_sprites()
        create_fruit_sprites()
        print("\n✅ All sprites created successfully!")
        print("Sprites saved to Content/Sprites/")
    except ImportError:
        print("Error: Pillow not installed. Run: pip install pillow")
    except Exception as e:
        print(f"Error creating sprites: {e}")
