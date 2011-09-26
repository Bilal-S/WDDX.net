using System;
using System.Xml;

namespace Mueller.Wddx
{
    /// <summary>
    ///		Serializes a byte array as a WDDX <c>binary</c> element (Base64-encoded).
    /// </summary>
    ///	<seealso cref="IWddxObjectSerializer"/>
    internal class BinarySerializer : IWddxObjectSerializer
    {
		private static BinarySerializer instance = null;

		/// <summary>
		///		Provides access to the instance of this class.
		/// </summary>
		public static BinarySerializer Instance
		{
			get
			{
				if (instance == null)
				{
					lock (typeof(BinarySerializer))
					{
						if (instance == null)
							instance = new BinarySerializer();
					}
				}

				return instance;
			}
		}

		private BinarySerializer() {}

		/// <summary>
		///		Writes the serialized object to the XML stream.
		/// </summary>
		/// <param name="output">A pre-initialized <see cref="XmlTextWriter"/> object.</param>
		/// <param name="obj">Object to serialize.</param>
		public void WriteObject(XmlWriter output, object obj)
		{
			// Write a byte array out as a "binary" element, base64 encoded.
			byte[] buffer = obj as byte[];

			if (buffer == null)
				throw new WddxException("Attemped to serialize incompatible object. Expected: " + 
					typeof(byte[]).FullName + " but got: " +
					((obj == null) ? "null" : obj.GetType().FullName));

			output.WriteStartElement("binary");
			output.WriteAttributeString("length", buffer.Length.ToString());
			output.WriteBase64(buffer, 0, buffer.Length);
			output.WriteEndElement();
		}
    }
}
