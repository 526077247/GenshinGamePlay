using System;
using System.IO;
using Nino.Shared.IO;
using Nino.Shared.Mgr;
using System.Collections;
using System.Runtime.CompilerServices;

namespace Nino.Serialization
{
    /// <summary>
    /// A writer that writes serialization Data
    /// </summary>
    public partial class Writer
    {
        /// <summary>
        /// block size when creating buffer
        /// </summary>
        private const ushort BufferBlockSize = ushort.MaxValue;

        /// <summary>
        /// Buffer that stores data
        /// </summary>
        private ExtensibleBuffer<byte> buffer;

        /// <summary>
        /// compress option
        /// </summary>
        private CompressOption option;

        /// <summary>
        /// Position of the current buffer
        /// </summary>
        private int position;

        /// <summary>
        /// Convert writer to byte
        /// </summary>
        /// <returns></returns>
        public byte[] ToBytes()
        {
            switch (option)
            {
                case CompressOption.Zlib:
                    return CompressMgr.Compress(buffer, position);
                case CompressOption.Lz4:
                    throw new NotSupportedException("not support lz4 yet");
                case CompressOption.NoCompression:
                    return buffer.ToArray(0, position);
            }

            return ConstMgr.Null;
        }

        /// <summary>
        /// Create a writer (needs to set up values)
        /// </summary>
        public Writer()
        {
        }

        /// <summary>
        /// Create a nino writer
        /// </summary>
        /// <param name="option"></param>
        public Writer(CompressOption option = CompressOption.Zlib)
        {
            Init(option);
        }

        /// <summary>
        /// Init writer
        /// </summary>
        /// <param name="compressOption"></param>
        public void Init(CompressOption compressOption)
        {
            if (buffer == null)
            {
                var peak = ObjectPool<ExtensibleBuffer<byte>>.Peak();
                if (peak != null && peak.ExpandSize == BufferBlockSize)
                {
                    buffer = ObjectPool<ExtensibleBuffer<byte>>.Request();
                }
                else
                {
                    buffer = new ExtensibleBuffer<byte>(BufferBlockSize);
                }
            }

            position = 0;
            option = compressOption;
        }

        /// <summary>
        /// Write primitive values, DO NOT USE THIS FOR CUSTOM IMPORTER
        /// </summary>
        /// <param name="type"></param>
        /// <param name="val"></param>
        /// <exception cref="InvalidDataException"></exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteCommonVal(Type type, object val) =>
            Serializer.Serialize(type, val, this, option, false);

        /// <summary>
        /// Write byte[]
        /// </summary>
        /// <param name="data"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void Write(byte[] data)
        {
            var len = data.Length;
            CompressAndWrite(len);
            fixed (byte* ptr = data)
            {
                Write(ptr, ref len);
            }
        }

        /// <summary>
        /// Write byte[]
        /// </summary>
        /// <param name="data"></param>
        /// <param name="len"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal unsafe void Write(byte* data, ref int len)
        {
            if (len <= 8)
            {
                while (len-- > 0)
                {
                    buffer[position++] = *data++;
                }

                return;
            }

            buffer.CopyFrom(data, 0, position, len);
            position += len;
        }

        /// <summary>
        /// Write a double
        /// </summary>
        /// <param name="value"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(double value)
        {
            Write(ref value, ConstMgr.SizeOfULong);
        }

        /// <summary>
        /// Write a float
        /// </summary>
        /// <param name="value"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(float value)
        {
            Write(ref value, ConstMgr.SizeOfUInt);
        }

        /// <summary>
        /// Write a DateTime
        /// </summary>
        /// <param name="value"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(DateTime value)
        {
            Write(value.ToOADate());
        }

        /// <summary>
        /// Write decimal
        /// </summary>
        /// <param name="d"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(decimal d)
        {
            Write(ref d, ConstMgr.SizeOfDecimal);
        }

