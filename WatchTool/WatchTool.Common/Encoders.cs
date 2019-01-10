﻿using System;
using System.Linq;

namespace WatchTool.Common
{
    public abstract class DataEncoder
    {
        // char.IsWhiteSpace fits well but it match other whitespaces
        // characters too and also works for unicode characters.
        public static bool IsSpace(char c)
        {
            switch (c)
            {
                case ' ':
                case '\t':
                case '\n':
                case '\v':
                case '\f':
                case '\r':
                    return true;
            }
            return false;
        }

        internal DataEncoder()
        {
        }

        public string EncodeData(byte[] data)
        {
            return EncodeData(data, 0, data.Length);
        }

        public abstract string EncodeData(byte[] data, int offset, int count);

        public abstract byte[] DecodeData(string encoded);
    }

    public class ASCIIEncoder : DataEncoder
    {
        //Do not using Encoding.ASCII (not portable)
        public override byte[] DecodeData(string encoded)
        {
            if (String.IsNullOrEmpty(encoded))
                return new byte[0];
            return encoded.ToCharArray().Select(o => (byte)o).ToArray();
        }

        public override string EncodeData(byte[] data, int offset, int count)
        {
            return new String(data.Skip(offset).Take(count).Select(o => (char)o).ToArray()).Replace("\0", "");
        }
    }

    public static class Encoders
    {
        private static readonly ASCIIEncoder _ASCII = new ASCIIEncoder();
        public static DataEncoder ASCII
        {
            get
            {
                return _ASCII;
            }
        }
    }
}
