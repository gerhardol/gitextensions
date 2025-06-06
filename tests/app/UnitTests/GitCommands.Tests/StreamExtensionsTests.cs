﻿using GitCommands;

namespace GitCommandsTests
{
    [TestFixture]
    public sealed class StreamExtensionsTests
    {
        private const byte nil = 0;

        [TestCase(
            new byte[] { })]
        [TestCase(
            new byte[] { 1 },
            new byte[] { 1 })]
        [TestCase(
            new byte[] { nil },
            new byte[0])]
        [TestCase(
            new byte[] { nil, nil },
            new byte[0], new byte[0])]
        [TestCase(
            new byte[] { 1, 2, 3, 4, 5, 6 },
            new byte[] { 1, 2, 3, 4, 5, 6 })]
        [TestCase(
            new byte[] { 2, 3, 4, 5, 6, nil },
            new byte[] { 2, 3, 4, 5, 6 })]
        [TestCase(
            new byte[] { nil, 1, 2, 3, 4, 5, 6 },
            new byte[0], new byte[] { 1, 2, 3, 4, 5, 6 })]
        [TestCase(
            new byte[] { 1, 2, 3, nil, 4, 5, 6 },
            new byte[] { 1, 2, 3 }, new byte[] { 4, 5, 6 })]
        [TestCase(
            new byte[] { 1, 2, 3, nil, nil, 4, 5, 6 },
            new byte[] { 1, 2, 3 }, new byte[0], new byte[] { 4, 5, 6 })]
        [TestCase(
            new byte[] { 1, 2, 3, nil, 4, 5, 6, nil, 7, 8, 9 },
            new byte[] { 1, 2, 3 }, new byte[] { 4, 5, 6 }, new byte[] { 7, 8, 9 })]
        [TestCase(
            new byte[] { nil, 1, nil, 2, 3, nil, 4, 5, 6, nil, 7, 8, 9, 10 },
            new byte[0], new byte[] { 1 }, new byte[] { 2, 3 }, new byte[] { 4, 5, 6 }, new byte[] { 7, 8, 9, 10 })]
        public void ReadNullTerminatedLines(byte[] input, params byte[][] expectedChunks)
        {
            MemoryStream stream = new(input);

            // Run the test at multiple buffer sizes to test boundary conditions thoroughly
            for (int bufferSize = 1; bufferSize < input.Length + 2; bufferSize++)
            {
                stream.Position = 0;

                using IEnumerator<ReadOnlyMemory<byte>> e = stream.SplitLogOutput(bufferSize).GetEnumerator();
                for (int chunkIndex = 0; chunkIndex < expectedChunks.Length; chunkIndex++)
                {
                    byte[] expected = expectedChunks[chunkIndex];
                    ClassicAssert.IsTrue(e.MoveNext());
                    ClassicAssert.AreEqual(
                        expected,
                        e.Current.ToArray(),
                        $"input=[{string.Join(",", expected)}] chunkIndex={chunkIndex} bufferSize={{bufferSize}}");
                }

                ClassicAssert.IsFalse(e.MoveNext(), $"bufferSize={bufferSize}");
            }
        }
    }
}
