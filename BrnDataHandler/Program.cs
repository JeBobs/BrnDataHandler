using System.Numerics;
using System.Globalization;

internal class Program
{
    // tokens
    static bool convertExtensions = false;
    static bool keepExtensions = true;
    static bool sortByType = false;
    static bool debug = false;

    static readonly string debugPrefix = $"DEBUG:";

    public static List<string> Directories = new();
    public static List<string> Files = new();

    delegate void Command();

    static void Main(string[] args)
    {
        if (args.Length > 0)
        {
            List<string> tokens = new List<string>();

            Command command = Command_Help;

            foreach (var arg in args)
            {
                Console.WriteLine($"Launched with argument {arg}");

                if (Directory.Exists(arg))
                    Directories.Add(arg);

                else if (File.Exists(arg))
                    Files.Add(arg);

                else
                    tokens.Add(arg.ToLower());

                // Really hacky system to do quick debugging.
                // Gotta replace with a proper switch parsing system.
                foreach (string tk in tokens)
                {
                    if (tk.StartsWith("--"))
                    {
                        switch (tk) // Switches
                        {
                            case "--sort-by-type":
                                sortByType = true;
                                break;
                            case "--convert-extensions":
                                convertExtensions = true;
                                keepExtensions = false;
                                break;
                            case "--keep-original-extensions":
                                keepExtensions = true;
                                break;
                            case "--debug":
                                debug = true;
                                break;
                        }
                    }
                    else
                    {
                        switch (tk) // Commands
                        {
                            case "recover":
                                // Process
                                command = Command_Recover;
                                break;
                            case "convert-asset-endian":
                                // Convert endian
                                break;
                        }
                    }
                }

                if (convertExtensions)
                    keepExtensions = false;
            }

            // Run delegate command
            command();
        }
    }

    #region Commands
    static void Command_Help()
    {
        // TODO: Help commmand
        return;
    }

    static void Command_Recover()
    {
        if (Files.Count > 0 || Directories.Count > 0)
        {
            foreach (string filePath in Files)
            {
                RecoverFileAsync(filePath);
            }
            foreach (string directoryPath in Directories)
            {
                RecoverFolder(directoryPath);
            }
        }
        else
        {
            RecoverFolder(Directory.GetCurrentDirectory());
        }
    }

    static void Command_ConvertAssetEndian()
    {

    }
#endregion

    #region Recover

    static bool RecoverFolder(string directory)
    {
        string[] filePaths = Directory.GetFiles(directory);

        foreach (string filePath in filePaths)
        {
            RecoverFileAsync(filePath);
        }

        return true;
    }


    static async Task<bool> RecoverFileAsync(string filePath)
    {
        DataType type = IdentifyFileType(filePath);

        string newExtension = "";
        string newPath = "";
        
        if (type == DataType.NONE || type == DataType.UNKNOWN)
        {
            Console.WriteLine($"Skipping unknown file {filePath}...");
            return false;
        }

        string currentExtension = Path.GetExtension(filePath);

        Console.WriteLine($"Processing {Enum.GetName(typeof(DataType), type)} file {filePath}...");
        
        if (convertExtensions)
        {
            //newPath = keepExtensions
            //    ? $"{filePath}"
            //    : $"{filePath.Substring(0, filePath.Length - currentExtension.Length)}";

            newPath = filePath;

            currentExtension.Substring(1);

            FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);

            // Set extension
            // TODO: Further parse data to more accurately determine data 
            switch (type)
            {
                case DataType.PNG:
                    // TODO: Process PNGs properly
                    // ProcessBurnoutPNG(ParsePNG(stream), out newPath);
                    newExtension = "png";
                    break;
                case DataType.BUNDLE2:
                    newExtension =
                        currentExtension == "BIN"
                     || currentExtension == "BNDL"
                     || currentExtension == "DAT"
                            ? currentExtension
                            : "BUNDLE";
                    break;
                case DataType.SELF:
                    newExtension = "self";
                    break;
                case DataType.XEX2:
                    newExtension = "xex";
                    break;
                case DataType.VP6:
                    newExtension = "VP6";
                    break;
                case DataType.SNS:
                    newExtension = "SNS";
                    break;
            }

            stream.Close();

            if (convertExtensions)
                newPath = Path.ChangeExtension(newPath, $".{newExtension}");

            File.Move(filePath, newPath);
        }
        
        return true;
    }

    static bool ProcessBurnoutPNG(IFilePNG filePNG, out string outPath)
    {
        string directory = filePNG.path != null ? Path.GetDirectoryName(filePNG.path) : "";
        
        // Determine if PNG is PS3 art
        switch (filePNG.Dimensions)
        {
            // PIC1.PNG
            case Vector2 a when a.X == 1920 && a.Y == 1080:
            case Vector2 b when b.X == 1280 && b.Y == 720:
                filePNG.path =
                    Path.Combine(directory, $"recovered_{GetRandomPrefix(directory)}PIC1.PNG");
                break;
        
            //ICON0.PNG
            case Vector2 c when c.X == 320 && c.Y == 176:
                filePNG.path =
                    Path.Combine(directory, $"recovered_{GetRandomPrefix(directory)}ICON0.PNG");
                break;
        }
        outPath = filePNG.path;
        return true;
    }

    static void ProcessBurnoutPNG(string filePath, out string outPath)
    {
        ProcessBurnoutPNG(ParsePNG(filePath), out outPath);
    }
