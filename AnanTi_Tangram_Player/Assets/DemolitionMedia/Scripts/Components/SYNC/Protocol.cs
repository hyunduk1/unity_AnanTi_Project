﻿using System.IO;


namespace DemolitionStudios.DemolitionMedia
{
    using ClockType = System.Double;

    class Protocol
    {
        public enum PacketId : byte
        {
            RoundTripPing = 0,
            RoundTripPong = 1,
            Sync          = 2,
            Speed         = 3,
            Pause         = 4,
        }

        private void InitWriter(int size)
        {
            m_buffer = new byte[size];
            m_stream = new MemoryStream(m_buffer);
            m_writer = new BinaryWriter(m_stream);
        }

        private void InitReader(byte[] buffer)
        {
            m_stream = new MemoryStream(buffer);
            m_reader = new BinaryReader(m_stream);
        }

        public byte[] Serialize(byte code)
        {
            const int bufSize = sizeof(byte);
            InitWriter(bufSize);
            m_writer.Write(code);
            return m_buffer;
        }

        public byte[] Serialize(byte code, bool value)
        {
            const int bufSize = sizeof(byte) + sizeof(bool);
            InitWriter(bufSize);
            m_writer.Write(code);
            m_writer.Write(value);
            return m_buffer;
        }

        public byte[] Serialize(byte code, ClockType value)
        {
            const int bufSize = sizeof(byte) + sizeof(ClockType);
            InitWriter(bufSize);
            m_writer.Write(code);
            m_writer.Write(value);
            return m_buffer;
        }

        public byte[] Serialize(byte code, ulong timestamp, ClockType value)
        {
            const int bufSize = sizeof(byte) + sizeof(ulong) + sizeof(ClockType);
            InitWriter(bufSize);
            m_writer.Write(code);
            m_writer.Write(timestamp);
            m_writer.Write(value);
            return m_buffer;
        }

        public void Deserialize(byte[] buf, out byte code, out ClockType value)
        {
            InitReader(buf);
            m_stream.Write(buf, 0, buf.Length);
            m_stream.Position = 0;
            code = m_reader.ReadByte();
            value = m_reader.ReadSingle();
        }

        private BinaryWriter m_writer;
        private BinaryReader m_reader;
        private MemoryStream m_stream;
        private byte[] m_buffer;
    }
}