using System.Numerics;

namespace BrnDataHandler
{
    internal class DataHandler
    {
        public string GetRandomPrefix(string directoryPath)
        {

            Random random = new Random();
            int randomNumber = random.Next(0000000, 9999999);

            List<string> files = Directory.GetFiles(directoryPath).ToList();

            // This is really non-performant and doesn't work 100% of the time.
            // If somebody would like to improve this, be my guest.
            // I have better things to do than figure out how to get a random
            // number to not equal any number in a list.

            for (int i = 0; i < files.Count(); i++)
            {
                if (files[i].Contains("recovered_"))
                    files[i] = Path.GetFileNameWithoutExtension(files[i]).Substring(10, 5);
                else
                    files.Remove(files[i]);
            }

            if (files.Count == 0)
                return $"{randomNumber}";

            foreach (string number in files)
            {
                if (randomNumber.ToString() == number)
                    randomNumber = random.Next(0000000, 9999999);
            }

            return $"{randomNumber}";
        }

        public DataType IdentifyFileType(FileStream stream, bool CloseStream = false)
        {
            if (!stream.CanRead) return DataType.NONE;

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
                845442658 => DataType.BUNDLE2,     // BUNDLE2
                4539219 => DataType.SELF,        // SELF
                844645720 => DataType.XEX2,        // XEX2
                1684559437 => DataType.VP6,         // VP6
                _ => DataType.UNKNOWN      // UNKNOWN
            };

        }

        public DataType IdentifyFileType(string filePath)
        {
            return IdentifyFileType(new FileStream(filePath, FileMode.Open, FileAccess.Read), true);
        }

        public IFilePNG ParsePNG(FileStream stream, bool CloseStream = false)
        {
            stream.Position = 0;

            IFilePNG info = new IFilePNG
            {
                Path = stream.Name,
                Dimensions = new Vector2 { X = 0, Y = 0 }
            };

            using (BinaryReader br = new BinaryReader(stream))
            {
                // Read the bytes of the file into a byte array
                byte[] bytes = br.ReadBytes((int)stream.Length);

                // Check the first 8 bytes of the file to verify that it is a PNG file
                if (bytes[0] == 137 && 
                    bytes[1] == 80 && 
                    bytes[2] == 78 && 
                    bytes[3] == 71 && 
                    bytes[4] == 13 && 
                    bytes[5] == 10 && 
                    bytes[6] == 26 && 
                    bytes[7] == 10)
                {
                    // The width and height are stored in the bytes starting at position 16 (9th element in the array)
                    // The width is stored in 4 bytes and the height is stored in 4 bytes, so we need to read 8 bytes total
                    int width = (bytes[16] << 24) | (bytes[17] << 16) | (bytes[18] << 8) | (bytes[19]);
                    int height = (bytes[20] << 24) | (bytes[21] << 16) | (bytes[22] << 8) | (bytes[23]);

                    if (Brn.C_MainInstance.debug)
                        Console.WriteLine($"{Brn.debugPrefix} Width: {width} - Height: {height}" );

                    info.Dimensions = new Vector2()
                    {
                        X = width,
                        Y = height
                    };
                }
                else
                {
                    Console.WriteLine("ERROR: Tried to parse a non-PNG file as a PNG.");
                }
            }

            if (CloseStream)
                stream.Close();

            return info;
        }

        public IFilePNG ParsePNG(string filePath)
        {
            return ParsePNG(new FileStream(filePath, FileMode.Open, FileAccess.Read), true);
        }

        public interface IFileInfo
        {
            public string Path { get; set; }
        }

        public struct IFilePNG : IFileInfo
        {
            public string Path { get; set; }
            public Vector2 Dimensions { get; set; }
        }

        // Unused for the time being. Compile-time
        // const arrays don't exist in C#. Sadge
        public static readonly int[] DataTypeHeaders =
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

        public enum DataType
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
}
