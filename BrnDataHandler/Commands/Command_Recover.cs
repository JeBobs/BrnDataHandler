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
                        ProcessBurnoutPNG(Brn.C_DataHandler.ParsePNG(stream), out newPath);
                        newExtension = "png";
                        break;
                    case DataHandler.DataType.BUNDLE2:
                        newExtension =
                            currentExtension    == "BIN"
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
            string directory = filePNG.Path != null ? Path.GetDirectoryName(filePNG.Path) : "";

            // Determine if PNG is PS3 art
            switch (filePNG.Dimensions)
            {
                // PIC1.PNG
                case { X: 1920, Y: 1080 }:
                case { X: 1280, Y: 720 }:
                    filePNG.Path =
                        Path.Combine(directory, $"recovered_{Brn.C_DataHandler.GetRandomPrefix(directory)}_PIC1.PNG");
                    break;

                // ICON0.PNG
                case { X: 320, Y: 176 }:
                    filePNG.Path =
                        Path.Combine(directory, $"recovered_{Brn.C_DataHandler.GetRandomPrefix(directory)}_ICON0.PNG");
                    break;
                // Unknown Name (PS3 Logo)
                case { X: 640, Y: 25 }:
                    filePNG.Path =
                        Path.Combine(directory, $"recovered_{Brn.C_DataHandler.GetRandomPrefix(directory)}_PS3.PNG");
                    break;
            }
            outPath = filePNG.Path;
            return true;
        }

        public void ProcessBurnoutPNG(string filePath, out string outPath)
        {
            ProcessBurnoutPNG(Brn.C_DataHandler.ParsePNG(filePath), out outPath);
        }
    }
}
