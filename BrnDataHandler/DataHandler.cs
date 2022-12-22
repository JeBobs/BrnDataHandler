using System.Globalization;
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

        public DataType IdentifyFileType(FileStream stream, bool CloseStream = false)
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
                    845442658 => DataType.BUNDLE2,     // BUNDLE2
                    4539219 => DataType.SELF,        // SELF
                    844645720 => DataType.XEX2,        // XEX2
                    1684559437 => DataType.VP6,         // VP6
                    _ => DataType.UNKNOWN      // UNKNOWN
                };
            }

            return DataType.NONE;
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
                path = stream.Name,
                Dimensions = new Vector2 { X = 0, Y = 0 }
            };

            //using (DeflateStream deflateStream = new DeflateStream(stream, CompressionMode.Decompress))

            using (BinaryReader reader = new BinaryReader(stream))
            {
                // Check the PNG signature
                uint header = reader.ReadUInt32();

                if (Brn.C_MainInstance.debug)
                    Console.WriteLine($"{Brn.debugPrefix} ParsePNG - prefix is {header}");

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

                if (Brn.C_MainInstance.debug)
                    Console.WriteLine($"{Brn.debugPrefix} ParsePNG - dimensions are {info.Dimensions.X}x{info.Dimensions.Y}");

                // Close our internal streams
                reader.Close();
                //deflateStream.Close();
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
            public string path { get; set; }
        }

        public struct IFilePNG : IFileInfo
        {
            public string path { get; set; }
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
