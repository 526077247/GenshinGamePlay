using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;

namespace Nebukam.JobAssist
{
    public static class Extensions
    {

        /// <summary>
        /// Call Complete() on a JobHandle only if the job IsCompleted = true.
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static bool TryComplete(this JobHandle @this)
        {
            if (@this.IsCompleted) { @this.Complete(); return true; }
            return false;
        }

        /// <summary>
        /// Extremely inneficient "remove at" method for NativeList<T>
        /// Usefull for debug & making sure algorithms are working as intented
        /// Shouldn't be used in production.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static T RemoveAt<T>(this ref NativeList<T> @this, int index)
            where T : unmanaged
        {
            int length = @this.Length;
            T val = @this[index];

            for (int i = index; i < length - 1; i++)
                @this[i] = @this[i + 1];

            @this.ResizeUninitialized(length - 1);
            return val;

        }

        /// <summary>
        /// Checks whether a NativeMultiHashMap as a given value associated to a given key
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="this"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool Contains<TKey, TValue>(this ref NativeParallelMultiHashMap<TKey, TValue> @this, ref TKey key, ref TValue value)
            where TKey : unmanaged, IEquatable<TKey>
            where TValue : unmanaged, IEquatable<TValue>
        {
            NativeParallelMultiHashMapIterator<TKey> it;
            TValue result;
            if (@this.TryGetFirstValue(key, out result, out it))
            {
                if (result.Equals(value)) { return true; }
                while (@this.TryGetNextValue(out result, ref it))
                {
                    if (result.Equals(value)) { return true; }
                }
            }
            return false;
        }

        /// <summary>
        /// Removes a single value from the list associated to the a key
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="this"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool Remove<TKey, TValue>(this ref NativeParallelMultiHashMap<TKey, TValue> @this, ref TKey key, ref TValue value)
            where TKey : unmanaged, IEquatable<TKey>
            where TValue : unmanaged, IEquatable<TValue>
        {
            if (!@this.Contains(ref key, ref value)) { return false; }

            NativeList<TValue> values = new NativeList<TValue>(5, Allocator.Temp);
            NativeParallelMultiHashMapIterator<TKey> it;
            TValue result;
            if (@this.TryGetFirstValue(key, out result, out it))
            {
                if (result.Equals(value)) { } else { values.Add(result); }
                while (@this.TryGetNextValue(out result, ref it))
                {
                    if (result.Equals(value)) { } else { values.Add(result); }
                }
            }

            @this.Remove(key);
            for (int i = 0, count = values.Length; i < count; i++) { @this.Add(key, values[i]); }
            values.Dispose();

            return true;
        }

        public static bool Contains<TValue>(this ref NativeList<TValue> @this, ref TValue value)
            where TValue : unmanaged, IEquatable<TValue>
        {
            for (int i = 0, count = @this.Length; i < count; i++)
                if (@this[i].Equals(value)) { return true; }
            return false;
        }

        public static bool AddOnce<TValue>(this ref NativeList<TValue> @this, ref TValue value)
            where TValue : unmanaged, IEquatable<TValue>
        {
            if (@this.Contains(ref value)) { return false; }
            @this.Add(value);
            return true;
        }

        public static TValue Pop<TValue>(this ref NativeList<TValue> @this)
            where TValue : unmanaged
        {
            int index = @this.Length - 1;
            TValue result = @this[index];
            @this.ResizeUninitialized(index);
            return result;
        }

        public static TValue Shift<TValue>(this ref NativeList<TValue> @this)
            where TValue : unmanaged
        {
            TValue result = @this[0];
            @this.RemoveAt(0);
            return result;
        }

        /// <summary>
        /// Return a list containing all values associated to a given key.
        /// If no value is found, returns an empty list.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="this"></param>
        /// <param name="key"></param>
        /// <param name="alloc"></param>
        /// <param name="capacity"></param>
        /// <returns></returns>
        public static NativeList<TValue> GetValues<TKey, TValue>(this ref NativeParallelMultiHashMap<TKey, TValue> @this, ref TKey key, Allocator alloc, int capacity = 5)
            where TKey : unmanaged, IEquatable<TKey>
            where TValue : unmanaged, IEquatable<TValue>
        {

            NativeList<TValue> list = new NativeList<TValue>(capacity, alloc);

            NativeList<TValue> values = new NativeList<TValue>(5, Allocator.Temp);
            NativeParallelMultiHashMapIterator<TKey> it;

            TValue result;
            if (@this.TryGetFirstValue(key, out result, out it))
            {
                list.Add(result);
                while (@this.TryGetNextValue(out result, ref it))
                {
                    list.Add(result);
                }
            }

            return list;
        }

