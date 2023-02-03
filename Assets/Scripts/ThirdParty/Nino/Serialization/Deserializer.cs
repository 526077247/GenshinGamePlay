﻿using System;
using Nino.Shared.IO;
using Nino.Shared.Mgr;
using System.Reflection;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

// ReSharper disable UnusedMember.Local

namespace Nino.Serialization
{
	// ReSharper disable UnusedParameter.Local
	public static class Deserializer
	{
		/// <summary>
		/// Custom Exporter delegate that reads bytes to object
		/// </summary>
		internal delegate T ExporterDelegate<out T>(Reader reader);

		/// <summary>
		/// Add custom Exporter of all type T objects
		/// </summary>
		/// <param name="func"></param>
		/// <typeparam name="T"></typeparam>
		public static void AddCustomExporter<T>(Func<Reader, T> func)
		{
			var type = typeof(T);
			if (WrapperManifest.TryGetWrapper(type, out var wrapper))
			{
				((GenericWrapper<T>)wrapper).Exporter = func.Invoke;
				return;
			}

			GenericWrapper<T> genericWrapper = new GenericWrapper<T>
			{
				Exporter = func.Invoke
			};
			WrapperManifest.AddWrapper(typeof(T), genericWrapper);
		}

		/// <summary>
		/// Deserialize a NinoSerialize object
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="data"></param>
		/// <param name="option"></param>
		/// <returns></returns>
		public static T Deserialize<T>(byte[] data, CompressOption option = CompressOption.Zlib)
			=> Deserialize<T>(new Span<byte>(data), option);

		/// <summary>
		/// Deserialize a NinoSerialize object
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="data"></param>
		/// <param name="option"></param>
		/// <returns></returns>
		public static T Deserialize<T>(ArraySegment<byte> data, CompressOption option = CompressOption.Zlib)
			=> Deserialize<T>(new Span<byte>(data.Array, data.Offset, data.Count), option);


		/// <summary>
		/// Deserialize a NinoSerialize object
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="data"></param>
		/// <param name="option"></param>
		/// <returns></returns>
		public static T Deserialize<T>(Span<byte> data, CompressOption option = CompressOption.Zlib)
		{
			Type type = typeof(T);

			Reader reader = ObjectPool<Reader>.Request();
			reader.Init(data, data.Length,
				TypeModel.IsNonCompressibleType(type) ? CompressOption.NoCompression : option);

			//basic type
			if (WrapperManifest.TryGetWrapper(type, out var wrapper))
			{
				var ret = ((NinoWrapperBase<T>)wrapper).Deserialize(reader);
				ObjectPool<Reader>.Return(reader);
				return ret;
			}

			//code generated type
			if (TypeModel.TryGetWrapper(type, out wrapper))
			{
				//add wrapper
				WrapperManifest.AddWrapper(type, wrapper);
				//start Deserialize
				var ret = ((NinoWrapperBase<T>)wrapper).Deserialize(reader);
				ObjectPool<Reader>.Return(reader);
				return ret;
			}

			//has to be an object or custom type
			var result = Deserialize(type, null, data, reader, option, true, true, true);
			if (result == null)
			{
				ObjectPool<Reader>.Return(reader);
				return default;
			}
			return (T)result;
		}

		/// <summary>
		/// Deserialize a NinoSerialize object
		/// </summary>
		/// <param name="type"></param>
		/// <param name="data"></param>
		/// <param name="option"></param>
		/// <returns></returns>
		public static object Deserialize(Type type, byte[] data, CompressOption option = CompressOption.Zlib)
			=> Deserialize(type, new Span<byte>(data), option);

		/// <summary>
		/// Deserialize a NinoSerialize object
		/// </summary>
		/// <param name="type"></param>
		/// <param name="data"></param>
		/// <param name="option"></param>
		/// <returns></returns>
		public static object Deserialize(Type type, ArraySegment<byte> data,
			CompressOption option = CompressOption.Zlib)
			=> Deserialize(type, new Span<byte>(data.Array, data.Offset, data.Count), option);

