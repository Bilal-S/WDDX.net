using System;
using System.Xml;
using System.Collections;

namespace Mueller.Wddx
{
	/// <summary>
	///		Deserializes a WDDX binary element into a byte array.
	/// </summary>
    /// <remarks>
    ///		This class is a Singleton class - only one instance of it will ever exist
    ///		in a given AppDomain.
    ///	</remarks>
    ///	<seealso cref="IWddxElementDeserializer"/>
    internal class BinaryDeserializer : IWddxElementDeserializer
    {
		private const int BUFFERSIZE = 1024;

		private static BinaryDeserializer instance = null;

		/// <summary>
		///		Provides access to the instance of this class.
		/// </summary>
		public static BinaryDeserializer Instance
		{
			get
			{
				if (instance == null)
				{
					lock (typeof(BinaryDeserializer))
					{
						if (instance == null)
							instance = new BinaryDeserializer();
					}
				}

				return instance;
			}
		}

		private BinaryDeserializer() {}

		/// <summary>
		///		Parses the WDDX element and returns the deserialized
		///		content as a byte array, advancing the reader to the next
		///		element.
		/// </summary>
		/// <remarks>
		///		Binary deserialization can be significantly less-efficient when validating.
		/// </remarks>
		/// <param name="input">The pre-initialized <see cref="System.Xml.XmlTextReader"/> pointing to the WDDX to be parsed.</param>
		public object ParseElement(XmlReader input)
		{
			if (input.IsEmptyElement)
			{
				input.Skip();
				return new byte[0];
			}

			XmlTextReader reader = input as XmlTextReader;

			if (reader == null)
			{
				if (input is XmlValidatingReader)
				{
					// The XmlValidatingReader does not support chunked Base64 decoding.
					return Convert.FromBase64String(input.ReadString());
				}
				else
					throw new ArgumentException("input must be an XmlTextReader or XmlValidatingReader.");
			}

			string LenAttribute = reader.GetAttribute("length");
			byte[] binaryData = null;

			// the length attribute is optional in the WDDX spec.
			if (LenAttribute != null)
			{
				// we know the length of the binary data
				int Length = int.Parse(LenAttribute);
				binaryData = new byte[Length];
				int pos = 0;
				int read = 0;

				// read the binary data in BufferSize increments
				while ((read = reader.ReadBase64(binaryData, pos, Math.Min(BUFFERSIZE, Length - pos))) > 0)
				{
					pos += read;
				}
			}
			else
			{
				// we don't know the length of the binary data (this method will be slower)
				byte[] buffer = new byte[BUFFERSIZE];
				
				try
				{
					// read the binary data in BufferSize increments
					int read = 0;
					while ((read = reader.ReadBase64(buffer, 0, BUFFERSIZE)) > 0)
					{
						if (binaryData == null)
						{
							binaryData = new byte[read];
							Array.Copy(buffer, 0, binaryData, 0, read);
						}
						else
						{
							byte[] temp = new byte[read + binaryData.Length];
							Array.Copy(binaryData, 0, temp, 0, binaryData.Length);
							Array.Copy(buffer, 0, temp, binaryData.Length, read);
							binaryData = temp;
						}
					}
				}
				catch (XmlException)
				{
					// This exception gets thrown when ReadBase64 reads
					// past the end of the data (bug?).  Since we've got all
					// our data at this point, we can just silently swallow
					// the exception and do nothing.
				}
			}

			return binaryData;
		}
    }
}
