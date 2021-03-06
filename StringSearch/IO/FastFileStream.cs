﻿/*
 * PiSearch
 * FastFileStream - a fast implementation of Stream, with the underlying data being stored on a file (like FileStream)
 * By Josh Keegan 02/01/2015
 * Last Edit 09/06/2016
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StringSearch.IO
{
    public class FastFileStream : Stream
    {
        //Private variables
        private readonly byte[] buffer;
        private readonly WinFileIO wfio = null;
        private readonly FileAccess fileAccess;

        public override bool CanRead => wfio != null && fileAccess != FileAccess.Write;

        public override bool CanSeek => wfio != null;

        public override bool CanWrite => wfio != null && fileAccess != FileAccess.Read;

        public override void Flush() 
        {
            throw new NotImplementedException();
            //TODO: Check if there is a Win API call that could be made by WinFileIO to flush
        }

        public override long Length
        {
            get 
            {
                throwIfClosed();

                return wfio.Length;
            }
        }

        public override long Position
        {
            get
            {
                throwIfClosed();

                return wfio.Position;
            }
            set
            {
                throwIfClosed();

                wfio.Position = value;
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            //Validation
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(offset), "must be >= 0");
            }

            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count), "must be >= 0");
            }

            if (buffer.Length - offset < count)
            {
                throw new ArgumentException(nameof(buffer) + " is too small to read " + nameof(count) +
                                            " bytes starting at index " + nameof(offset));
            }

            throwIfClosed();

            if (wfio.Position >= wfio.Length || count == 0)
            {
                return 0;
            }

            //If the end of the stream is going to come before reading count bytes, correct count
            if (wfio.Position > wfio.Length - count)
            {
                count = (int)(wfio.Length - wfio.Position);
            }

            //Read in the requested bytes
            //  WFIO reads into the buffer from this class, so can only read a maximum of this.buffer.Length bytes at a time
            int bytesRead = 0;
            do
            {
                //How many to read in this iteration
                int readCount = Math.Min(this.buffer.Length, count - bytesRead);

                //Read them in
                int actualReadCount = wfio.ReadBlocks(readCount);

                //Copy the contents of the WFIO buffer into the Read buffer
                Array.Copy(this.buffer, 0, buffer, bytesRead + offset, actualReadCount);

                bytesRead += actualReadCount;
            }
            while (bytesRead < count);

            return bytesRead;
        }

        //TODO: ReadByte() Implementation would be more efficient than relying on Read(byte[], int, int) & would get used

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            //Validation
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(offset), "must be >= 0");
            }

            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count), "must be >= 0");
            }

            if (buffer.Length - offset < count)
            {
                throw new ArgumentException(nameof(buffer) + " is too small to write " + nameof(count) +
                                            " bytes starting at index " + nameof(offset));
            }

            throwIfClosed();

            // WFIO writes from the buffer from the class, so can only write a maximum of this.buffer.Length bytes at a time
            int bytesWritten = 0;
            do
            {
                //How many to write in this iteration
                int writeCount = Math.Min(this.buffer.Length, count - bytesWritten);

                //Copy the contents of the write buffer into the WFIO buffer
                Array.Copy(buffer, bytesWritten + offset, this.buffer, 0, writeCount);

                //Write them out
                int actualWriteCount = wfio.WriteBlocks(writeCount);

                bytesWritten += actualWriteCount;
            }
            while (bytesWritten < count);
        }

        ~FastFileStream()
        {
            Dispose(false);
        }

        protected override void Dispose(bool disposing)
        {
            if(wfio != null)
            {
                wfio.Close();
            }

            base.Dispose(disposing);
        }

        public FastFileStream(string path, FileAccess fileAccess, int maxReadWriteCallSize = WinFileIO.BLOCK_SIZE,
            WinFileFlagsAndAttributes? flagsAndAttributes = null)
        {
            if(path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            if(path.Trim() == "")
            {
                throw new ArgumentException("path is empty", nameof(path));
            }

            if(fileAccess < FileAccess.Read || fileAccess > FileAccess.ReadWrite)
            {
                throw new ArgumentOutOfRangeException(nameof(fileAccess),
                    "must contain a valid value for the FileAccess enum");
            }

            if(maxReadWriteCallSize < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(maxReadWriteCallSize), "must be > 0");
            }

            if(path.IndexOfAny(Path.GetInvalidPathChars()) != -1)
            {
                throw new ArgumentException("contains invalid characters", nameof(path));
            }

            if(Directory.Exists(path))
            {
                //Seems an odd choice of exception, but it's what FileStream does
                throw new UnauthorizedAccessException("The specified path points to a directory, must be a file");
            }

            //If opening file for read only, but the file doesn't exist, throw FileNotFoundException
            if (fileAccess == FileAccess.Read && !File.Exists(path))
            {
                throw new FileNotFoundException("path not found");
            }

            //TODO: Could still be problems accessing the file due to permissions or it being locked by another process
            //  In these cases, currently the Exception thrown by WinFileIO will get propagated. Could improve this with better Exceptions
            buffer = new byte[maxReadWriteCallSize];
            wfio =  new WinFileIO(buffer);
            
            //If no flags and attributes were provided
            if(flagsAndAttributes == null)
            {
                switch (fileAccess)
                {
                    case FileAccess.Read:
                        wfio.OpenForReading(path);
                        break;
                    case FileAccess.Write:
                        wfio.OpenForWriting(path);
                        break;
                    case FileAccess.ReadWrite:
                        wfio.OpenForReadingWriting(path);
                        break;
                }
            }
            else //Otherwise use the flags and attributes provided
            {
                switch (fileAccess)
                {
                    case FileAccess.Read:
                        wfio.OpenForReading(path, flagsAndAttributes.GetValueOrDefault());
                        break;
                    case FileAccess.Write:
                        wfio.OpenForWriting(path, flagsAndAttributes.GetValueOrDefault());
                        break;
                    case FileAccess.ReadWrite:
                        wfio.OpenForReadingWriting(path, flagsAndAttributes.GetValueOrDefault());
                        break;
                }
            }

            this.fileAccess = fileAccess;
        }

        //Helpers
        private void throwIfClosed()
        {
            if(wfio == null)
            {
                throw new ObjectDisposedException("Stream has been closed");
            }
        }
    }
}
