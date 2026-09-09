# ASCII Art

**This project is no longer under active development.**

## About

This was just something I wanted to try one weekend: converting an image into ASCII art, entirely in C#, no external image-processing libraries.

## Goal

Grab a region of the screen and render it as ASCII, either printed straight to the console in color, or written out to a much higher-resolution text file for viewing elsewhere.

## Implementation

The program captures a rectangular region of the screen between two points you click, using `GetCursorPos` (Win32) to read where you clicked and GDI+ (`Graphics.CopyFromScreen`) to grab the pixels.

For each ASCII character in the output, it downsamples the block of pixels behind it into a single brightness value (a luminance-weighted average of R/G/B), then picks a character from a light-to-dark gradient string based on how bright or dark that block is.

There's also an optional color mode: each block's average color gets snapped to the closest match in a small hardcoded console-safe palette, and the Windows console API is used to set the text color per character.

Since the console itself only fits around 100x60 characters, there's a second, higher-detail mode that skips color and writes the ASCII art out to a much larger text file (980x240 characters) instead, meant to be viewed in an editor with word-wrap off, zoomed out.

## Warnings

This is Windows-only: it relies on GDI+ (`System.Drawing`) for screen capture and direct Win32 calls (`user32.dll`/`kernel32.dll`) for reading the cursor position and setting console text color.

## Usage

Build and run the project. It'll walk you through it:

Move your cursor to one corner of the image you want to convert and press enter, then do the same for the opposite corner, then press enter again to actually take the screenshot.

Enter a width in characters. Height is calculated automatically to match the screenshot's aspect ratio.

Depending on the `WRITE_TO_FILE` constant near the top of `ASCIIDrawer.cs`, the result either prints straight to the console in color, or gets written out to `image.txt` in the project folder.
