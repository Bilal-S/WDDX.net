using System;

namespace Mueller.Wddx
{
    /// <summary>
    ///		This class provides the appropriate deserializer
    ///		based on the tag name of the element being processed.
    /// </summary>
    internal class WddxElementDeserializerFactory
    {
		private WddxElementDeserializerFactory() {}

		/// <summary>
		///		Gets the appropriate deserializer based on the tag name
		///		of the element being processed.
		/// </summary>
		/// <param name="TagName">A valid WDDX tag name.</param>
        public static IWddxElementDeserializer GetDeserializer(string TagName)
		{
			switch (TagName)
			{
				case "string":
					return StringDeserializer.Instance;
				case "number":
					return NumberDeserializer.Instance;
				case "dateTime":
					return DateTimeDeserializer.Instance;
				case "boolean":
					return BoolDeserializer.Instance;
				case "null":
					return NullDeserializer.Instance;
				case "struct":
					return StructDeserializer.Instance;
				case "array":
					return ArrayDeserializer.Instance;
				case "binary":
					return BinaryDeserializer.Instance;
				case "recordset":
					return DataSetDeserializer.Instance;
				default:
					throw new ArgumentException("Unsupported tag: " + TagName);
			}
		}
    }
}
