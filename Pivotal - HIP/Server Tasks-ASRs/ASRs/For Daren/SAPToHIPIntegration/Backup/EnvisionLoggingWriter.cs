//
// $Workfile: EnvisionLoggingWriter.cs$
// $Revision: 8$
// $Author: tlyne$
// $Date: Thursday, March 01, 2007 5:24:09 PM$
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
    /// Custom TextWriter that makes a copy of the stream for logging purposes
    /// </summary>
    public sealed class EnvisionLoggingWriter : System.IO.TextWriter
    {

        // the base stream writer
        private System.IO.TextWriter m_streamWriter;

        // the string writer that writes a copy of the text stream
        private System.IO.StringWriter m_logWriter;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="destStream">The main stream to write to.</param>
        /// <param name="soapXml">The string buffer that recieves a copy of the stream.</param>
        /// <param name="bufferSize">The size of the stream.</param>
        public EnvisionLoggingWriter(System.IO.Stream destStream, StringBuilder soapXml, int bufferSize): base(CultureInfo.CurrentCulture)
        {
            // use a writer to write the stream copy 
            this.m_logWriter = new System.IO.StringWriter(soapXml, CultureInfo.CurrentCulture);

            // use a writer to write to the main stream
            this.m_streamWriter = new System.IO.StreamWriter(destStream, System.Text.Encoding.UTF8, bufferSize);
        }


        /// <summary>
        /// Closes the main stream and the copy of the stream.
        /// </summary>
        public override void Close()
        {
            this.m_logWriter.Close();
            this.m_streamWriter.Close();
            base.Close();
        }

        /// <summary>
        /// Flush both streams.
        /// </summary>
        public override void  Flush()
        {
            this.m_logWriter.Flush();
            this.m_streamWriter.Flush();
 	         //base.Flush();
        }

        /// <summary>
        /// Write to both streams
        /// </summary>
        /// <param name="buffer">The character buffer for this write operation</param>
        /// <param name="index">The index of the character buffer</param>
        /// <param name="count">The number of characters to write</param>
        public override void Write(char[] buffer, int index, int count)
        {
            this.m_logWriter.Write(buffer, index, count);
            this.m_streamWriter.Write(buffer, index, count); 
        }

        /// <summary>
        /// Sets the text encoding.
        /// </summary>
        public override Encoding Encoding
        {
            get 
            {
                return this.m_streamWriter.Encoding;
            }
        }
    }
}
