using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Security.Policy;
using System.IO;

class ASCIIDrawer
{
    // CONSTANTS

    public const bool WRITE_TO_FILE = true;

    public const bool FIT_ASCII_MAX = true;
    public const int CONSOLE_WIDTH = 100;
    public const int CONSOLE_HEIGHT = 61;
    public const int FILE_WIDTH = 980;
    public const int FILE_HEIGHT = 240;

    public const int ASCII_WIDTH_MAX = MAX_WIDTH;
    public const int ASCII_HEIGHT_MAX = MAX_HEIGHT;

    public const bool INVERT_BRIGHTNESS = false;
    public const bool AUTO_INVERT = false;
    public const double INVERT_THRESHOLD = 0.5;

    public const bool IGNORE_BELOW_AVERAGE = false;
    public const bool IGNORE_BELOW_THRESHOLD = false;
    public const double IGNORE_THRESHOLD = 0;

    public const double WHITESHIFT = 0;
    public const bool COLOR = false;

    public const double WHITE_THRESHOLD = 1;
    public const double BLACK_THRESHOLD = 0;

    public const int GRADIENT = 0;


    public const String FILEPATH = "../../image.txt";

    public const int MAX_WIDTH = WRITE_TO_FILE ? FILE_WIDTH : CONSOLE_WIDTH / 2;
    public const int MAX_HEIGHT = WRITE_TO_FILE ? FILE_HEIGHT : CONSOLE_HEIGHT;

    public const int STD_OUTPUT_HANDLE = -11;

    //public const bool PRINT_OVERLAP = true; // add in overlap



    // VARIABLES

    public static Bitmap picture;

    public static int picture_width;
    public static int picture_height;

    public static int ascii_width;
    public static int ascii_height;

    public static double picture_ratio;

    public static String[] ascii_gradients = new string[] { "$@B%8&WM#*oahkbdpqwmZO0QLCJUYXzcvunxrjft/\\|()1{}[]?-_+~<>i!lI;:,\"^`'. ",
                                                             "$#+-.  ",
                                                             "█▓▒░ "};


    public static char[] ascii_gradient = ascii_gradients[GRADIENT].ToCharArray();
    public static int gradient_length = ascii_gradient.Length;

    public static double[,] ascii_brightnesses;
    public static Color[,] ascii_colors;

    public static double maxBrightness;
    public static double minBrightness;
    public static double totalAverageBrightness;

    public static List<Color> colors;

    


    // MAIN

    public static void Main()
    {
        /*
        for (int i = 0; i < 255; i += 51)
        {
            for (int j = 0; j < 255; j += 51)
            {
                for (int k = 0; k < 255; k += 51)
                {
                    setConsoleColor(i, j, k);
                    Console.Write("█");
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.WriteLine(", " + i + " " + j + " " + k);
                }
            }
        }
        Console.ReadLine();
        return;
        */

        initColors();
        captureScreenshot();
        getAsciiDimensions();

        print();

        Console.BackgroundColor = ConsoleColor.Black;
        Console.WriteLine("Press enter to exit");
        Console.ReadLine();
    }



    // EXTERNAL METHODS

    [DllImport("user32.dll")]
    static extern bool GetCursorPos(ref Point lpPoint);

    [DllImport("kernel32.dll")]
    static extern bool SetConsoleTextAttribute(IntPtr hConsoleOutput, int wAttributes);

    [DllImport("kernel32.dll")]
    static extern IntPtr GetStdHandle(int nStdHandle);


    // PUBLIC METHODS

    public static void captureScreenshot()
    {
        Point firstCorner = new Point();
        Point secondCorner = new Point();

        Console.WriteLine("Move cursor to one corner of the picture and press enter.");
        Console.ReadLine();
        GetCursorPos(ref firstCorner);

        Console.WriteLine("Move cursor to the other corner of the picture and press enter.");
        Console.ReadLine();
        GetCursorPos(ref secondCorner);

        Console.WriteLine("Press enter again to take a screenshot");
        Console.ReadLine();

        Rectangle rect = new Rectangle(
            Math.Min(firstCorner.X, secondCorner.X),
            Math.Min(firstCorner.Y, secondCorner.Y),
            Math.Abs(firstCorner.X - secondCorner.X),
            Math.Abs(firstCorner.Y - secondCorner.Y));

        picture_width = rect.Width;
        picture_height = rect.Height;

        picture = new Bitmap(rect.Width, rect.Height);

        using (Graphics g = Graphics.FromImage(picture))
        {
           g.CopyFromScreen(rect.X, rect.Y, 0, 0, picture.Size);
        }
    }



