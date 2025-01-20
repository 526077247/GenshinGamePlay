/*
MIT License
   
   Copyright (c) 2018 Jonathan Linsner
   
   Permission is hereby granted, free of charge, to any person obtaining a copy
   of this software and associated documentation files (the "Software"), to deal
   in the Software without restriction, including without limitation the rights
   to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
   copies of the Software, and to permit persons to whom the Software is
   furnished to do so, subject to the following conditions:
   
   The above copyright notice and this permission notice shall be included in all
   copies or substantial portions of the Software.
   
   THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
   IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
   FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
   AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
   LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
   OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
   SOFTWARE.
 */

using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Newtonsoft.Json
{
    /// <summary>
    /// JsonConverter to convert Unity math types to and from JSON, for the Newtonsoft JSON library
    /// This library may be aquired at - https://www.newtonsoft.com/json
    /// This Converter requires '.NET 4.x' API compatibility to be enabled in the Unity player settings
    ///
    /// All supported types are written to a JSON array of numbers
    /// Vector values are written in XYZW order
    /// Quaterion values are written in XYZW order by default
    /// Matrix values are written in the order the [] accessor reads values
    /// Ray values are written as ray.origin.xyz ray.direction.xyz
    /// Plane values are written as plane.normal.xyz plane.distance
    /// </summary>
    public class UnityJsonConverter : JsonConverter
    {
        /// <summary>
        /// By default, this converter reads and writes quaternions in the XYZW component order,
        /// If this value is set to true, it will instead read and write in the WXYZ order
        /// </summary>
        public bool QuaternionWComponentFirst = false;

        private readonly Dictionary<Type, Action<JsonWriter, object>> _writerDispatchDictionary;
        private readonly Dictionary<Type, Func<JArray, object>> _readerDispatchDictionary;

        public UnityJsonConverter()
        {
            _writerDispatchDictionary = GenerateJsonWriters();
            _readerDispatchDictionary = GenerateJsonReaders();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            //Read serialze action from dispatch dictionary, and execute if found
            if (_writerDispatchDictionary.TryGetValue(value.GetType(), out var serializeAction))
            {
                serializeAction(writer, value);
            }
            else
            {
                //The check in CanConvert(Type) should prevent this case from ever happening
                throw new JsonReaderException($"Invalid type for ValueConverter:{value.GetType()}");
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            //Read deserialze func from dispatch dictionary, and execute if found
            if (_readerDispatchDictionary.TryGetValue(objectType, out var deserializeFunc))
            {
                //Convert token from reader to array for deserialize func
                var token = JToken.ReadFrom(reader);
                if (!(token is JArray asArray))
                {
                    throw new JsonReaderException($"Could not read {objectType} from json, expected a json array but got {token.Type}");
                }
                
                return deserializeFunc(asArray);
            }
            else
            {
                //The check in CanConvert(Type) should prevent this case from ever happening
                throw new JsonReaderException($"Invalid type for ValueConverter:{objectType}");
            }
        }

        /// <summary>
        /// Generate the methods used to write a type to json. They are stored in a Dictionary for type based lookup
        /// </summary>
        private Dictionary<Type, Action<JsonWriter, object>> GenerateJsonWriters()
        {
            var writerDispatchDictionary = new Dictionary<Type, Action<JsonWriter, object>>();

            writerDispatchDictionary[typeof(Vector2)] = (writer, o) =>
            {
                if (!(o is Vector2 asVector2))
                {
                    //This should not occure durning normal program flow, as the type of o is used to look up this method
                    throw new JsonReaderException();
                }

                //This is measurably faster (/Approximitly 20%) than allocated a JArray for each value we write              
                writer.WriteStartArray();
                writer.WriteValue(asVector2.x);
                writer.WriteValue(asVector2.y);
                writer.WriteEndArray();

                //var result = new JArray { asVector2.x, asVector2.y };
                //result.WriteTo(writer);
            };

            writerDispatchDictionary[typeof(Vector3)] = (writer, o) =>
            {
                if (!(o is Vector3 asVector3))
                {
                    throw new JsonReaderException();
                }
                writer.WriteStartArray();
                writer.WriteValue(asVector3.x);
                writer.WriteValue(asVector3.y);
                writer.WriteValue(asVector3.z);
                writer.WriteEndArray();
            };

            writerDispatchDictionary[typeof(Vector4)] = (writer, o) =>
            {
                if (!(o is Vector4 asVector4))
                {
                    throw new JsonReaderException();
                }

                writer.WriteStartArray();
                writer.WriteValue(asVector4.x);
                writer.WriteValue(asVector4.y);
                writer.WriteValue(asVector4.z);
                writer.WriteValue(asVector4.w);
                writer.WriteEndArray();
            };

            writerDispatchDictionary[typeof(Quaternion)] = (writer, o) =>
            {
                if (!(o is Quaternion asQuaternion))
                {
                    throw new JsonReaderException();
                }

                writer.WriteStartArray();

                if (QuaternionWComponentFirst)
                {
                    writer.WriteValue(asQuaternion.w);
                    writer.WriteValue(asQuaternion.x);
                    writer.WriteValue(asQuaternion.y);
                    writer.WriteValue(asQuaternion.z);
                }
                else
                {
                    writer.WriteValue(asQuaternion.x);
                    writer.WriteValue(asQuaternion.y);
                    writer.WriteValue(asQuaternion.z);
                    writer.WriteValue(asQuaternion.w);
                }
                writer.WriteEndArray();
            };

            writerDispatchDictionary[typeof(Matrix4x4)] = (writer, o) =>
            {
                if (!(o is Matrix4x4 asMatrix))
                {
                    throw new JsonReaderException();
                }

                writer.WriteStartArray();

                for (int i = 0; i < 16; i++)
                {
                    writer.WriteValue(asMatrix[i]);
                }

                writer.WriteEndArray();
            };

            writerDispatchDictionary[typeof(Ray)] = (writer, o) =>
            {
                if (!(o is Ray asRay))
                {
                    throw new JsonReaderException();
                }

                writer.WriteStartArray();

                writer.WriteValue(asRay.origin.x);
                writer.WriteValue(asRay.origin.y);
                writer.WriteValue(asRay.origin.z);

                writer.WriteValue(asRay.direction.x);
                writer.WriteValue(asRay.direction.y);
                writer.WriteValue(asRay.direction.z);

                writer.WriteEndArray();
            };

            writerDispatchDictionary[typeof(Plane)] = (writer, o) =>
            {
                if (!(o is Plane asPlane))
                {
                    throw new JsonReaderException();
                }

                writer.WriteStartArray();

                writer.WriteValue(asPlane.normal.x);
                writer.WriteValue(asPlane.normal.y);
                writer.WriteValue(asPlane.normal.z);

                writer.WriteValue(asPlane.distance);

                writer.WriteEndArray();
            };

            return writerDispatchDictionary;
        }

        //TODO: Rewrite the reader methods to directly use the JsonReader class, instead of reading the whole token and allocating a JArray
        /// <summary>
        /// Generate the methods used to read a type from json. They are stored in a Dictionary for type based lookup
        /// </summary>
        private Dictionary<Type, Func<JArray, object>> GenerateJsonReaders()
        {
            var readerDispatchMap = new Dictionary<Type, Func<JArray, object>>();

            readerDispatchMap[typeof(Vector2)] = (jArray) =>
            {
                if (jArray.Count < 2)
                {
                    throw new JsonReaderException($"Could not read {typeof(Vector2)} from json, expected a json array with 2 elements");
                }
                return new Vector2(
                    jArray[0].ToObject<float>(),
                    jArray[1].ToObject<float>());
            };

            readerDispatchMap[typeof(Vector3)] = (jArray) =>
            {
                if (jArray.Count < 3)
                {
                    throw new JsonReaderException($"Could not read {typeof(Vector3)} from json, expected a json array with 3 elements");
                }
                return new Vector3(
                    jArray[0].ToObject<float>(),
                    jArray[1].ToObject<float>(),
                    jArray[2].ToObject<float>());
            };

            readerDispatchMap[typeof(Vector4)] = (jArray) =>
            {
                if (jArray.Count < 4)
                {
                    throw new JsonReaderException($"Could not read {typeof(Vector4)} from json, expected a json array with 4 elements");
                }
                return new Vector4(
                    jArray[0].ToObject<float>(),
                    jArray[1].ToObject<float>(),
                    jArray[2].ToObject<float>(),
                    jArray[3].ToObject<float>());
            };

            readerDispatchMap[typeof(Quaternion)] = (jArray) =>
            {
                if (jArray.Count < 4)
                {
                    throw new JsonReaderException($"Could not read {typeof(Quaternion)} from json, expected a json array with 4 elements");
                }

                return QuaternionWComponentFirst
                    ? new Quaternion(
                        jArray[1].ToObject<float>(),
                        jArray[2].ToObject<float>(),
                        jArray[3].ToObject<float>(),
                        jArray[0].ToObject<float>())
                    : new Quaternion(
                        jArray[0].ToObject<float>(),
                        jArray[1].ToObject<float>(),
                        jArray[2].ToObject<float>(),
                        jArray[3].ToObject<float>());
            };

            readerDispatchMap[typeof(Matrix4x4)] = (jArray) =>
            {
                if (jArray.Count < 16)
                {
                    throw new JsonReaderException($"Could not read {typeof(Matrix4x4)} from json, expected a json array with 16 elements");
                }

                var result = new Matrix4x4();
                for (int i = 0; i < 16; i++)
                {
                    result[i] = jArray[i].ToObject<float>();
                }

                return result;
            };

            readerDispatchMap[typeof(Ray)] = (jArray) =>
            {
                if (jArray.Count < 6)
                {
                    throw new JsonReaderException($"Could not read {typeof(Ray)} from json, expected a json array with 6 elements");
                }

                var ray = new Ray();

                ray.origin = new Vector3(
                    jArray[0].ToObject<float>(),
                    jArray[1].ToObject<float>(),
                    jArray[2].ToObject<float>());

                ray.direction = new Vector3(
                    jArray[3].ToObject<float>(),
                    jArray[4].ToObject<float>(),
                    jArray[5].ToObject<float>());

                return ray;
            };

            readerDispatchMap[typeof(Plane)] = (jArray) =>
            {
                if (jArray.Count < 4)
                {
                    throw new JsonReaderException($"Could not read {typeof(Plane)} from json, expected a json array with 4 elements");
                }

                var plane = new Plane();

                plane.normal = new Vector3(
                    jArray[0].ToObject<float>(),
                    jArray[1].ToObject<float>(),
                    jArray[2].ToObject<float>());

                plane.distance = jArray[3].ToObject<float>();

                return plane;
            };

            return readerDispatchMap;
        }

        public override bool CanConvert(Type objectType)
        {
            //If the dictionary contains the key, the type can be handled by this class
            return _writerDispatchDictionary.ContainsKey(objectType);
        }

        //Enable this JSON Converter for reading as well as writing values
        public override bool CanRead => true;
        public override bool CanWrite => true;

    }
}