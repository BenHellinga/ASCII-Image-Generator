# ASCII Image Converter

**This project is no longer under active development.**

## About

This was just something I wanted to try one weekend in 2022, converting an image into ASCII art.

## Goal

Grab a region of the screen and render it as ASCII, written out to a specified file.

## Implementation

The program captures a rectangular region of the screen between two points you click, using `GetCursorPos` (Win32) to read where you clicked and GDI+ (`Graphics.CopyFromScreen`) to grab the pixels.

For each ASCII character in the output, it downsamples the block of pixels behind it into a single brightness value (a luminance-weighted average of R/G/B), then picks a character from a light-to-dark gradient string based on how bright or dark that block is. The brightest and darkest cells in the picture are used to stretch the whole gradient across the image's actual range, so a dark photo still spans light to dark characters instead of only using the dark end.

## Warnings

This is Windows-only: it relies on GDI+ (`System.Drawing`) for screen capture and direct Win32 calls (`user32.dll`) for reading the cursor position.

## Usage

Convert an existing image file:

```
dotnet run <input path> <output path>
```

Or capture a region of the screen instead:

```
dotnet run <output path>
```

Move the cursor to one corner of the region and press enter, then do the same for the other corner. The screenshot is taken right after, no need to move the cursor out of frame first.

Either form also takes these optional flags:

- `-h <height>` sets the output height in characters (default 100). Width is calculated automatically to match the aspect ratio of the picture.
- `-g "<gradient>"` uses your own gradient of characters instead of the default one. Gradients go light to dark, same as the default (`" .:-=+*#%@"`).

```
dotnet run in/image.png out/image.txt -h 200 -g " .-+*#"
```