using System;
using System.IO;
using System.Xml;

namespace Mueller.Wddx
{
    /// <summary>
    ///		This class is the main entry point for deserializing a WDDX packet
    ///		into a native CLR object.
    /// </summary>
    public class WddxDeserializer
    {
		/// <summary>
		///		Deserializes a string containing a WDDX packet. Does not perform validation on the packet.
		/// </summary>
		/// <param name="WddxPacket">The WDDX packet to be deserialized.</param>
		public object Deserialize(string WddxPacket)
		{
			return this.Deserialize(WddxPacket, false);
		}

		/// <summary>
		///		Deserializes a string containing a WDDX packet.
		/// </summary>
		/// <param name="WddxPacket">The WDDX packet to be deserialized.</param>
		/// <param name="validate">
		///		Boolean indicating whether or not validation should be performed during deserialization.
		///	</param>
		/// <remarks>
		///		If <c>validate</c> is set to <c>true</c>, this method will throw a <see cref="WddxValidationException"/>
		///		if it encounters invalid WDDX.
		/// </remarks>
		/// <exception cref="WddxValidationException">
		///		The <c>validate</c> parameter is set to <c>true</c> and invalid WDDX is encountered.
		///	</exception>
		public object Deserialize(string WddxPacket, bool validate)
		{
			object retVal = null;

			// open an XmlTextReader on the string
			using (StringReader stream = new StringReader(WddxPacket))
			{
				XmlTextReader reader = new XmlTextReader(stream);
				reader.Namespaces = false;
				retVal = this.Deserialize(reader, validate);
				reader.Close();
			}

			return retVal;
		}

		/// <summary>
		///		Deserialize an <see cref="System.Xml.XmlTextReader"/> pointing to a WDDX packet. 
		///		Does not perform validation on the packet.
		/// </summary>
		/// <param name="input">The pre-initialized <see cref="System.Xml.XmlTextReader"/> pointing to the WDDX to be parsed.</param>
		public object Deserialize(XmlTextReader input)
		{
			return this.Deserialize(input, false);
		}

		/// <summary>
		///		Deserialize an <see cref="System.Xml.XmlTextReader"/> pointing to a WDDX packet.
		/// </summary>
		/// <param name="input">The pre-initialized <see cref="System.Xml.XmlTextReader"/> pointing to the WDDX to be parsed.</param>
		/// <param name="validate">
		///		Boolean indicating whether or not validation should be performed during deserialization.
		///	</param>
		/// <remarks>
		///		If <c>validate</c> is set to <c>true</c>, this method will throw a <see cref="WddxValidationException"/>
		///		if it encounters invalid WDDX.
		///		
		///		Note that ColdFusion can produce <c>dateTime</c> elements that the WDDX XML Schema
		///		cannot validate, so this method may throw a <see cref="WddxValidationException"/>
		///		for packets that can be successfully deserialized if validation is turned off.
		/// </remarks>
		/// <exception cref="WddxValidationException">
		///		The <c>validate</c> parameter is set to <c>true</c> and invalid WDDX is encountered.
		///	</exception>
		public object Deserialize(XmlTextReader input, bool validate)
		{
			if (validate)
			{
				WddxValidator validator = new WddxValidator();
				return validator.Deserialize(input);
			}
			else
			{
				IWddxElementDeserializer deserializer;
				object retVal = null;
			
				while (input.Read())
				{
					if (input.NodeType == XmlNodeType.Element && input.Name == "data")
					{
						input.Read();  // move to next node after <data>
						deserializer = WddxElementDeserializerFactory.GetDeserializer(input.Name);
						retVal = deserializer.ParseElement(input);
					}
				}

				return retVal;
			}
		}

		/// <summary>
		///		Validates the given WDDX packet against the WDDX XML Schema,
		///		and indicates whether or not the packet passed validation.
		/// </summary>
		/// <remarks>
		///		ColdFusion can produce <c>dateTime</c> elements that the WDDX XML Schema
		///		cannot validate, so this method may return <b>false</b> for packets that
		///		can be successfully deserialized.
		/// </remarks>
		/// <param name="WddxPacket">The WDDX packet to be validated.</param>
		/// <returns>Returns <c>true</c> if the packet passes validation, <c>false</c> if it does not.</returns>
		public bool IsValid(string WddxPacket)
		{
			bool isValid = false;

			// open an XmlTextReader on the string
			using (StringReader stream = new StringReader(WddxPacket))
			{


				XmlTextReader reader = new XmlTextReader(stream);
				reader.Namespaces = false;
				isValid = this.IsValid(reader);
				reader.Close();
			}

			return isValid;
		}

		/// <summary>
		///		Validates the given WDDX packet against the WDDX XML Schema,
		///		and indicates whether or not the packet passed validation.
		/// </summary>
		/// <remarks>
		///		ColdFusion can produce <c>dateTime</c> elements that the WDDX XML Schema
		///		cannot validate, so this method may return <b>false</b> for packets that
		///		can be successfully deserialized.
		/// </remarks>
		/// <param name="input">The pre-initialized <see cref="System.Xml.XmlTextReader"/> pointing to the WDDX to be validated.</param>
		/// <returns>Returns <c>true</c> if the packet passes validation, <c>false</c> if it does not.</returns>
		public bool IsValid(XmlTextReader input)
		{
			WddxValidator validator = new WddxValidator();
			return validator.IsValid(input);
		}
    }
}
