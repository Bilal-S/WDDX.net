using System;
using System.Xml;
using System.Collections;

namespace Mueller.Wddx
{
	/// <summary>
	///		Deserializes a WDDX struct element into a <see cref="Hashtable"/> object.
	/// </summary>
    /// <remarks>
    ///		This class is a Singleton class - only one instance of it will ever exist
    ///		in a given AppDomain.
    ///	</remarks>
    ///	<seealso cref="IWddxElementDeserializer"/>
    internal class StructDeserializer : IWddxElementDeserializer
    {
		private static StructDeserializer instance = null;

		/// <summary>
		///		Provides access to the instance of this class.
		/// </summary>
		public static StructDeserializer Instance
		{
			get
			{
				if (instance == null)
				{
					lock (typeof(StructDeserializer))
					{
						if (instance == null)
							instance = new StructDeserializer();
					}
				}

				return instance;
			}
		}

		private StructDeserializer() {}

		/// <summary>
		///		Parses the WDDX element and returns the deserialized
		///		content as a <see cref="Hashtable"/> object, advancing the reader to the next
		///		element.
		/// </summary>
		/// <param name="input">The pre-initialized <see cref="System.Xml.XmlTextReader"/> pointing to the WDDX to be parsed.</param>
		public object ParseElement(XmlReader input)
		{
			Hashtable thisTable = new Hashtable();

			if (input.IsEmptyElement)
			{
				input.Skip();
				return thisTable;
			}

			string elementName;
			object elementValue;
			IWddxElementDeserializer deserializer;
			while (input.Read() && (!(input.Name == "struct" && input.NodeType == XmlNodeType.EndElement)))
			{
				if (input.Name == "var" && input.NodeType != XmlNodeType.EndElement)
				{
					elementName = input.GetAttribute("name");
					input.Read();  // move to contents of <var>
					deserializer = WddxElementDeserializerFactory.GetDeserializer(input.Name);
					elementValue = deserializer.ParseElement(input);
					thisTable.Add(elementName, elementValue);
				}
			}
			input.ReadEndElement();

			return thisTable;
		}
    }
}
