﻿/*
 * PiSearch
 * FourBitDigitArray Unit Tests
 * By Josh Keegan 27/11/2014
 * Last Edit 24/03/2016
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework;
using StringSearch;
using StringSearch.Collections;
using StringSearch.IO;

namespace UnitTests.StringSearch.Collections
{
    [TestFixture]
    public class FourBitDigitBigArrayTests
    {
        [Test]
        public void TestConstructor()
        {
            const string STR = "1234";

            Stream memStream = convertStringTo4BitDigitStream(STR);

            FourBitDigitBigArray a = new FourBitDigitBigArray(memStream);
        }

        [Test]
        public void TestEmpty()
        {
            Stream memStream = convertStringTo4BitDigitStream("");

            FourBitDigitBigArray a = new FourBitDigitBigArray(memStream);

            Assert.AreEqual(0, a.Length);
        }

        [Test]
        public void TestOddNumberOfDigits()
        {
            Stream memStream = convertStringTo4BitDigitStream("123");

            FourBitDigitBigArray a = new FourBitDigitBigArray(memStream);

            Assert.AreEqual(3, a.Length);
        }

        [Test]
        public void TestGet()
        {
            const string STR = "391";

            FourBitDigitBigArray a = convertStringTo4BitDigitArray(STR);

            for(int i = 0; i < STR.Length; i++)
            {
                char c = STR[i];
                byte b = a[i];

                Assert.AreEqual(c.ToString(), b.ToString());
            }
        }

        [Test]
        public void TestSetEven()
        {
            const string ORIG = "391";

            FourBitDigitBigArray a = convertStringTo4BitDigitArray(ORIG);

            a[0] = 7;
            Assert.AreEqual(7, a[0]);

            for (int i = 1; i < ORIG.Length; i++)
            {
                Assert.AreEqual(ORIG[i].ToString(), a[i].ToString());
            }
        }

        [Test]
        public void TestSetOdd()
        {
            const string ORIG = "391";

            FourBitDigitBigArray a = convertStringTo4BitDigitArray(ORIG);

            a[1] = 7;
            Assert.AreEqual(7, a[1]);

            for (int i = 0; i < ORIG.Length; i++)
            {
                if(i != 1)
                {
                    Assert.AreEqual(ORIG[i].ToString(), a[i].ToString());
                }
            }
        }

        [Test]
        public void TestAccessOutOfRangeNeg()
        {
            FourBitDigitBigArray a = convertStringTo4BitDigitArray("123");

            Assert.Throws<IndexOutOfRangeException>(() =>
            {
                byte b = a[-1];
            });
        }

        [Test]
        public void TestAccessOutOfRange()
        {
            FourBitDigitBigArray a = convertStringTo4BitDigitArray("123");

            Assert.Throws<IndexOutOfRangeException>(() =>
            {
                byte b = a[3];
            });
        }

        [Test]
        public void TestSetOutOfRangeNeg()
        {
            FourBitDigitBigArray a = convertStringTo4BitDigitArray("123");

            Assert.Throws<IndexOutOfRangeException>(() =>
            {
                a[-1] = 3;
            });
        }

        [Test]
        public void TestSetOutOfRange()
        {
            FourBitDigitBigArray a = convertStringTo4BitDigitArray("123");

            Assert.Throws<IndexOutOfRangeException>(() =>
            {
                a[3] = 3;
            });
        }

        [Test]
        public void TestSetOverflow()
        {
            FourBitDigitBigArray a = convertStringTo4BitDigitArray("123");

            Assert.Throws<OverflowException>(() =>
            {
                a[0] = 16;
            });
        }

        [Test]
        public void TestSetReservedOverflow()
        {
            //Highest possible value in 4 bits (15) reserved for marking that half of the byte as not in use
            //  so it counts as overflow
            FourBitDigitBigArray a = convertStringTo4BitDigitArray("123");

            Assert.Throws<OverflowException>(() =>
            {
                a[0] = 15;
            });
        }

        [Test]
        public void TestLength()
        {
            FourBitDigitBigArray a = convertStringTo4BitDigitArray("123");
            Assert.AreEqual(3, a.Length);
        }

        [Test]
        public void TestLengthEven()
        {
            FourBitDigitBigArray a = convertStringTo4BitDigitArray("1234");
            Assert.AreEqual(4, a.Length);
        }

        [Test]
        public void TestLengthEmpty()
        {
            const long LENGTH = 3;
            FourBitDigitBigArray a = makeNew(LENGTH);
            Assert.AreEqual(LENGTH, a.Length);
        }

        [Test]
        public void TestLengthEmptyEven()
        {
            const long LENGTH = 4;
            FourBitDigitBigArray a = makeNew(LENGTH);
            Assert.AreEqual(LENGTH, a.Length);
        }

        [Test]
        public void TestLengthBig()
        {
            const long LENGTH = 3000000001;
            FourBitDigitBigArray a = makeNew(LENGTH);
            Assert.AreEqual(LENGTH, a.Length);
        }

        [Test]
        public void TestLengthBigEven()
        {
            const long LENGTH = 3000000000;
            FourBitDigitBigArray a = makeNew(LENGTH);
            Assert.AreEqual(LENGTH, a.Length);
        }

        [Test]
        public void TestConstructorBigMemoryStreamAsUnderlyingStream()
        {
            BigMemoryStream stream = new BigMemoryStream(5);
            FourBitDigitBigArray a = new FourBitDigitBigArray(stream);
        }

        [Test]
        public void TestConstructorBigMemoryStreamAsUnderlyingStreamBig()
        {
            BigMemoryStream stream = new BigMemoryStream(3000000000); //3bil bytes ~= 3GB
            FourBitDigitBigArray a = new FourBitDigitBigArray(stream);
        }

        [Test]
        public void TestGetSetBig()
        {
            FourBitDigitBigArray a = makeNew(3000000000);
            a[2500000000] = 5;
            Assert.AreEqual(5, a[2500000000]);
        }

        #region Helpers
        public static FourBitDigitBigArray convertStringTo4BitDigitArray(string str)
        {
            Stream memStream = convertStringTo4BitDigitStream(str);

            return new FourBitDigitBigArray(memStream);
        }

        private static FourBitDigitBigArray makeNew(long length)
        {
            //If length is odd, add one to it
            bool odd = false;
            if(length % 2 == 1)
            {
                odd = true;
                length++;
            }

            Stream stream;
            long streamLength = length / 2;
            if(length > int.MaxValue)
            {
                stream = new BigMemoryStream(streamLength);
            }
            else
            {
                stream = new MemoryStream((int)streamLength);
            }
            stream.SetLength(streamLength);

            //If the length was odd, set the last byte to 15 (last 4 bits are all 1's)
            if(odd)
            {
                stream.Position = streamLength - 1;
                stream.WriteByte(15);
            }

            FourBitDigitBigArray a = new FourBitDigitBigArray(stream);
            return a;
        }

        public static Stream convertStringTo4BitDigitStream(string str)
        {
            // Write string to stream
            using (MemoryStream uncompressedStream = new MemoryStream())
            {
                StreamWriter writer = new StreamWriter(uncompressedStream);
                writer.Write(str);
                
                // Just flush the writer here to ensure all the data is written to the stream.
                //  It cannot be closed/disposed as that will also close the underlying stream
                writer.Flush();

                // Reset the stream position back to the beginning, as the compressor will run from
                //  the current position
                uncompressedStream.Position = 0;

                // Stream to hold compressed 4 bit digit output
                MemoryStream outStream = new MemoryStream();

                Compression.CompressStream4BitDigit(uncompressedStream, outStream);
                return outStream;
            }
        }
        #endregion
    }
}
