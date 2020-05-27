using System;
using System.Linq;
using System.Collections.Generic;

namespace mock_sns_core2.Services
{
    public static class FileSignatureService
    {

        public static int MaxImageBytes = 8;
        public static int MaxVideoBytes = 12;

        private static readonly Dictionary<string, List<byte[]>> _imageSignature =
            new Dictionary<string, List<byte[]>>
            {
                {
                    "image/jpeg", new List<byte[]>
                    {
                        new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 },
                        new byte[] { 0xFF, 0xD8, 0xFF, 0xE1 },
                        new byte[] { 0xFF, 0xD8, 0xFF, 0xE2 },
                        new byte[] { 0xFF, 0xD8, 0xFF, 0xE3 },
                    }
                },
                {
                    "image/gif", new List<byte[]>
                    {
                        new byte[] { 0x47, 0x49, 0x46, 0x38, 0x37, 0x61 },
                        new byte[] { 0x47, 0x49, 0x46, 0x38, 0x39, 0x61 },
                    }
                },
                {
                    "image/bmp", new List<byte[]>
                    {
                        new byte[] { 0x42, 0x4D },
                    }
                },
                {
                    "image/png", new List<byte[]>
                    {
                        new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A },
                    }
                }
            };

        private static readonly Dictionary<string, List<byte[]>> _videoSignature =
            new Dictionary<string, List<byte[]>>
            {
                {
                    "video/mp4", new List<byte[]>
                    {
                        new byte[] { 0x00, 0x00, 0x00, 0x18, 0x66, 0x74, 0x79, 0x70, 0x6D, 0x70, 0x34, 0x32 },
                        new byte[] { 0x00, 0x00, 0x00, 0x20, 0x66, 0x74, 0x79, 0x70, 0x69, 0x73, 0x6F, 0x6D },
                    }
                },
            };

        public static string checkImageSignature(byte[] argBytes)
        {
            foreach (var(id, list) in _imageSignature)
            {
                if(list.Any(bytes => argBytes.Take(bytes.Length).SequenceEqual(bytes)))
                {
                    return id;
                }
            }

            return "";
        }

        public static string checkVideoSignature(byte[] argBytes)
        {
            foreach (var (id, list) in _videoSignature)
            {
                if (list.Any(bytes => argBytes.Take(bytes.Length).SequenceEqual(bytes)))
                {
                    return id;
                }
            }

            return "";
        }
    }
}
