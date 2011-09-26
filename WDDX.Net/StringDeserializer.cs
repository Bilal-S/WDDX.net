using System;
using System.Xml;
using System.Text;
using System.Globalization;

namespace Mueller.Wddx
{
	/// <summary>
	///		Deserializes a string.
	/// </summary>
	/// <remarks>
	///		This class is a Singleton class - only one instance of it will ever exist
	///		in a given AppDomain.
	///	</remarks>
	///	<seealso cref="IWddxElementDeserializer"/>
	internal class StringDeserializer : IWddxElementDeserializer
	{
		private static StringDeserializer instance = null;

		/// <summary>
		///		Provides access to the instance of this class.
		/// </summary>
		public static StringDeserializer Instance
		{
			get
			{
				if (instance == null)
				{
					lock (typeof(StringDeserializer))
					{
						if (instance == null)
							instance = new StringDeserializer();
					}
				}

				return instance;
			}
		}

		private StringDeserializer() {}

		/// <summary>
		///		Parses the WDDX element and returns the deserialized
		///		content as a <see cref="System.String"/> object, advancing the reader to the next
		///		element.
		/// </summary>
		/// <param name="input">The pre-initialized <see cref="System.Xml.XmlTextReader"/> pointing to the WDDX to be parsed.</param>
		public object ParseElement(XmlReader input)
		{
			if (input.IsEmptyElement)
			{
				input.Skip();
				return String.Empty;
			}

			StringBuilder output = new StringBuilder();
			input.Read();
			
			while (!(input.Name == "string" && input.NodeType == XmlNodeType.EndElement))
			{
				if (input.NodeType == XmlNodeType.Text || input.NodeType == XmlNodeType.Whitespace)
				{
					output.Append(input.ReadString());
				}
				else if (input.Name == "char")
				{
					// parse out the <char code="xx"/> tag
					output.Append((char)Int32.Parse(input.GetAttribute("code"), NumberStyles.AllowHexSpecifier));
					input.Skip();
				}
			}

			input.ReadEndElement();

			return output.ToString();
		}
	}
}
