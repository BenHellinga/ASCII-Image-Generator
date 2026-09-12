using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;



class ASCIIDrawer
{
    // CONSTANTS



    // light to dark, custom gradients passed via -g must use the same order
    public const string DEFAULT_GRADIENT = " .:-=+*#%@";
    public const int DEFAULT_HEIGHT = 100;



    // VARIABLES



    public static Bitmap picture = null!;

    public static int asciiWidth;
    public static int asciiHeight;

    public static double pictureRatio;

    public static char[] asciiGradient = null!;
    public static int gradientLength;

    public static double[,] asciiBrightnesses = null!;

    public static double maxBrightness;
    public static double minBrightness;



    // MAIN



    public static void Main(string[] args)
    {
        (string? inputPath, string outputPath, string gradient, int height) parsed;

        try
        {
            parsed = ParseArgs(args);
        }
        catch (ArgumentException e)
        {
            Console.WriteLine(e.Message);
            return;
        }

        asciiGradient = parsed.gradient.ToCharArray();
        gradientLength = asciiGradient.Length;
        asciiHeight = parsed.height;

        picture = parsed.inputPath == null ? CaptureScreenshot() : LoadImage(parsed.inputPath);

        SetAsciiDimensions();

        string ascii = Convert();
        Save(ascii, parsed.outputPath);

        Console.WriteLine("Finished");
    }



    // EXTERNAL METHODS



    [DllImport("user32.dll")]
    static extern bool GetCursorPos(ref Point lpPoint);



    // PUBLIC METHODS



    // reads input path, output path, and optional gradient/height flags from the command line
    // dotnet run <output path>, or dotnet run <input path> <output path>, both take optional -g "gradient" and -h <height>
    public static (string? inputPath, string outputPath, string gradient, int height) ParseArgs(string[] args)
    {
        string gradient = DEFAULT_GRADIENT;
        int height = DEFAULT_HEIGHT;
        List<string> positional = [];

        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "-g")
            {
                if (i + 1 >= args.Length)
                    throw new ArgumentException("-g needs a gradient string after it (eg. \" .-O#\")");

                gradient = args[i + 1];
                i++;
                continue;
            }

            if (args[i] == "-s")
            {
                if (i + 1 >= args.Length || !int.TryParse(args[i + 1], out height))
                    throw new ArgumentException("-s needs a height in characters after it (eg. \"-s 200\")");

                i++;
                continue;
            }

