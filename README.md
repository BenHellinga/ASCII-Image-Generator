# ASCII Image Converter

**This project is no longer under active development.**

## About

This was just something I wanted to try one weekend in 2022, converting an image into ASCII art.

## Goal

Convert an image, either a saved local image or new screenshot, to ASCII art in a specific output file.

## Implementation

The program first obtains an image, either by reading a local image or by taking a screenshot of the user designated region of the screen.
It then uses the user provided or default height, computes the appropriate width using the aspect ratio of the image, and downsamples or upsamples the image to get brightness values at the output resolution.
It then uses the min and max brightness to choose the appropriate ascii character from either the user provided or default gradient string.

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

Move the cursor to one corner of the region and press enter, then do the same for the other corner.

Either form also takes these optional flags:

- `-s <height>` sets the output height in characters (default 100). Width is calculated automatically to match the aspect ratio of the picture.
- `-g "<gradient>"` uses your own gradient of characters instead of the default one. Gradients go light to dark, same as the default (`" .:-=+*#%@"`).

```
dotnet run in/image.png out/image.txt -s 50 -g " .-+*#"
```

## Examples

<img width="1460" height="1238" alt="pi" src="https://github.com/user-attachments/assets/88b33d71-7af1-4d1f-8230-2345fdc30188" />

<img width="1191" height="1109" alt="image" src="https://github.com/user-attachments/assets/0466a729-eb13-4ce2-aa8a-abf56de9d40e" />

<img width="1017" height="1020" alt="spiderman" src="https://github.com/user-attachments/assets/74099430-db37-454f-b6a3-fd66291bfa9f" />

<img width="1210" height="1205" alt="image" src="https://github.com/user-attachments/assets/a0a04dd0-366f-4b81-9785-76d8cf4f5389" />
