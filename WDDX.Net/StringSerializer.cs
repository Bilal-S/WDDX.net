using System;
using System.Xml;
using System.Text;
using System.Text.RegularExpressions;

namespace Mueller.Wddx
{
	/// <summary>
	///		Serializes a string.
	/// </summary>
	/// <remarks>
	///		This class is a Singleton class - only one instance of it will ever exist
	///		in a given AppDomain.
	///	</remarks>
	///	<seealso cref="IWddxObjectSerializer"/>
	internal class StringSerializer : IWddxObjectSerializer
	{
		// regex to find control characters
		private static Regex REControl = new Regex(@"([\a\b\t\r\v\f\n\e\cA-\cZ])", RegexOptions.Compiled);
		private static StringSerializer instance = null;

		/// <summary>
		///		Provides access to the instance of this class.
		/// </summary>
		public static StringSerializer Instance
		{
			get
			{
				if (instance == null)
				{
					lock (typeof(StringSerializer))
					{
						if (instance == null)
							instance = new StringSerializer();
					}
				}

				return instance;
			}
		}

		private StringSerializer() {}

		/// <summary>
		///		Writes the serialized object to the XML stream.
		/// </summary>
		/// <param name="output">A pre-initialized <see cref="XmlTextWriter"/> object.</param>
		/// <param name="obj">Object to serialize.</param>
		public void WriteObject(XmlWriter output, object obj)
		{
			string theString = obj.ToString();

			Match match;
			if ((match = REControl.Match(theString)).Success)
			{
				// escape any control characters with a <char code="xx"/> tag
				int pos = 0;
				output.WriteStartElement("string");
				foreach (Capture capture in match.Captures)
				{
					output.WriteString(theString.Substring(pos, capture.Index - pos));
					output.WriteStartElement("char");
					output.WriteAttributeString("code", ((int)capture.Value[0]).ToString("x2"));
					output.WriteEndElement();
					pos = capture.Index + capture.Length;
				}
				output.WriteString(theString.Substring(pos));
				output.WriteEndElement();
			}
			else
			{
				output.WriteElementString("string", theString);
			}
		}
	}
}