		/// <summary>
		/// Deserialize a NinoSerialize object
		/// </summary>
		/// <param name="type"></param>
		/// <param name="data"></param>
		/// <param name="option"></param>
		/// <returns></returns>
		public static object Deserialize(Type type, Span<byte> data,
			CompressOption option = CompressOption.Zlib)
		{
			Reader reader = ObjectPool<Reader>.Request();
			reader.Init(data, data.Length,
				TypeModel.IsNonCompressibleType(type) ? CompressOption.NoCompression : option);

			//basic type
			if (WrapperManifest.TryGetWrapper(type, out var wrapper))
			{
				var ret = wrapper.Deserialize(reader);
				ObjectPool<Reader>.Return(reader);
				return ret;
			}

			//code generated type
			if (TypeModel.TryGetWrapper(type, out wrapper))
			{
				//add wrapper
				WrapperManifest.AddWrapper(type, wrapper);
				//start Deserialize
				var ret = wrapper.Deserialize(reader);
				ObjectPool<Reader>.Return(reader);
				return ret;
			}

			var result = Deserialize(type, null, data, reader, option, true, true, true);
			if (result == null)
			{
				ObjectPool<Reader>.Return(reader);
				return null;
			}
			return result;
		}

		/// <summary>
		/// Deserialize a NinoSerialize object
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="data"></param>
		/// <param name="reader"></param>
		/// <param name="option"></param>
		/// <param name="returnDispose"></param>
		/// <param name="skipBasicCheck"></param>
		/// <param name="skipCodeGenCheck"></param>
		/// <param name="skipGenericCheck"></param>
		/// <param name="skipEnumCheck"></param>
		/// <returns></returns>
		internal static T Deserialize<T>(Span<byte> data, Reader reader,
			CompressOption option = CompressOption.Zlib, bool returnDispose = true, bool skipBasicCheck = false,
			bool skipCodeGenCheck = false, bool skipGenericCheck = false, bool skipEnumCheck = false)
		{
			Type type = typeof(T);

			//basic type
			if (!skipBasicCheck && WrapperManifest.TryGetWrapper(type, out var wrapper))
			{
				var ret = ((NinoWrapperBase<T>)wrapper).Deserialize(reader);
				if (returnDispose)
				{
					ObjectPool<Reader>.Return(reader);
				}

				return ret;
			}

			//code generated type
			if (!skipCodeGenCheck && TypeModel.TryGetWrapper(type, out wrapper))
			{
				//add wrapper
				WrapperManifest.AddWrapper(type, wrapper);
				//start Deserialize
				var ret = ((NinoWrapperBase<T>)wrapper).Deserialize(reader);
				if (returnDispose)
				{
					ObjectPool<Reader>.Return(reader);
				}

				return ret;
			}

			return (T)Deserialize(type, null, data, reader, option, returnDispose,
				skipBasicCheck, skipCodeGenCheck, skipGenericCheck, skipEnumCheck);
		}

		/// <summary>
		/// Deserialize a NinoSerialize object
		/// </summary>
		/// <param name="type"></param>
		/// <param name="val"></param>
		/// <param name="data"></param>
		/// <param name="reader"></param>
		/// <param name="option"></param>
		/// <param name="returnDispose"></param>
		/// <param name="skipBasicCheck"></param>
		/// <param name="skipCodeGenCheck"></param>
		/// <param name="skipGenericCheck"></param>
		/// <param name="skipEnumCheck"></param>
		/// <returns></returns>
		/// <exception cref="InvalidOperationException"></exception>
		/// <exception cref="NullReferenceException"></exception>
		// ReSharper disable CognitiveComplexity
		internal static object Deserialize(Type type, object val, byte[] data, Reader reader,
				CompressOption option = CompressOption.Zlib, bool returnDispose = true, bool skipBasicCheck = false,
				bool skipCodeGenCheck = false, bool skipGenericCheck = false, bool skipEnumCheck = false)
			// ReSharper restore CognitiveComplexity
		{
			if (reader == null)
			{
				reader = ObjectPool<Reader>.Request();
				reader.Init(data, data.Length,
					TypeModel.IsNonCompressibleType(type) ? CompressOption.NoCompression : option);
			}

			return Deserialize(type, val, (Span<byte>)data, reader, option, returnDispose, skipBasicCheck,
				skipCodeGenCheck, skipGenericCheck, skipEnumCheck);
		}

