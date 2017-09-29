//
// $Workfile: EnvisionLoggingReader.cs$
// $Revision: 2$
// $Author: tlyne$
// $Date: Thursday, January 24, 2008 11:19:09 AM$
//
// Copyright © Pivotal Corporation
//


using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

namespace CdcSoftware.Pivotal.Applications.HomeBuilders.EF.Server
{
    /// <summary>
    /// Custom TextReader that creates a copy of the stream for logging.
    /// </summary>
    public sealed class EnvisionLoggingReader : System.IO.TextReader
    {
        // the stream to read from
        private System.IO.TextReader m_streamReader;

        // the copy of the stream to write to.
        private System.IO.StringWriter m_logWriter;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="stream">The base stream to read from.</param>
        /// <param name="soapXml">A reference to the copy of the stream.</param>
        /// <param name="bufferSize">Size of the stream to read.</param>
        public EnvisionLoggingReader(System.IO.Stream stream, out StringBuilder soapXml, int bufferSize)
        {
            // initializes the stream copy
            soapXml = new StringBuilder(bufferSize);

            // create a writer to write to the copy
            this.m_logWriter = new System.IO.StringWriter(soapXml, CultureInfo.CurrentCulture);

            // create a reader to read from the stream
            this.m_streamReader = new System.IO.StreamReader(stream);
        }


        /// <summary>
        /// Closes the open streams
        /// </summary>
        public override void Close()
        {
            this.m_logWriter.Close();
            this.m_streamReader.Close();
            base.Close();
        }

        /// <summary>
        /// Not implemented
        /// </summary>
        /// <returns>n/a</returns>
        public override int Peek()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Reads one character at a time.
        /// </summary>
        /// <returns>The character read.</returns>
        public override int Read()
        {
            char c = (char)this.m_streamReader.Read();
            this.m_logWriter.Write(c);
            return c;
        }

        /// <summary>
        /// Not Implemented
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="index"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public override int ReadBlock(char[] buffer, int index, int count)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Not Implemented
        /// </summary>
        /// <returns></returns>
        public override string ReadToEnd()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Reads a maximum of count characters from the current stream and writes the data to buffer, beginning at index 
        /// </summary>
        /// <param name="buffer">The character buffer from which to read</param>
        /// <param name="index">The start position within the buffer</param>
        /// <param name="count">The number of characters to read</param>
        /// <returns>The number of characters read</returns>
        public override int Read(char[] buffer, int index, int count)
        {
            int rt = this.m_streamReader.Read(buffer, index, count);

            // make a copy of the stream
            char[] subBuffer = new char[rt];
            Array.Copy(buffer, index, subBuffer, 0, rt);

            // seems to be a bug with .Write(char[], int, int) using .Write(char[]) instead.
            this.m_logWriter.Write(subBuffer);

            return rt;
        }

        /// <summary>
        /// Not Implemented
        /// </summary>
        /// <returns></returns>
        public override string ReadLine()
        {
            throw new NotImplementedException();
        }
    }
}
