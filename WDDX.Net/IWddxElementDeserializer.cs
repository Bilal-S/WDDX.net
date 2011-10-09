using System;
using System.Xml;

namespace Mueller.Wddx
{
    /// <summary>
    ///    Interface that defines the behavior for all deserializers.
    /// </summary>
    internal interface IWddxElementDeserializer
    {
		/// <summary>
		///		Parses the WDDX element and returns the deserialized
		///		content as an object, advancing the reader to the next
		///		element.
		/// </summary>
		/// <param name="input">The pre-initialized <see cref="System.Xml.XmlTextReader"/> pointing to the WDDX to be parsed.</param>
        object ParseElement(XmlReader input);
        //object ParseElement(XmlValidatingReader input);
    }
}
