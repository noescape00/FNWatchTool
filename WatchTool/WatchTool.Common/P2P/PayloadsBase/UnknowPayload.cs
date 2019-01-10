﻿namespace WatchTool.Common.P2P.PayloadsBase
{
    public class UnknowPayload : Payload
    {
        internal string command;

        public override string Command { get { return this.command; } }

        private byte[] data = new byte[0];

        public byte[] Data { get { return this.data; } set { this.data = value; } }

        public UnknowPayload()
        {
        }

        public UnknowPayload(string command)
        {
            this.command = command;
        }
    }
}