            positional.Add(args[i]);
        }

        if (positional.Count == 1)
            return (null, positional[0], gradient, height);

        if (positional.Count == 2)
            return (positional[0], positional[1], gradient, height);

        throw new ArgumentException("Usage: dotnet run [input path] <outputh path> [-g <gradient string>] [-s <height>]");
    }



    // picks the region to convert by having you click two opposite corners of the picture on screen
    public static Bitmap CaptureScreenshot()
    {
        Point firstCorner = new();
        Point secondCorner = new();

        Console.Write("Move cursor to one corner of the picture and press enter");
        Console.ReadLine();
        GetCursorPos(ref firstCorner);

        Console.Write("Move cursor to the other corner of the picture and press enter");
        Console.ReadLine();
        GetCursorPos(ref secondCorner);

        Rectangle rect = new(
            Math.Min(firstCorner.X, secondCorner.X),
            Math.Min(firstCorner.Y, secondCorner.Y),
            Math.Abs(firstCorner.X - secondCorner.X),
            Math.Abs(firstCorner.Y - secondCorner.Y));

        Bitmap screenshot = new(rect.Width, rect.Height);

        // copyfromscreen doesnt grab the cursor, no need to move it out of frame first
        using (Graphics g = Graphics.FromImage(screenshot))
            g.CopyFromScreen(rect.X, rect.Y, 0, 0, screenshot.Size);

        return screenshot;
    }



    // loads the image to convert from disk
    public static Bitmap LoadImage(string inputPath)
    {
        return new Bitmap(inputPath);
    }



    // width is derived from asciiHeight and the picture's aspect ratio, no user input needed
    public static void SetAsciiDimensions()
    {
        CalculateDimensions();
        Console.WriteLine($"Resolution was set to ({asciiWidth}, {asciiHeight})");
    }



    // converts the picture into an ascii string, one line per row
    // first pass gets each cell's brightness and tracks the real min/max, second pass
    // scales every cell into that min/max range before picking a gradient character, so
    // a dark image still spans the whole gradient instead of only using the dark end
    public static string Convert()
    {
        ChangeResolution();

        maxBrightness = double.MinValue;
        minBrightness = double.MaxValue;

        for (int ascii_y = 0; ascii_y < asciiHeight; ascii_y++)
        for (int ascii_x = 0; ascii_x < asciiWidth * 2; ascii_x++)
        {
            double brightness = asciiBrightnesses[ascii_x, ascii_y];

            if (brightness > maxBrightness)
                maxBrightness = brightness;

            if (brightness < minBrightness)
                minBrightness = brightness;
        }

        StringBuilder output = new();

        for (int ascii_y = 0; ascii_y < asciiHeight; ascii_y++)
        {
            for (int ascii_x = 0; ascii_x < asciiWidth * 2; ascii_x++)
            {
                double brightness = asciiBrightnesses[ascii_x, ascii_y];

                if (minBrightness == maxBrightness)
                {
                    output.Append(asciiGradient[(int)((gradientLength - 1) * 0.5)]);
                    continue;
                }

                double scaled = (brightness - minBrightness) / (maxBrightness - minBrightness);

                output.Append(asciiGradient[(int)((gradientLength - 1) * scaled)]);
            }

            output.Append('\n');
        }

        return output.ToString();
    }



    // writes the ascii art out to the output path
    public static void Save(string ascii, string outputPath)
    {
        File.WriteAllText(outputPath, ascii);
    }



    // PRIVATE METHODS



    private static void CalculateDimensions()
    {
        pictureRatio = picture.Height / (double)picture.Width;
        asciiWidth = (int)Math.Ceiling(asciiHeight / pictureRatio);
    }



    // standard luminance weights, eyes are more sensitive to green than red or blue
    private static double Brightness(int x, int y)
    {
        Color pixelColor = picture.GetPixel(x, y);
        return (pixelColor.R * 0.299 + pixelColor.G * 0.587 + pixelColor.B * 0.114) / 255;
    }



    // downsamples a block of pixels one ascii character worth into a single brightness value
    private static double AverageBrightness(int x1, int y1, int x2, int y2)
    {
        int width = Math.Abs(x1 - x2);
        int height = Math.Abs(y1 - y2);
        int x = Math.Min(x1, x2);
        int y = Math.Min(y1, y2);

        int count = 0;

        double totalBrightness = 0;
        for (int i = x; i < x + width; i++)
        for (int j = y; j < y + height; j++)
        {
            if (i >= 0 && i < picture.Width && j >= 0 && j < picture.Height)
            {
                totalBrightness += Brightness(i, j);
                count++;
                continue;
            }
        }

        return totalBrightness / count;
    }



    private static void ChangeResolution()
    {
        double ascii_horizontal = picture.Width / (double)asciiWidth / 2;
        double ascii_vertical = picture.Height / (double)asciiHeight;

        asciiBrightnesses = new double[asciiWidth * 2, asciiHeight];

        for (int ascii_y = 0; ascii_y < asciiHeight; ascii_y++)
        for (int ascii_x = 0; ascii_x < asciiWidth * 2; ascii_x++)
        {
            asciiBrightnesses[ascii_x, ascii_y] = AverageBrightness((int)Math.Floor(ascii_x * ascii_horizontal), (int)Math.Floor(ascii_y * ascii_vertical),
                                                                    (int)Math.Floor((ascii_x + 1) * ascii_horizontal) + 1, (int)Math.Floor(ascii_y * ascii_vertical) + 1);
        }
    }
}