        /// <summary>
        /// Push value associated with a given key to a given list.
        /// Return the number of values added.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="this"></param>
        /// <param name="key"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        public static int PushValues<TKey, TValue>(this ref NativeParallelMultiHashMap<TKey, TValue> @this, ref TKey key, ref NativeList<TValue> list)
            where TKey : unmanaged, IEquatable<TKey>
            where TValue : unmanaged, IEquatable<TValue>
        {

            NativeList<TValue> values = new NativeList<TValue>(5, Allocator.Temp);
            NativeParallelMultiHashMapIterator<TKey> it;

            int resultCount = 0;
            TValue result;
            if (@this.TryGetFirstValue(key, out result, out it))
            {
                list.Add(result);
                resultCount++;
                while (@this.TryGetNextValue(out result, ref it))
                {
                    list.Add(result);
                    resultCount++;
                }
            }

            return resultCount;
        }

        /// <summary>
        /// Returns a clone of a NativeMultiHashMap
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="this"></param>
        /// <param name="alloc"></param>
        /// <returns></returns>
        public static NativeParallelMultiHashMap<TKey, TValue> Clone<TKey, TValue>(this ref NativeParallelMultiHashMap<TKey, TValue> @this, Allocator alloc)
            where TKey : unmanaged, IEquatable<TKey>
            where TValue : unmanaged
        {

            NativeParallelMultiHashMap<TKey, TValue> cloneHashMap = new NativeParallelMultiHashMap<TKey, TValue>(@this.Count(), alloc);

            NativeParallelMultiHashMapIterator<TKey> it;
            NativeArray<TKey> keys = @this.GetKeyArray(Allocator.Temp);
            TKey key;
            TValue value;

            for (int k = 0, count = keys.Length; k < count; k++)
            {
                key = keys[k];
                if (@this.TryGetFirstValue(key, out value, out it))
                {
                    cloneHashMap.Add(key, value);
                    while (@this.TryGetNextValue(out value, ref it))
                    {
                        cloneHashMap.Add(key, value);
                    }
                }
            }

            keys.Dispose();

            return cloneHashMap;
        }

        /// <summary>
        /// Ensure a NativeArray is of required size.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="nativeArray"></param>
        /// <param name="length"></param>
        /// <param name="alloc"></param>
        /// <returns>true if the size is unchanged, false if the NativeArray has been updated</returns>
        public static bool MakeLength<T>(ref NativeArray<T> nativeArray, int length, Allocator alloc = Allocator.Persistent)
            where T : unmanaged
        {
            if (!nativeArray.IsCreated
                || nativeArray.Length != length)
            {
                nativeArray.Release();
                nativeArray = new NativeArray<T>(length, alloc);
                return false;
            }

            return true;

        }

        /// <summary>
        /// Ensure a NativeArray is of required size.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="nativeHashMap"></param>
        /// <param name="length"></param>
        /// <param name="alloc"></param>
        /// <returns>true if the size is unchanged, false if the NativeArray has been updated</returns>
        public static bool MakeLength<TKey, TValue>(ref NativeParallelHashMap<TKey, TValue> nativeHashMap, int length, Allocator alloc = Allocator.Persistent)
            where TKey : unmanaged, IEquatable<TKey>
            where TValue : unmanaged
        {
            if (!nativeHashMap.IsCreated
                || nativeHashMap.Count() != length)
            {
                nativeHashMap.Release();
                nativeHashMap = new NativeParallelHashMap<TKey, TValue>(length, alloc);
                return false;
            }

            return true;

        }

        /// <summary>
        /// Ensure a NativeArray is of required size.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="nativeArray"></param>
        /// <param name="length"></param>
        /// <param name="alloc"></param>
        /// <returns>true if the size is unchanged, false if the NativeArray has been updated</returns>
        public static bool MakeLength<T>(ref T[] array, int length)
            where T : struct
        {
            if (array.Length != length)
            {
                array = new T[length];
                return false;
            }

            return true;

        }

        /// <summary>
        /// Ensure a NativeArray has at least a given size.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="nativeArray"></param>
        /// <param name="length"></param>
        /// <param name="padding"></param>
        /// <param name="alloc"></param>
        /// <returns>true if the size is unchanged, false if the NativeArray has been updated</returns>
        public static bool EnsureMinLength<T>(ref NativeArray<T> nativeArray, int length, int padding = 0, Allocator alloc = Allocator.Persistent)
            where T : unmanaged
        {
            if (!nativeArray.IsCreated
                || nativeArray.Length < length)
            {
                nativeArray.Release();
                nativeArray = new NativeArray<T>(length + padding, alloc);
                return false;
            }

            return true;

        }

