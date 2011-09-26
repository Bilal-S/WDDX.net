using System;
using System.Xml;
using System.Collections;

namespace Mueller.Wddx
{
	/// <summary>
	///		Deserializes a WDDX array element into a <see cref="ArrayList"/> object.
	/// </summary>
    /// <remarks>
    ///		This class is a Singleton class - only one instance of it will ever exist
    ///		in a given AppDomain.
    ///	</remarks>
    ///	<seealso cref="IWddxElementDeserializer"/>
    internal class ArrayDeserializer : IWddxElementDeserializer
    {
		private static ArrayDeserializer instance = null;

		/// <summary>
		///		Provides access to the instance of this class.
		/// </summary>
		public static ArrayDeserializer Instance
		{
			get
			{
				if (instance == null)
				{
					lock (typeof(ArrayDeserializer))
					{
						if (instance == null)
							instance = new ArrayDeserializer();
					}
				}

				return instance;
			}
		}

		private ArrayDeserializer() {}

		/// <summary>
		///		Parses the WDDX element and returns the deserialized
		///		content as an <see cref="ArrayList"/> object, advancing the reader to the next
		///		element.
		/// </summary>
		/// <param name="input">The pre-initialized <see cref="System.Xml.XmlTextReader"/> pointing to the WDDX to be parsed.</param>
		public object ParseElement(XmlReader input)
		{
			if (input.IsEmptyElement)
			{
				input.Skip();
				return new ArrayList();
			}

			int ArrayLen = Int32.Parse(input.GetAttribute("length"));
			ArrayList thisList = new ArrayList(ArrayLen);

			object elementValue;
			IWddxElementDeserializer deserializer;
			input.Read();
			while (!(input.Name == "array" && input.NodeType == XmlNodeType.EndElement))
			{
				deserializer = WddxElementDeserializerFactory.GetDeserializer(input.Name);
				elementValue = deserializer.ParseElement(input);
				thisList.Add(elementValue);
			}
			input.ReadEndElement();

			return thisList;
		}
    }
}