    public static void getAsciiDimensions()
    {
        Console.WriteLine("Enter the width. Height will be adjusted to match the aspect ratio of the screenshot");
        Console.Write("Width: ");

        while (true)
        {
            var input = Console.ReadLine();

            if (!int.TryParse(input, out ascii_width))
            {
                Console.WriteLine("Invalid input. Enter positive integers only");
                continue;
            }

            break;
        }

        ASCIIDrawer.calculateDimensions();

        Console.WriteLine("Width was set to: " + ascii_width);
        Console.WriteLine("Height was set to: " + ascii_height);
        Console.WriteLine();
    }



    public static void print()
    {
        changeResoltuion();

        if (WRITE_TO_FILE)
            writeToFile();
        else
            printToConsole();
    }

    

    // PRIVATE METHODS

    private static void calculateDimensions()
    {
        bool modified = false;
        picture_ratio = picture_height / (double)picture_width;
        
        if (FIT_ASCII_MAX && ascii_width > MAX_WIDTH * 2)
        {
            ascii_width = MAX_WIDTH * 2;
            modified = true;
        }

        ascii_height = (int)Math.Ceiling(ascii_width * picture_ratio);

        if (FIT_ASCII_MAX && ascii_height > MAX_HEIGHT)
        {
            ascii_height = MAX_HEIGHT;
            ascii_width = (int)Math.Ceiling(ascii_height / picture_ratio);
            modified = true;
        }

        if (modified)
            Console.WriteLine("The dimensions have been altered");
    }



    private static double brightness(int x, int y)
    {
        Color pixelColor = picture.GetPixel(x, y);
        return (pixelColor.R * 0.299 + pixelColor.G * 0.587 + pixelColor.B * 0.114) / 255;
    }



    private static double averageBrightness(int x1, int y1, int x2, int y2)
    {
        double average_brightness;
        
        int width = Math.Abs(x1 - x2);
        int height = Math.Abs(y1 - y2);
        int x = Math.Min(x1, x2);
        int y = Math.Min(y1, y2);
        
        int count = 0;

        double totalBrightness = 0;
        for (int i = x; i < x + width; i++)
        {
            for (int j = y; j < y + height; j++)
            {
                if (i >= 0 && i < picture.Width && j >= 0 && j < picture.Height)
                {
                    totalBrightness += brightness(i, j);
                    count++;
                    continue;
                }
            }
        }

        return 1 - Math.Pow(totalBrightness / count, Math.Pow(10, WHITESHIFT));
    }



    private static void changeResoltuion()
    {
        double ascii_horizontal = picture_width / (double)ascii_width / 2;
        double ascii_vertical = picture_height / (double)ascii_height;

        ascii_brightnesses = new double[ascii_width * 2, ascii_height];

        if (!WRITE_TO_FILE && COLOR)
            ascii_colors = new Color[ascii_width * 2, ascii_height];

        totalAverageBrightness = 0.0;

        for (int ascii_y = 0; ascii_y < ascii_height; ascii_y++)
        {
            for (int ascii_x = 0; ascii_x < ascii_width * 2; ascii_x++)
            {
                ascii_brightnesses[ascii_x, ascii_y] = averageBrightness((int)Math.Floor(ascii_x * ascii_horizontal), (int)Math.Floor(ascii_y * ascii_vertical),
                                                                         (int)Math.Floor((ascii_x + 1) * ascii_horizontal) + 1, (int)Math.Floor(ascii_y * ascii_vertical) + 1);

                if (!WRITE_TO_FILE && COLOR)
                    ascii_colors[ascii_x, ascii_y] = averageColor((int)Math.Floor(ascii_x * ascii_horizontal), (int)Math.Floor(ascii_y * ascii_vertical),
                                                                  (int)Math.Floor((ascii_x + 1) * ascii_horizontal) + 1, (int)Math.Floor(ascii_y * ascii_vertical) + 1);

                totalAverageBrightness += ascii_brightnesses[ascii_x, ascii_y] / (ascii_height * ascii_width);

                if (ascii_brightnesses[ascii_x, ascii_y] > maxBrightness)
                    maxBrightness = ascii_brightnesses[ascii_x, ascii_y];

                if (ascii_brightnesses[ascii_x, ascii_y] < minBrightness)
                    minBrightness = ascii_brightnesses[ascii_x, ascii_y];
            }
        }
    }