        /// <summary>
        /// Copies the content of a managed array into a nativeArray
        /// Ensure the target native array has the same length as the source.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="src"></param>
        /// <param name="dest"></param>
        /// <returns>true if the size is unchanged, false if the NativeArray has been updated</returns>
        public static bool Copy<T>(T[] src, ref NativeArray<T> dest, Allocator alloc = Allocator.Persistent)
            where T : unmanaged
        {
            int count = src.Length;
            bool resized = MakeLength<T>(ref dest, src.Length, alloc);
            NativeArray<T>.Copy(src, dest);
            return resized;
        }

        /// <summary>
        /// Copies the content of a NativeArray into a managed array
        /// Ensure the target native array has the same length as the source.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="src"></param>
        /// <param name="dest"></param>
        /// <returns>true if the size is unchanged, false if the NativeArray has been updated</returns>
        public static bool Copy<T>(NativeArray<T> src, ref T[] dest)
            where T : unmanaged
        {
            int count = src.Length;
            bool resized = dest.Length != count;
            if (resized) { dest = new T[count]; }
            NativeArray<T>.Copy(src, dest);
            return resized;
        }

        /// <summary>
        /// Copies the content of a NativeArray into a managed array
        /// Ensure the target native array has the same length as the source.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="src"></param>
        /// <param name="dest"></param>
        /// <returns>true if the size is unchanged, false if the NativeArray has been updated</returns>
        public static bool Copy<T>(NativeArray<T> src, ref NativeArray<T> dest, Allocator alloc = Allocator.Persistent)
            where T : unmanaged
        {
            int count = src.Length;
            bool resized = !dest.IsCreated || dest.Length != count;
            if (resized)
            {
                dest.Release();
                dest = new NativeArray<T>(count, alloc);
            }
            NativeArray<T>.Copy(src, dest);
            return resized;
        }

        /// <summary>
        /// Copies the content of a managed list into a nativeArray
        /// Ensure the target native array has the same length as the source.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="src"></param>
        /// <param name="dest"></param>
        /// <returns>true if the size is unchanged, false if the NativeArray has been updated</returns>
        public static bool Copy<T>(List<T> src, ref NativeArray<T> dest, Allocator alloc = Allocator.Persistent)
            where T : unmanaged
        {
            int count = src.Count;
            bool resized = MakeLength<T>(ref dest, src.Count, alloc);

            for (int i = 0; i < count; i++)
                dest[i] = src[i];

            return resized;
        }

        /// <summary>
        /// Copies the content of a managed list into a nativeArray
        /// Ensure the target native array has the same length as the source.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="src"></param>
        /// <param name="dest"></param>
        public static void Copy<T>(List<T> src, ref NativeList<T> dest, Allocator alloc = Allocator.Persistent)
            where T : unmanaged
        {
            int count = src.Count;

            if (!dest.IsCreated)
                dest = new NativeList<T>(count + 1, alloc);
            else if (dest.Capacity <= count)
                dest.Capacity = count + 1;

            for (int i = 0; i < count; i++)
                dest.AddNoResize(src[i]);

        }

        public static void Release<T>(this NativeArray<T> @this) where T : unmanaged { if (@this.IsCreated) { @this.Dispose(); } }
        public static void Release<T>(this NativeList<T> @this) where T : unmanaged { if (@this.IsCreated) { @this.Dispose(); } }
        public static void Release<TKey, TValue>(this NativeParallelHashMap<TKey, TValue> @this) where TKey : unmanaged, IEquatable<TKey> where TValue : unmanaged { if (@this.IsCreated) { @this.Dispose(); } }
        public static void Release<TKey, TValue>(this NativeParallelMultiHashMap<TKey, TValue> @this) where TKey : unmanaged, IEquatable<TKey> where TValue : unmanaged { if (@this.IsCreated) { @this.Dispose(); } }

        public static void FloodArray<T>(NativeArray<T> array, T value)
            where T : unmanaged
        {
            new FloodArray<T> { array = array, value = value }.Run(array.Length);
        }

    }

    [BurstCompile]
    internal struct FloodArray<T> : Unity.Jobs.IJobParallelFor
        where T : unmanaged
    {
        public NativeArray<T> array;
        public T value;
        public void Execute(int index)
        {
            array[index] = value;
        }
    }
}