		/// <summary>
		/// Deserialize a NinoSerialize object
		/// </summary>
		/// <param name="type"></param>
		/// <param name="val"></param>
		/// <param name="data"></param>
		/// <param name="reader"></param>
		/// <param name="option"></param>
		/// <param name="returnDispose"></param>
		/// <param name="skipBasicCheck"></param>
		/// <param name="skipCodeGenCheck"></param>
		/// <param name="skipGenericCheck"></param>
		/// <param name="skipEnumCheck"></param>
		/// <returns></returns>
		/// <exception cref="InvalidOperationException"></exception>
		/// <exception cref="NullReferenceException"></exception>
		// ReSharper disable CognitiveComplexity
		internal static object Deserialize(Type type, object val, ArraySegment<byte> data, Reader reader,
				CompressOption option = CompressOption.Zlib, bool returnDispose = true, bool skipBasicCheck = false,
				bool skipCodeGenCheck = false, bool skipGenericCheck = false, bool skipEnumCheck = false)
			// ReSharper restore CognitiveComplexity
		{
			if (reader == null)
			{
				reader = ObjectPool<Reader>.Request();
				reader.Init(data, data.Count,
					TypeModel.IsNonCompressibleType(type) ? CompressOption.NoCompression : option);
			}

			return Deserialize(type, val, (Span<byte>)data, reader, option, returnDispose, skipBasicCheck,
				skipCodeGenCheck, skipGenericCheck, skipEnumCheck);
		}

