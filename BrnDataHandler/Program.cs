internal class Program
{
    // Switches
    static bool debug = false;
    static bool convertExtensions = false;
    static bool keepExtensions = true;
    static bool sortByType = false;

    private static void Main(string[] args)
    {
        if (args.Length > 0)
        {
            List<string> directories = new List<string>();
            List<string> files = new List<string>();
            List<string> switches = new List<string>();

            foreach (var arg in args)
            {
                Console.WriteLine($"Launched with argument {arg}");

                if (Directory.Exists(arg))
                    directories.Add(arg);

                else if (File.Exists(arg))
                    files.Add(arg);

                else
                    switches.Add(arg.ToLower());

                // Really hacky system to do quick debugging.
                // Gotta replace with a proper switch parsing system.
                foreach (string sw in switches)
                {
                    if (sw == "--debug")
                        debug = true;

                    if (sw == "--convert-extensions")
                        convertExtensions = true;
                        keepExtensions = false;

                    if (sw == "--keep-original-extensions")
                        keepExtensions = true;
                }
            }

            foreach (string filePath in files)
            {
                ProcessFileAsync(filePath);
            }

            foreach (string directoryPath in directories)
            {
                ProcessFolder(directoryPath);
            }
        }
        else
        {
            ProcessFolder(Directory.GetCurrentDirectory());
        }
    }

    static bool ProcessFolder(string directory)
    {
        string[] filePaths = Directory.GetFiles(directory);

        foreach (string filePath in filePaths)
        {
            ProcessFileAsync(filePath);
        }

        return true;
    }


    static async Task<bool> ProcessFileAsync(string filePath)
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
            newPath = keepExtensions
                ? $"{filePath}"
                : $"{filePath.Substring(0, filePath.Length - currentExtension.Length)}";

            currentExtension.Substring(1);

            // Set extension
            // TODO: Further parse data to more accurately determine data 
            switch (type)
            {
                case DataType.PNG:
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
            
            newPath += $".{newExtension}";

            File.Move(filePath, newPath);
        }
        
        return true;
    }

    static DataType IdentifyFileType(string filePath)
    {
        FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
    
        if (stream.CanRead)
        {
            byte[] headerBytes = new byte[4];
    
            if (stream.Read(headerBytes, 0, 2) < 2)
            {
                stream.Close();
                return DataType.UNKNOWN;
            }
            else if (BitConverter.ToUInt32(headerBytes) == 0)
            {
                stream.Close();
                return DataType.SNS;
            }
            else if (stream.Read(headerBytes, 2, 2) < 2)
            {
                stream.Close();
                return DataType.UNKNOWN;
            }
            else
            {
                stream.Close();

                uint header = BitConverter.ToUInt32(headerBytes);
    
                switch (header)
                {
                    case 1196314761:                // PNG
                        return DataType.PNG;
                    case 845442658:                 // BUNDLE2
                        return DataType.BUNDLE2;
                    case 4539219:                   // SELF
                        return DataType.SELF;
                    case 844645720:                 // XEX2
                        return DataType.XEX2;
                    case 1684559437:                // VP6
                        return DataType.VP6;
                    default:                        // UNKNOWN
                        return DataType.UNKNOWN;
                }
            }
        }
    
        return DataType.NONE;
    }


    // Unused for the time being. Compile-time
    // const arrays don't exist in C#. Sadge
    readonly static int[] DataTypeHeaders = 
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
}
