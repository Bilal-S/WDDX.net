using System;
using System.Collections;
using System.Collections.Specialized;
using System.Data;

namespace Mueller.Wddx
{
    /// <summary>
    ///		This class provides the appropriate serializer based on the
    ///		type of object being serialized.
    /// </summary>
    internal class WddxObjectSerializerFactory
    {
		/// <summary>
		///		Returns the appropriate serializer based on an object instance.
		/// </summary>
		/// <param name="obj">The object to be serialized.</param>
		public IWddxObjectSerializer GetSerializer(object obj)
		{
			return this.GetObjectSerializer(obj);
		}

		/// <summary>
		///		Returns the appropriate serializer based on an object type.
		/// </summary>
		/// <param name="type">The type to be serialized.</param>
		public IWddxObjectSerializer GetSerializer(Type type)
		{
			return this.GetTypeSerializer(type.FullName);
		}

		private IWddxObjectSerializer GetObjectSerializer(object obj)
		{
			if (obj == null || obj == DBNull.Value)
			{
				return NullSerializer.Instance;
			}
			else if (obj is IDictionary)
			{
				// Any object that implements IDictionary can be treated as a "struct"
				// for the purposes of Wddx serialization.
				return StructSerializer.Instance;
			}
			else if (obj is ICollection)
			{
				if (obj.GetType() == typeof(System.Byte[]))
				{
					// Byte arrays are handled by the binary serializer.
					return BinarySerializer.Instance;
				}
				else
				{
					// Any other object that implements ICollection can be treated as an "array"
					// for the purposes of Wddx serialization.
					return ArraySerializer.Instance;					
				}
			}
			else if (obj is DataSet)
			{
				// do this test to catch strongly-typed datasets that are subclasses of DataSet
				return DataSetSerializer.Instance;
			}
			else
			{
				return this.GetTypeSerializer(obj.GetType().FullName);
			}
		}

		private IWddxObjectSerializer GetTypeSerializer(string className)
		{
			switch (className)
			{
				case "System.Char":
				case "System.String":
					return StringSerializer.Instance;
				case "System.Byte":
				case "System.Int16":
				case "System.Int32":
				case "System.Int64":
				case "System.Single":
				case "System.Double":
				case "System.Decimal":
					return NumberSerializer.Instance;
				case "System.Boolean":
					return BoolSerializer.Instance;
				case "System.DateTime":
					return DateTimeSerializer.Instance;
				case "System.Collections.Hashtable":
					return StructSerializer.Instance;
				case "System.Data.DataSet":
					return DataSetSerializer.Instance;
				case "System.Byte[]":
					return BinarySerializer.Instance;
				case "System.Collections.ArrayList":
				case "System.Array":
				case "System.String[]":
				case "System.Int16[]":
				case "System.Int32[]":
				case "System.Int64[]":
				case "System.Single[]":
				case "System.Double[]":
				case "System.Decimal[]":
				case "System.DateTime[]":
					return ArraySerializer.Instance;
				default:
					return GenericSerializer.Instance;
			}
		}
    }
}
