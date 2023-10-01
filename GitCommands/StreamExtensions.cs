using System.Buffers.Text;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Microsoft.Toolkit.HighPerformance;

namespace GitCommands
{
    public static class StreamExtensions
    {
#if DEBUG
        // Prefix for each commit: "log size "
        private static readonly byte[] _prefix = { (byte)'l', (byte)'o', (byte)'g', (byte)' ', (byte)'s', (byte)'i', (byte)'z', (byte)'e', (byte)' ' };
#endif

        [MustUseReturnValue]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0057:Use range operator", Justification = "Performance")]
        public static IEnumerable<ReadOnlyMemory<byte>> SplitLogOutput(this Stream stream)
        {
            byte[] buffer = new byte[4096];

            // The bytes to read for next commit, possibly including the null terminator
            // after previous (avoid waiting for it after data to minimize reads)
            int allBytesToRead = 0;
#if TRACE_REVISIONREADER
            int readCount = 0;
#else
            // Dummy declarations for Trace.Write()
            const string readCount = "";
#endif

            while (true)
            {
                // Bytes that has been read for next commit
                int bytesRead = 0;

                // Position to the start of the log in the buffer (skippng e.g. the prefix)
                int logStart = 0;

                GetBytesToRead(buffer, ref allBytesToRead, ref logStart, ref bytesRead);
                if (allBytesToRead <= 0)
                {
                    // no more data to read
                    yield break;
                }

                if (allBytesToRead > buffer.Length)
                {
                    // Allocate size for next power of 2
                    int newSize = buffer.Length;
                    while (newSize < allBytesToRead)
                    {
                        newSize *= 2;
                    }

                    // Copy relevant part of existing buffer
                    byte[] newBuffer = new byte[newSize];
                    Array.Copy(buffer, logStart, newBuffer, 0, bytesRead - logStart);

                    buffer = newBuffer;
                    bytesRead -= logStart;
                    allBytesToRead -= logStart;
                    logStart = 0;
                }

                // .NET7 'stream.ReadAtLeast()' can simplify this loop
                do
                {
                    int lastRead = stream.Read(buffer, bytesRead, allBytesToRead - bytesRead);
                    if (lastRead == 0)
                    {
                        // out of sync if not all expected read
                        Trace.WriteLineIf(bytesRead < allBytesToRead, $"Read failed for commit {readCount} {bytesRead} {lastRead}/{allBytesToRead}");

                        // no more data in stream
                        yield break;
                    }

                    // Not all read is a common scenario (especially if the null terminator would be read with the preceding commit)
                    // Debug.WriteLineIf(bytesRead < size, $"Read incomplete {readCount} {bytesRead}/{lastRead}/{size}");
                    bytesRead += lastRead;
                }
                while (bytesRead < allBytesToRead);

#if TRACE_REVISIONREADER
                readCount++;
#endif

                yield return buffer.AsMemory(logStart..allBytesToRead);

                // Read the null terminator in next commit
                allBytesToRead = 1;
            }

            // Get the total bytes to read for next commit in bytesToReadNext, including the "log size" prefix.
            // bytesRead is increased, may contain bytes after the prefix.
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            void GetBytesToRead(byte[] buffer, ref int allBytesToRead, ref int logStart, ref int bytesRead)
            {
                // The min header: ignore byte (max 1) + prefix (9) + log size chars + newline (1)
                // log size is minimum 2 chars as a log can never be less than 89 bytes,
                // reasonable max is 5 chars (linux about 36KB).
                // Read some more than necessary (to be efficient),
                // but be satisfied with a 21 byte header (at least 10 chars, well out of bounds).
                const int prefixLength = 9;
                const int readHeaderLength = 21;
                int firstPossibleNewlineIndex = allBytesToRead + prefixLength + 2;

                do
                {
                    Debug.Assert(!Debugger.IsAttached || bytesRead < readHeaderLength, "Size is larger than header buffer {readCount} {bytesRead}/{ignoreBytes} {lastRead}");
                    int lastRead = stream.Read(buffer, bytesRead, readHeaderLength - bytesRead);
                    if (lastRead == 0)
                    {
                        // No more data in stream if zero read
                        if (bytesRead > 0)
                        {
                            Trace.WriteLine($"Only partial log size header received for commit {readCount} {bytesRead}/{allBytesToRead} {lastRead}");
                            allBytesToRead = -1;
                            return;
                        }

                        // No usable data at eof, nothing to be read
                        Debug.WriteLineIf(bytesRead >= buffer.Length, $"No log size header received for commit {readCount} {bytesRead}/{allBytesToRead} {lastRead}");
                        allBytesToRead = 0;
                        return;
                    }

                    bytesRead += lastRead;
                }
                while (bytesRead < readHeaderLength);

#if DEBUG
                if (buffer.AsSpan(allBytesToRead, _prefix.Length).SequenceCompareTo<byte>(_prefix) != 0)
                {
                    Trace.WriteLine($"Unexpected log size header for commit {readCount} {bytesRead}/{allBytesToRead} {buffer[bytesRead - 1]}");
                    allBytesToRead = -1;
                    return;
                }
#endif

                int newlineIndex = Array.IndexOf(buffer, (byte)'\n', firstPossibleNewlineIndex, bytesRead - firstPossibleNewlineIndex);
                if (newlineIndex < 0)
                {
                    Trace.WriteLine($"Only partial log size header received for commit {readCount} {bytesRead}/{allBytesToRead} {buffer[bytesRead - 1]}");
                    allBytesToRead = -1;
                    return;
                }

                logStart = newlineIndex + 1;
                if (!Utf8Parser.TryParse(buffer.AsSpan(allBytesToRead + prefixLength, logStart - allBytesToRead - prefixLength - 1), out int logSize, out int _))
                {
                    Trace.WriteLine($"Cannot parse size in log size header for commit {readCount} {bytesRead}/{allBytesToRead} {buffer[bytesRead - 1]}");
                    allBytesToRead = -1;
                    return;
                }

                allBytesToRead = logStart + logSize;
            }
        }
    }
}