#endregion

    #region General Data Handling
    static string GetRandomPrefix(string directoryPath)
    {

        Random random = new Random();
        int randomNumber = random.Next(0000000, 9999999);

        List<string> files = Directory.GetFiles(directoryPath).ToList();

        // This is really non-performant and doesn't work 100% of the time.
        // If somebody would like to improve this, be my guest.
        // I have better things to do than figure out how to get a random
        // number to not equal any number in a list.

        for (int i = 0; i >= files.Count(); i++)
        {
            if (files[i].Contains("recovered_"))
                files[i] = Path.GetFileNameWithoutExtension(files[i]).Substring(10, 5);
            else
                files.Remove(files[i]);
        }

        foreach (string number in files)
        {
            if (randomNumber.ToString() == number)
                randomNumber = random.Next(0000000, 9999999);
            else
                files.Remove(number);
        }

        return $"{randomNumber}";
    }

    static DataType IdentifyFileType(FileStream stream, bool CloseStream = false)
    {
        if (stream.CanRead)
        {
            byte[] headerBytes = new byte[4];
    
            if (stream.Read(headerBytes, 0, 2) < 2)
            {
                if (CloseStream)
                    stream.Close();
                return DataType.UNKNOWN;
            }
            if (BitConverter.ToUInt32(headerBytes) == 0)
            {
                if (CloseStream)
                    stream.Close();
                return DataType.SNS;
            }
            if (stream.Read(headerBytes, 2, 2) < 2)
            {
                if (CloseStream)
                    stream.Close();
                return DataType.UNKNOWN;
            }
            
            if (CloseStream)
                stream.Close();

            uint header = BitConverter.ToUInt32(headerBytes);

            return header switch
            {
                1196314761 => DataType.PNG,         // PNG
                845442658  => DataType.BUNDLE2,     // BUNDLE2
                4539219    => DataType.SELF,        // SELF
                844645720  => DataType.XEX2,        // XEX2
                1684559437 => DataType.VP6,         // VP6
                _          => DataType.UNKNOWN      // UNKNOWN
            };
        }
    
        return DataType.NONE;
    }

    static DataType IdentifyFileType(string filePath)
    {
        return IdentifyFileType(new FileStream(filePath, FileMode.Open, FileAccess.Read), true);
    }

    static IFilePNG ParsePNG(FileStream stream, bool CloseStream = false)
    {
        stream.Position = 0;

        IFilePNG info = new IFilePNG
        {
            path = stream.Name,
            Dimensions = new Vector2 { X = 0, Y = 0 }
        };

        //using (DeflateStream deflateStream = new DeflateStream(stream, CompressionMode.Decompress))

        using (BinaryReader reader = new BinaryReader(stream))
        {
            // Check the PNG signature
            uint header = reader.ReadUInt32();
            
            if (debug) 
                Console.WriteLine($"{debugPrefix} ParsePNG - prefix is {header}");
            
            try
            {
                if (header != 1196314761)
                    throw new InvalidDataException("File passed to the PNG parser is not a PNG file!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: {ex.Message}");
            }

            // Read the rest of the header
            //reader.ReadBytes(4);

            reader.ReadInt32();

            Vector2 dimensions = new Vector2 
            { 
                X = int.Parse(reader.ReadBytes(4).ToString(), NumberStyles.HexNumber), 
                Y = int.Parse(reader.ReadBytes(4).ToString(), NumberStyles.HexNumber)
            };

            info.Dimensions = dimensions;

            if (debug)
                Console.WriteLine($"{debugPrefix} ParsePNG - dimensions are {info.Dimensions.X}x{info.Dimensions.Y}");

            // Close our internal streams
            reader.Close();
            //deflateStream.Close();
        }

        if (CloseStream) 
            stream.Close();

        return info;
    }

    static IFilePNG ParsePNG(string filePath)
    {
        return ParsePNG(new FileStream(filePath, FileMode.Open, FileAccess.Read), true);
    }

    interface IFileInfo
    {
        public string path { get; set; }
    }

    struct IFilePNG : IFileInfo
    {
        public string path { get; set; }
        public Vector2 Dimensions { get; set; }
    }
    #endregion

    #region Burnout Data Headers
    // Unused for the time being. Compile-time
    // const arrays don't exist in C#. Sadge
    static readonly int[] DataTypeHeaders = 
    {
        -1,             // NONE
        -1,             // UNKNOWN
        1196314761,     // PNG
        845442658,      // BUNDLE2
        4539219,        // SELF
        844645720,      // XEX2
        1684559437,     // VP6
        0,              // SNS/Other
    };

    enum DataType
    {
        NONE,           // N/A
        UNKNOWN,        // N/A
        PNG,            // ‰PNG
        BUNDLE2,        // bnd2
        SELF,           // SCE�
        XEX2,           // XEX2
        VP6,            // MVhd
        SNS             // ��
    };
#endregion
}
