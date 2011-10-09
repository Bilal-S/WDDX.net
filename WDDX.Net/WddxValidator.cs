using System;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Reflection;

namespace Mueller.Wddx
{
	/// <summary>
	///		Provides validation services for WDDX.
	/// </summary>
	internal class WddxValidator
	{
		//private static XmlSchemaCollection _schemaCache = null;
        private static XmlSchemaSet _schemaCache = null;
        private bool _isValid;

		/// <summary>
		///		Validates the given WDDX packet against the WDDX XML Schema,
		///		and indicates whether or not the packet passed validation.
        ///		XmlTextReader does not provide data validation only that document is wellformed
		/// </summary>
		/// <param name="input">The pre-initialized <see cref="System.Xml.XmlTextReader"/> pointing to the WDDX to be validated.</param>
		/// <returns>Returns <c>true</c> if the packet passes validation, <c>false</c> if it does not.</returns>
		public bool IsValid(XmlTextReader input)
		{
            try
            {
                //define the schema set
                this.SetSchemaSet();
                // Set the validation settings and schema reference
                XmlReaderSettings settings = new XmlReaderSettings();
                settings.ValidationType = ValidationType.Schema;
                settings.IgnoreComments = true;
                settings.IgnoreWhitespace = true;
                settings.Schemas = _schemaCache;
                settings.ValidationEventHandler += new ValidationEventHandler(this.ValidationCheckHandler);

                //set up the reader
                XmlReader reader = XmlReader.Create(input, settings);
                       
                //assume is valid before reading starts
                _isValid = true;

                // spin through the document if we are not already at final node
                if (!reader.EOF)
                {
                    while (reader.Read());
                 
                }
            }
            catch (Exception e)
            {
                _isValid = false; //any exceptioin in this process invalidates the xml
       
            }

			return _isValid;
		}

		/// <summary>
		///		Deserialize an <see cref="System.Xml.XmlTextReader"/> pointing to a WDDX packet.
		/// </summary>
		/// <param name="input">The pre-initialized <see cref="System.Xml.XmlTextReader"/> pointing to the WDDX to be parsed.</param>
		/// <exception cref="WddxValidationException">
		///		Invalid WDDX is encountered.
		///	</exception>
		public object Deserialize(XmlTextReader input)
		{
			object retVal = null;

            //define the schema set
            this.SetSchemaSet();
            // Set the validation settings and schema reference
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.ValidationType = ValidationType.Schema;
            settings.IgnoreComments = true;
            settings.IgnoreWhitespace = true;
            settings.Schemas = _schemaCache;
            settings.ValidationEventHandler += new ValidationEventHandler(this.ValidationCheckHandler);

            //set up the reader
            XmlReader reader = XmlReader.Create(input, settings);       

			IWddxElementDeserializer deserializer;
            try
            {
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element && reader.Name == "data")
                    {
                        reader.Read();  // move to next node after <data>
                        deserializer = WddxElementDeserializerFactory.GetDeserializer(reader.Name);
                        retVal = deserializer.ParseElement(reader);
                    }
                }
            }
            catch (Exception e) { 
                //we also mark anything that runs into trouble during Validated desirialization with the validation exception
                throw new WddxValidationException("Validation error parsing WDDX packet (B).", e.Message);
            }

			return retVal;
		}

		private void ValidationCheckHandler(object sender, ValidationEventArgs args)
		{
			// we got a validation error, but don't want details
			_isValid = false;
		}

		private void ValidationErrorHandler(object sender, ValidationEventArgs args)
		{
			throw new WddxValidationException("Validation error parsing WDDX packet.", args.Message);
		}

        //helper function to set schema set if not already done
        private void SetSchemaSet()
        {
            if (_schemaCache == null)
            {
                lock (typeof(WddxDeserializer))
                {
                    if (_schemaCache == null)
                    {
                        // get the xsd
                        Stream xsdStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Mueller.Wddx.wddx_0100.xsd");
                        XmlTextReader xsdReader = new XmlTextReader(xsdStream);

                        //add to schema set
                        XmlSchemaSet sc = new XmlSchemaSet();
                        sc.Add("", xsdReader);
                      
                        // cache it
                        _schemaCache = sc;
                    }
                }
            }
        }

        //depricated function GetSchema needs to be removed -- bsoylu
        /*
		private XmlSchemaCollection GetSchema()
		{
			if (_schemaCache == null)
			{
				lock (typeof(WddxDeserializer))
				{
					if (_schemaCache == null)
					{
						// build schema collection
						Stream xsdStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Mueller.Wddx.wddx_0100.xsd");
						XmlTextReader xsdReader = new XmlTextReader(xsdStream);

						XmlSchemaCollection xsc = new XmlSchemaCollection();
                        //used to be null set to empty string -- bsoylu
						xsc.Add("", xsdReader);

						// cache it
						_schemaCache = xsc;
					}
				}
			}

			return _schemaCache;
		} 
        */
	}
}
