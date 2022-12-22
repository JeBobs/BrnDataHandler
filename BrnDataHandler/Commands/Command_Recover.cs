using System.Numerics;

namespace BrnDataHandler.Commands
{
    internal class Command_Recover : Command
    {
        public override bool Run()
        {
            if (Brn.C_MainInstance.Files.Count > 0 || Brn.C_MainInstance.Directories.Count > 0)
            {
                foreach (string filePath in Brn.C_MainInstance.Files)
                {
                    RecoverFileAsync(filePath);
                }
                foreach (string directoryPath in Brn.C_MainInstance.Directories)
                {
                    RecoverFolder(directoryPath);
                }
            }
            else
            {
                RecoverFolder(Directory.GetCurrentDirectory());
            }

            return true;
        }

        public bool RecoverFolder(string directory)
        {
            string[] filePaths = Directory.GetFiles(directory);

            foreach (string filePath in filePaths)
            {
                RecoverFileAsync(filePath);
            }

            return true;
        }

        public async Task<bool> RecoverFileAsync(string filePath)
        {
            DataHandler.DataType type = Brn.C_DataHandler.IdentifyFileType(filePath);

            string newExtension = "";

            if (type == DataHandler.DataType.NONE || type == DataHandler.DataType.UNKNOWN)
            {
                Console.WriteLine($"Skipping unknown file {filePath}...");
                return false;
            }

            string currentExtension = Path.GetExtension(filePath);

            Console.WriteLine($"Processing {Enum.GetName(typeof(DataHandler.DataType), type)} file {filePath}...");

            if (Brn.C_MainInstance.convertExtensions)
            {
                //newPath = keepExtensions
                //    ? $"{filePath}"
                //    : $"{filePath.Substring(0, filePath.Length - currentExtension.Length)}";

                string newPath = filePath;

                currentExtension.Substring(1);

                FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);

                // Set extension
                // TODO: Further parse data to more accurately determine data 
                switch (type)
                {
                    case DataHandler.DataType.PNG:
                        // TODO: Process PNGs properly
                        // ProcessBurnoutPNG(ParsePNG(stream), out newPath);
                        newExtension = "png";
                        break;
                    case DataHandler.DataType.BUNDLE2:
                        newExtension =
                            currentExtension == "BIN"
                            || currentExtension == "BNDL"
                            || currentExtension == "DAT"
                                ? currentExtension
                                : "BUNDLE";
                        break;
                    case DataHandler.DataType.SELF:
                        newExtension = "self";
                        break;
                    case DataHandler.DataType.XEX2:
                        newExtension = "xex";
                        break;
                    case DataHandler.DataType.VP6:
                        newExtension = "VP6";
                        break;
                    case DataHandler.DataType.SNS:
                        newExtension = "SNS";
                        break;
                }

                stream.Close();

                if (Brn.C_MainInstance.convertExtensions)
                    newPath = Path.ChangeExtension(newPath, $".{newExtension}");

                File.Move(filePath, newPath);
            }

            return true;
        }

        public bool ProcessBurnoutPNG(DataHandler.IFilePNG filePNG, out string outPath)
        {
            string directory = filePNG.path != null ? Path.GetDirectoryName(filePNG.path) : "";

            // Determine if PNG is PS3 art
            switch (filePNG.Dimensions)
            {
                // PIC1.PNG
                case Vector2 a when a.X == 1920 && a.Y == 1080:
                case Vector2 b when b.X == 1280 && b.Y == 720:
                    filePNG.path =
                        Path.Combine(directory, $"recovered_{Brn.C_DataHandler.GetRandomPrefix(directory)}PIC1.PNG");
                    break;

                //ICON0.PNG
                case Vector2 c when c.X == 320 && c.Y == 176:
                    filePNG.path =
                        Path.Combine(directory, $"recovered_{Brn.C_DataHandler.GetRandomPrefix(directory)}ICON0.PNG");
                    break;
            }
            outPath = filePNG.path;
            return true;
        }

        public void ProcessBurnoutPNG(string filePath, out string outPath)
        {
            ProcessBurnoutPNG(Brn.C_DataHandler.ParsePNG(filePath), out outPath);
        }
    }
}