    private static void printToConsole()
    {
        double modifiedBrightness;
        Color color;

        maxBrightness = 0;
        minBrightness = 1;

        for (int ascii_y = 0; ascii_y < ascii_height; ascii_y++)
        {
            for (int ascii_x = 0; ascii_x < ascii_width * 2; ascii_x++)
            {
                if (COLOR)
                {
                    color = ascii_colors[ascii_x, ascii_y];
                    color = findClosestColor(color);
                    setConsoleColor(color.R, color.G, color.B);
                }

                modifiedBrightness = 1 - ascii_brightnesses[ascii_x, ascii_y];

                if (INVERT_BRIGHTNESS || (AUTO_INVERT && totalAverageBrightness > INVERT_THRESHOLD))
                    modifiedBrightness = 1 - modifiedBrightness;

                if (IGNORE_BELOW_AVERAGE && modifiedBrightness > maxBrightness ||
                    IGNORE_BELOW_THRESHOLD && modifiedBrightness > IGNORE_THRESHOLD)
                {
                    Console.Write(ascii_gradient[gradient_length - 1]);
                    continue;
                }

                if (minBrightness == maxBrightness)
                {
                    Console.Write(ascii_gradient[(int)((gradient_length - 1) * 0.5)]);
                    continue;
                }

                if (maxBrightness - minBrightness == 0)
                {
                    Console.Write(ascii_gradient[0]);
                    continue;
                }

                modifiedBrightness = (modifiedBrightness - minBrightness) / (maxBrightness - minBrightness);

                Console.Write(ascii_gradient[(int)((gradient_length - 1) * (modifiedBrightness))]);
            }

            Console.WriteLine();
        }
    }



    private static void writeToFile()
    {
        double modifiedBrightness;

        maxBrightness = 0;
        minBrightness = 1;

        File.Delete(FILEPATH);

        for (int ascii_y = 0; ascii_y < ascii_height; ascii_y++)
        {
            Console.WriteLine((int)(ascii_y / (double) ascii_height * 10000) / 100.0 + "%");

            for (int ascii_x = 0; ascii_x < ascii_width * 2; ascii_x++)
            {
                modifiedBrightness = 1 - ascii_brightnesses[ascii_x, ascii_y];


                if (INVERT_BRIGHTNESS || AUTO_INVERT && totalAverageBrightness < INVERT_THRESHOLD)
                    modifiedBrightness = 1 - modifiedBrightness;

                if (IGNORE_BELOW_AVERAGE && modifiedBrightness > maxBrightness ||
                    IGNORE_BELOW_THRESHOLD && modifiedBrightness > IGNORE_THRESHOLD)
                {
                    File.AppendAllText(FILEPATH, ascii_gradient[gradient_length - 1].ToString());
                    continue;
                }

                if (minBrightness == maxBrightness)
                {
                    File.AppendAllText(FILEPATH, ascii_gradient[(int)((gradient_length - 1) * 0.5)].ToString());
                    continue;
                }

                if (maxBrightness - minBrightness == 0)
                {
                    File.AppendAllText(FILEPATH, ascii_gradient[0].ToString());
                    continue;
                }

                modifiedBrightness = (modifiedBrightness - minBrightness) / (maxBrightness - minBrightness);

                File.AppendAllText(FILEPATH, ascii_gradient[(int)((gradient_length - 1) * (modifiedBrightness))].ToString());
            }

            File.AppendAllText(FILEPATH, "\n");
        }
    }



    private static void setConsoleColor(int r, int g, int b)
    {
        int color = (16 + (r / 51) * 36) + ((g / 51) * 6) + (b / 51);
        SetConsoleTextAttribute(GetStdHandle(STD_OUTPUT_HANDLE), color);
    }



    private static Color averageColor(int x1, int y1, int x2, int y2)
    {
        int red = 0;
        int green = 0;
        int blue = 0;
        int total = 0;

        for (int x = x1; x < x2; x++)
        {
            for (int y = y1; y < y2; y++)
            {
                if (x >= 0 && x < picture.Width && y >= 0 && y < picture.Height)
                {
                    Color pixel = picture.GetPixel(x, y);
                    red += pixel.R;
                    green += pixel.G;
                    blue += pixel.B;
                    total++;
                }
            }
        }
        return Color.FromArgb(red / total, green / total, blue / total);
    }

    private static Color findClosestColor(Color targetColor)
    {
        Color closestColor = colors[0];
        int closestDistance = int.MaxValue;

        foreach (Color color in colors)
        {
            int redDistance = color.R - targetColor.R;
            int greenDistance = color.G - targetColor.G;
            int blueDistance = color.B - targetColor.B;

            int distance = redDistance * redDistance + greenDistance * greenDistance + blueDistance * blueDistance;

            if (distance < closestDistance)
            {
                closestColor = color;
                closestDistance = distance;
            }
        }

        return closestColor;
    }



    private static void initColors()
    {
        colors = new List<Color>();

        colors.Add(Color.FromArgb(0, 0, 51));
        colors.Add(Color.FromArgb(0, 0, 102));
        colors.Add(Color.FromArgb(0, 0, 153));
        colors.Add(Color.FromArgb(0, 0, 204));
        colors.Add(Color.FromArgb(0, 51, 0));
        colors.Add(Color.FromArgb(0, 51, 51));
        colors.Add(Color.FromArgb(0, 51, 102));
        colors.Add(Color.FromArgb(0, 102, 0));
        colors.Add(Color.FromArgb(0, 102, 51));
        colors.Add(Color.FromArgb(0, 102, 102));
        colors.Add(Color.FromArgb(0, 204, 153));
        colors.Add(Color.FromArgb(0, 102, 204));
    }
}
