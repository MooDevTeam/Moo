using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using System.Net.Sockets;
using Moo.Core.Utility;
namespace Moo.Core.Tester.MooTester
{
    public class Out
    {
        public enum ResultType : uint
        {
            Success, WrongAnswer, TimeLimitExceeded, RuntimeError, MemoryLimitExceeded, CompareError,OutputLimitExceeded
        }

        public ResultType Type { get; set; }
        public long Time { get; set; }
        public long Memory { get; set; }
        public string Message;

        void ReadIntoBuffer(Socket sock, byte[] buf)
        {
            int haveRead = 0;
            while (haveRead < buf.Length)
            {
                int currentRead= sock.Receive(buf, haveRead, buf.Length - haveRead, SocketFlags.None);
                if (currentRead == 0)
                {
                    throw new Exception("ReadIntoBuffer No Enough Bytes!");
                }
                haveRead += currentRead;
            }
        }

        public Out(Socket sock)
        {
            byte[] buf = new byte[sizeof(uint) + sizeof(uint) + sizeof(long) + sizeof(long)];
            ReadIntoBuffer(sock, buf);

            uint messageLength;
            using (MemoryStream stream = new MemoryStream(buf))
            {
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    messageLength = reader.ReadUInt32();
                    Type = (ResultType)reader.ReadUInt32();
                    Time = reader.ReadInt64();
                    Memory = reader.ReadInt64();
                }
            }

            if (messageLength > 0)
            {
                buf = new byte[messageLength];
                sock.Receive(buf);
                Message = Encoding.Default.GetString(buf);
            }
            else
            {
                Message = "";
            }
        }

        public override string ToString()
        {
            return "[Out Type=" + Type + " Time=" + Time + " Memory=" + Memory + " Messsage=" + Message + "]";
        }
    }

    public class Message
    {
        public enum MessageType : uint
        {
            Compile = 1, Test = 2
        }

        public MessageType Type { get; set; }
        public IMessageContent Content { get; set; }

        public byte[] ToBytes()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.Write((uint)Type);
                    byte[] contentBytes = Content.ToBytes();
                    writer.Write((uint)contentBytes.LongLength);
                    writer.Write(contentBytes);
                }
                return stream.ToArray();
            }
        }
    }

    public interface IMessageContent
    {
        byte[] ToBytes();
    }

    public class CompileIn : IMessageContent
    {
        public long Time { get; set; }
        public long Memory { get; set; }
        public string Code { get; set; }
        public string Command { get; set; }

        public byte[] ToBytes()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.Write(Time);
                    writer.Write(Memory);
                    byte[] code = Encoding.Default.GetBytes(Code);
                    writer.Write((uint)code.Length);
                    writer.Write(code);
                    writer.Write(Encoding.Default.GetBytes(Command));
                    writer.Write((byte)0);
                }
                return stream.ToArray();
            }
        }
    }

    public class TestIn : IMessageContent
    {
        public long Time { get; set; }
        public long Memory { get; set; }
        public string ExecPath { get; set; }
        public string CmpPath { get; set; }
        public byte[] Input { get; set; }
        public byte[] Output { get; set; }

        public byte[] ToBytes()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    byte[] execPath = Encoding.Default.GetBytes(ExecPath);
                    byte[] cmpPath = Encoding.Default.GetBytes(CmpPath);
                    writer.Write((uint)0);
                    writer.Write((uint)(execPath.Length + 1));
                    writer.Write((uint)(execPath.Length + 1 + Input.Length));
                    writer.Write((uint)(execPath.Length + 1 + Input.Length + Output.Length));
                    writer.Write(Time);
                    writer.Write(Memory);
                    writer.Write(execPath);
                    writer.Write((byte)0);
                    writer.Write(Input);
                    writer.Write(Output);
                    writer.Write(cmpPath);
                    writer.Write((byte)0);
                }
                return stream.ToArray();
            }
        }
    }
}