		/// <summary>
		/// Deserialize a NinoSerialize object
		/// </summary>
		/// <param name="type"></param>
		/// <param name="val"></param>
		/// <param name="data"></param>
		/// <param name="reader"></param>
		/// <param name="option"></param>
		/// <param name="returnDispose"></param>
		/// <param name="skipBasicCheck"></param>
		/// <param name="skipCodeGenCheck"></param>
		/// <param name="skipGenericCheck"></param>
		/// <param name="skipEnumCheck"></param>
		/// <returns></returns>
		/// <exception cref="InvalidOperationException"></exception>
		/// <exception cref="NullReferenceException"></exception>
		// ReSharper disable CognitiveComplexity
		internal static object Deserialize(Type type, object val, Span<byte> data, Reader reader,
				CompressOption option = CompressOption.Zlib, bool returnDispose = true, bool skipBasicCheck = false,
				bool skipCodeGenCheck = false, bool skipGenericCheck = false, bool skipEnumCheck = false)
			// ReSharper restore CognitiveComplexity
		{
			//array
			if (!skipGenericCheck && type.IsArray)
			{
				var ret = reader.ReadArray(type);
				if (returnDispose)
				{
					ObjectPool<Reader>.Return(reader);
				}

				return ret;
			}

			//list, dict
			if (!skipGenericCheck && type.IsGenericType)
			{
				var genericDefType = type.GetGenericTypeDefinition();
				//不是list和dict就再见了
				if (genericDefType == ConstMgr.ListDefType)
				{
					var ret = reader.ReadList(type);
					if (returnDispose)
					{
						ObjectPool<Reader>.Return(reader);
					}

					return ret;
				}

				if (genericDefType == ConstMgr.DictDefType)
				{
					var ret = reader.ReadDictionary(type);
					if (returnDispose)
					{
						ObjectPool<Reader>.Return(reader);
					}

					return ret;
				}
			}

#if ILRuntime
			type = type.ResolveRealType();
#endif

			//basic type
			if (!skipBasicCheck && WrapperManifest.TryGetWrapper(type, out var wrapper))
			{
				var ret = wrapper.Deserialize(reader);
				if (returnDispose)
				{
					ObjectPool<Reader>.Return(reader);
				}

				return ret;
			}

			//enum
			if (!skipEnumCheck && TypeModel.IsEnum(type))
			{

				var underlyingType = Enum.GetUnderlyingType(type);
				var ret = Deserialize(underlyingType, null, data, reader, option, returnDispose);
#if ILRuntime
				if (type is ILRuntime.Reflection.ILRuntimeType)
				{
						if (underlyingType == ConstMgr.LongType
						    || underlyingType == ConstMgr.UIntType
						    || underlyingType == ConstMgr.ULongType)
							return Convert.ChangeType(ret, ConstMgr.LongType);
						return Convert.ChangeType(ret, ConstMgr.IntType);
				}
#endif
				ret = Enum.ToObject(type, ret);
				return ret;
			}

			//code generated type
			if (!skipCodeGenCheck && TypeModel.TryGetWrapper(type, out wrapper))
			{
				//add wrapper
				WrapperManifest.AddWrapper(type, wrapper);
				//start Deserialize
				var ret = wrapper.Deserialize(reader);
				if (returnDispose)
				{
					ObjectPool<Reader>.Return(reader);
				}

				return ret;
			}
			
			if (!reader.ReadBool())
			{
				if (returnDispose)
				{
					ObjectPool<Reader>.Return(reader);
				}
				return null;
			}

			//create type
			if (val == null || val == ConstMgr.Null)
			{
#if ILRuntime
				val = ILRuntimeResolver.CreateInstance(type);
#else
				val = Activator.CreateInstance(type);
#endif
			}

			//Get Attribute that indicates a class/struct to be serialized
			TypeModel.TryGetModel(type, out var model);

			//invalid model
			if (model != null && !model.Valid)
			{
				return val;
			}

			//generate model
			if (model == null)
			{
				model = TypeModel.CreateModel(type);
			}

			//min, max index
			ushort min = model.Min, max = model.Max;

			void Read()
			{
				//only include all model need this
				if (model.IncludeAll)
				{
					//read len
					var len = reader.ReadLength();
					Dictionary<string, object> values = new Dictionary<string, object>(len);
					//read elements key by key
					for (int i = 0; i < len; i++)
					{
						var key = reader.ReadString();
						var typeFullName = reader.ReadString();
						var value = Deserialize(Type.GetType(typeFullName), ConstMgr.Null, ConstMgr.Null, reader,
							option, false);
						values.Add(key, value);
					}

					//set elements
					while (min <= max)
					{
						//prevent index not exist
						if (!model.Types.ContainsKey(min))
						{
							min++;
							continue;
						}

						//get the member
						var member = model.Members[min];
						//member type
						type = model.Types[min];
						//try get same member and set it
						if (values.TryGetValue(member.Name, out var ret))
						{
							//type check
#if !ILRuntime
							if (ret.GetType() != type)
							{
								if (TypeModel.IsEnum(type))
								{
									ret = Enum.ToObject(type, ret);
								}
								else
								{
									ret = Convert.ChangeType(ret, type);
								}
							}
#endif

							SetMember(model.Members[min], val, ret);
						}

						min++;
					}
				}
				else
				{
					while (min <= max)
					{
						//if end, skip
						if (reader.EndOfReader)
						{
							min++;
							break;
						}

						//prevent index not exist
						if (!model.Types.ContainsKey(min))
						{
							min++;
							continue;
						}

						//get type of that member
						type = model.Types[min];
						//try code gen, if no code gen then reflection

						//read basic values
						var ret = Deserialize(type, ConstMgr.Null, Span<byte>.Empty, reader, option, false);
						//type check
#if !ILRuntime
						if (TypeModel.IsEnum(type))
						{
							ret = Enum.ToObject(type, ret);
						}
						else
						{
							ret = Convert.ChangeType(ret, type);
						}
#endif

						SetMember(model.Members[min], val, ret);
						min++;
					}
				}
			}

			//start Deserialize
			Read();
			if (returnDispose)
			{
				ObjectPool<Reader>.Return(reader);
			}

			return val;
		}

		/// <summary>
		/// Set value from MemberInfo
		/// </summary>
		/// <param name="info"></param>
		/// <param name="instance"></param>
		/// <param name="val"></param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void SetMember(MemberInfo info, object instance, object val)
		{
			switch (info)
			{
				case FieldInfo fo:
					fo.SetValue(instance, val);
					break;
				case PropertyInfo po:
					po.SetValue(instance, val);
					break;
				default:
					return;
			}
		}
	}
	// ReSharper restore UnusedParameter.Local
}