        /// <summary>
        /// Writes a boolean to this stream. A single byte is written to the stream
        /// with the value 0 representing false or the value 1 representing true.
        /// </summary>
        /// <param name="value"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(bool value)
        {
            buffer[position++] = Unsafe.As<bool, byte>(ref value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(char value)
        {
            Write(ref value, ConstMgr.SizeOfUShort);
        }

        /// <summary>
        /// Write string
        /// </summary>
        /// <param name="val"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void Write(string val)
        {
            if (val is null)
            {
                Write(false);
                return;
            }

            Write(true);

            if (val == string.Empty)
            {
                Write((byte)CompressType.Byte);
                Write((byte)0);
                return;
            }

            var strSpan = val.AsSpan(); // 2*len, utf16 str
            int len = strSpan.Length * ConstMgr.SizeOfUShort;
            fixed (char* first = &strSpan.GetPinnableReference())
            {
                CompressAndWrite(len);
                Write((byte*)first, ref len);
            }
        }

        #region write whole num

        /// <summary>
        /// Write byte val to binary writer
        /// </summary>
        /// <param name="num"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(byte num)
        {
            buffer[position++] = num;
        }

        /// <summary>
        /// Write byte val to binary writer
        /// </summary>
        /// <param name="num"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(sbyte num)
        {
            buffer[position++] = Unsafe.As<sbyte, byte>(ref num);
        }

        /// <summary>
        /// Write int val to binary writer
        /// </summary>
        /// <param name="num"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(int num)
        {
            Write(ref num, ConstMgr.SizeOfInt);
        }

        /// <summary>
        /// Write uint val to binary writer
        /// </summary>
        /// <param name="num"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(uint num)
        {
            Write(ref num, ConstMgr.SizeOfUInt);
        }

        /// <summary>
        /// Write short val to binary writer
        /// </summary>
        /// <param name="num"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(short num)
        {
            Write(ref num, ConstMgr.SizeOfShort);
        }

        /// <summary>
        /// Write ushort val to binary writer
        /// </summary>
        /// <param name="num"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(ushort num)
        {
            Write(ref num, ConstMgr.SizeOfUShort);
        }

        /// <summary>
        /// Write long val to binary writer
        /// </summary>
        /// <param name="num"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(long num)
        {
            Write(ref num, ConstMgr.SizeOfLong);
        }

        /// <summary>
        /// Write ulong val to binary writer
        /// </summary>
        /// <param name="num"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(ulong num)
        {
            Write(ref num, ConstMgr.SizeOfULong);
        }

        #endregion

        #region write whole number without sign

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CompressAndWrite(ref ulong num)
        {
            if (num <= uint.MaxValue)
            {
                if (num <= ushort.MaxValue)
                {
                    if (num <= byte.MaxValue)
                    {
                        buffer[position++] = (byte)CompressType.Byte;
                        Write(ref num, 1);
                        return;
                    }

                    buffer[position++] = (byte)CompressType.UInt16;
                    Write(ref num, ConstMgr.SizeOfUShort);
                    return;
                }

                buffer[position++] = (byte)CompressType.UInt32;
                Write(ref num, ConstMgr.SizeOfUInt);
                return;
            }

            buffer[position++] = (byte)CompressType.UInt64;
            Write(ref num, ConstMgr.SizeOfULong);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CompressAndWrite(ref uint num)
        {
            if (num <= ushort.MaxValue)
            {
                if (num <= byte.MaxValue)
                {
                    buffer[position++] = (byte)CompressType.Byte;
                    Write(ref num, 1);
                    return;
                }

                buffer[position++] = (byte)CompressType.UInt16;
                Write(ref num, ConstMgr.SizeOfUShort);
                return;
            }

            buffer[position++] = (byte)CompressType.UInt32;
            Write(ref num, ConstMgr.SizeOfUInt);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CompressAndWrite(ulong num)
        {
            ref var n = ref num;
            CompressAndWrite(ref n);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CompressAndWrite(uint num)
        {
            ref var n = ref num;
            CompressAndWrite(ref n);
        }

        #endregion

        #region write whole number with sign

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CompressAndWrite(ref long num)
        {
            if (num < 0)
            {
                if (num >= int.MinValue)
                {
                    if (num >= short.MinValue)
                    {
                        if (num >= sbyte.MinValue)
                        {
                            buffer[position++] = (byte)CompressType.SByte;
                            Write(ref num, 1);
                            return;
                        }

                        buffer[position++] = (byte)CompressType.Int16;
                        Write(ref num, ConstMgr.SizeOfShort);
                        return;
                    }

                    buffer[position++] = (byte)CompressType.Int32;
                    Write(ref num, ConstMgr.SizeOfInt);
                    return;
                }

                buffer[position++] = (byte)CompressType.Int64;
                Write(ref num, ConstMgr.SizeOfLong);
                return;
            }

            if (num <= int.MaxValue)
            {
                if (num <= short.MaxValue)
                {
                    if (num <= byte.MaxValue)
                    {
                        buffer[position++] = (byte)CompressType.Byte;
                        Write(ref num, 1);
                        return;
                    }

                    buffer[position++] = (byte)CompressType.UInt16;
                    Write(ref num, ConstMgr.SizeOfShort);
                    return;
                }

                buffer[position++] = (byte)CompressType.UInt32;
                Write(ref num, ConstMgr.SizeOfInt);
                return;
            }

            buffer[position++] = (byte)CompressType.UInt64;
            Write(ref num, ConstMgr.SizeOfLong);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CompressAndWrite(ref int num)
        {
            if (num < 0)
            {
                if (num >= short.MinValue)
                {
                    if (num >= sbyte.MinValue)
                    {
                        buffer[position++] = (byte)CompressType.SByte;
                        Write(ref num, 1);
                        return;
                    }

                    buffer[position++] = (byte)CompressType.Int16;
                    Write(ref num, ConstMgr.SizeOfShort);
                    return;
                }

                buffer[position++] = (byte)CompressType.Int32;
                Write(ref num, ConstMgr.SizeOfInt);
                return;
            }

            if (num <= short.MaxValue)
            {
                if (num <= byte.MaxValue)
                {
                    buffer[position++] = (byte)CompressType.Byte;
                    Write(ref num, 1);
                    return;
                }

                buffer[position++] = (byte)CompressType.UInt16;
                Write(ref num, ConstMgr.SizeOfShort);
                return;
            }

            buffer[position++] = (byte)CompressType.UInt32;
            Write(ref num, ConstMgr.SizeOfInt);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CompressAndWrite(long num)
        {
            ref var n = ref num;
            CompressAndWrite(ref n);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CompressAndWrite(int num)
        {
            ref var n = ref num;
            CompressAndWrite(ref n);
        }

        #endregion

        /// <summary>
        /// Compress and write enum (no boxing)
        /// </summary>
        /// <param name="type"></param>
        /// <param name="val"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Obsolete("Please re-generate nino serialize code to use the latest api")]
        public void CompressAndWriteEnum(Type type, ulong val)
        {
            switch (TypeModel.GetTypeCode(type))
            {
                case TypeCode.Byte:
                    Write((byte)val);
                    return;
                case TypeCode.SByte:
                    Write((sbyte)val);
                    return;
                case TypeCode.Int16:
                    Write((short)val);
                    return;
                case TypeCode.UInt16:
                    Write((ushort)val);
                    return;
                case TypeCode.Int32:
                    CompressAndWrite((int)val);
                    return;
                case TypeCode.UInt32:
                    CompressAndWrite((uint)val);
                    return;
                case TypeCode.Int64:
                    CompressAndWrite((long)val);
                    return;
                case TypeCode.UInt64:
                    CompressAndWrite(val);
                    return;
            }
        }

        /// <summary>
        /// Write array
        /// </summary>
        /// <param name="arr"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(Array arr)
        {
            //null
            if (arr == null)
            {
                Write(false);
                return;
            }

            Write(true);
            //empty
            if (arr.Length == 0)
            {
                //write len
                CompressAndWrite(0);
                return;
            }

            //write len
            int len = arr.Length;
            CompressAndWrite(ref len);
            //write item
            int i = 0;
            while (i < len)
            {
                var obj = arr.GetValue(i++);
#if ILRuntime
                var eType = obj is ILRuntime.Runtime.Intepreter.ILTypeInstance ilIns
                    ? ilIns.Type.ReflectionType
                    : obj.GetType();
#else
                var eType = obj.GetType();
#endif
                Write(eType.GetHashCode());
                WriteCommonVal(eType, obj);
            }
        }

        /// <summary>
        /// Write list
        /// </summary>
        /// <param name="arr"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(IList arr)
        {
            //null
            if (arr == null)
            {
                Write(false);
                return;
            }

            Write(true);
            //empty
            if (arr.Count == 0)
            {
                //write len
                CompressAndWrite(0);
                return;
            }

            //write len
            CompressAndWrite(arr.Count);
            //write item
            foreach (var c in arr)
            {
#if ILRuntime
                var eType = c is ILRuntime.Runtime.Intepreter.ILTypeInstance ilIns
                    ? ilIns.Type.ReflectionType
                    : c.GetType();
#else
                var eType = c.GetType();
#endif
                Write(eType.GetHashCode());
                WriteCommonVal(eType, c);
            }
        }

        /// <summary>
        /// Write dictionary
        /// </summary>
        /// <param name="dictionary"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(IDictionary dictionary)
        {
            //null
            if (dictionary == null)
            {
                Write(false);
                return;
            }

            Write(true);
            //empty
            if (dictionary.Count == 0)
            {
                //write len
                CompressAndWrite(0);
                return;
            }

            //write len
            int len = dictionary.Count;
            CompressAndWrite(ref len);
            //record keys
            var keys = dictionary.Keys;
            //write items
            foreach (var c in keys)
            {
                //write key
#if ILRuntime
                var eType = c is ILRuntime.Runtime.Intepreter.ILTypeInstance ilIns
                    ? ilIns.Type.ReflectionType
                    : c.GetType();
#else
                var eType = c.GetType();
#endif
                Write(eType.GetHashCode());
                WriteCommonVal(eType, c);
                //write val
                var val = dictionary[c];
#if ILRuntime
                eType = val is ILRuntime.Runtime.Intepreter.ILTypeInstance ilIns2
                    ? ilIns2.Type.ReflectionType
                    : val.GetType();
#else
                eType = val.GetType();
#endif
                Write(eType.GetHashCode());
                WriteCommonVal(eType, val);
            }
        }
    }
}