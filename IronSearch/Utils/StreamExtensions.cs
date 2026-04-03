using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronSearch.Utils
{
    public static class StreamExtensions
    {

        public static byte[] ReadFully(Stream stream, int initialLength = 0)
        {
            if (initialLength < 1)
            {
                initialLength = 32768;
            }

            byte[] buffer = new byte[initialLength];
            int read = 0;

            int chunk;
            while ((chunk = stream.Read(buffer, read, buffer.Length - read)) > 0)
            {
                read += chunk;

                if (read == buffer.Length)
                {
                    int nextByte = stream.ReadByte();
                    if (nextByte == -1)
                    {
                        return buffer;
                    }
                    byte[] newBuffer = new byte[buffer.Length * 2];
                    Array.Copy(buffer, newBuffer, buffer.Length);
                    newBuffer[read] = (byte)nextByte;
                    buffer = newBuffer;
                    read++;
                }
            }
            byte[] ret = new byte[read];
            Array.Copy(buffer, ret, read);
            return ret;
        }
        internal static Stream StringToStream(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream, Encoding.ASCII);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        public static MemoryStream CopyToMemory(this Stream input)
        {
            var ms = new MemoryStream();
            input.CopyTo(ms);
            ms.Position = 0;
            return ms;
        }
    }
}
