### WDDX.Net

#### About
WDDX (Web Distributed Data eXchange) is a programming-language-, platform- and transport-neutral data interchange mechanism to pass data between different environments and different computers. This is WDDX implementation for .NET platforms. Source code is in C#. The realease dll is in releases subdirectory (wddx.net.dll).

The .NET implementation is based on work from Joel Mueller.
Changes in version 1.0.4 contributed by Bilal Soylu.

#### help

Windows help (.chm) files are available in the Release directory.

### Implementation

**Version 1.0.4**

[Joel Mueller](mailto:jmueller@swiftk.com)

The only classes you need to know are WddxSerializer and WddxDeserializer. Click [here](WDDX.Net.chm) for documentation. For those who want to play with the source, full documentation on the internal API is also supplied [here](WDDX.Net%20Internals.chm). See the "WDDX Tests" project for usage samples and tests.

The following table compares the WDDX data types with the .NET Common Language Runtime types that they map to.

| WDDX | .NET CLR |
| null | A null object reference (null in C#, Nothing in VB.NET) |
| boolean | System.Boolean (bool in C#, Boolean in VB.NET) |
| number | One of the following, depending on what fits:

| CLR | C# | VB.NET |
| System.Int16 | short | Short |
| System.Int32 | int | Integer |
| System.Int64 | long | Long |
| System.Single | float | Single |
| System.Double | double | Double |
| System.Decimal | decimal | Decimal |

 |
| dateTime | System.DateTime (Date in VB.NET) |
| string | System.String (string in C#, String in VB.NET) |
| array | System.Collections.ArrayList (see notes below for details) |
| struct | System.Collections.Hashtable (see notes below for details) |
| recordset | System.Data.DataSet (see notes below for details) |
| binary | 

A System.Byte array (byte[] in C#, Byte() in VB.NET)

 |
| char | System.Char (char in C#, Char in VB.NET) |

* The precision of the System.Decimal type is not directly supported by WDDX; when serialized if numbers have higher precision than 15 they will be first converted to Double.

**New in 1.0.4 (Bilal Soylu):**

*   updraded to .net framework 3
*   updated unit test to nunit 2.5
*   corrected iso8601 date and time interpretation and unit tests. Hours are now correctly translated across time zones.Corrected date time serialization.
*   fixed IsValid() function
*   introduced System.decimal as an alternate numeric return for improvement in precision for up to 15 digits after the decimal
*   upgraded XML checking components to framework 3\. Removed obsolete XMLValidatingReader calls in core validators.
*   ignore whitespace and comments so human formatted xml can be parsed without exceptions.
*   fix handling of dataset with starting null-data records

**New in 1.0.3:**

*   Fixed a bug in string deserialization that could result in an infinite loop in certain situations.
*   Updated unit tests to work with [NUnit 2.0](http://nunit.sourceforge.net/).
*   Slight performance improvement when deserializing large recordsets.

**New in 1.0.2:**

*   Fixed minor deserialization bugs for dateTime and string elements in large recordsets.

**New in 1.0.1:**

*   Fixed date parsing to properly handle non-ISO8601 dates produced by ColdFusion.
*   Refactored for better error handling during serialization. 
*   Known issue - any non-ISO8601 dates (such as 2002-7-2T8:3:0-5:0, without leading zeros) will cause XSD validation to fail even though the parser can handle these dates. For this reason, is it possible for WddxDeserializer.IsValid() to return false on a packet that it can successfully parse.

**New in 1.0:**

*   Support for NUnit 1.11 in the unit tests.

**New in RC2:**

*   Fixed a bug with serialization of database nulls from a DataSet.
*   Made the addition of the XML document declaration optional, with a default of false, as CFWDDX throws an error rather than ignoring the XML declaration.

**New in RC1:**

*   Added support for WDDX validation via XSD. This is supported in two ways - WddxDeserializer.IsValid() will determine if the WDDX packet passed to it is valid WDDX without attempting to deserialize it, and is extremely fast. There is also an option to validate the WDDX as it is being deserialized; this option will cause the deserializer to throw an exception detailing what exactly is wrong with any invalid WDDX that it encounters. Note that the XSD in question is embedded into the WDDX.Net DLL, meaning that there is only one file to work with, and no internet access is required to download an external XSD.
*   Added support for the WDDX "char" element for escaped control characters in strings.
*   Fixed dateTime serialization and deserialization to properly convert to/from UTC time.

**Serialization:**

*   Can serialize to a string or a pre-existing XmlTextWriter. The pre-existing XmlTextWriter can point to a string, a file, a network stream, the Response stream in ASP.NET, database, etc...
*   Any object that implements the IDictionary interface is serialized into a WDDX "struct" element. This includes the Hashtable, as well as most other classes that contain name/value pairs.
*   Any object that implements the ICollection interface is serialized into a WDDX "array" element. This includes all arrays of all data types (jagged, single-, and multi-dimensional) as well as the ArrayList class, and most other classes that contain a collection of items. Since WDDX doesn't support true multi-dimensional arrays, they are currently flattened to a one-dimensional array upon serialization.
*   A byte array is serialized as a WDDX "binary" element.
*   A DataSet is serialized as a WDDX "recordset" element. In the case of a DataSet that contains multiple DataTables, it is serialized as a WDDX "struct" element that contains multiple "recordset" elements. The name of each DataTable is the key into the struct.
*   Any object that doesn't fit a pre-defined serializer, but does have properties or fields will be serialized as a WDDX "struct" element, with the property names/values being the names and values in the struct. If the object has no properties or fields, the serializer will call the ToString() method of the object, and serialize it as a WDDX "string" element.

**Deserialization:**

*   Can deserialize from a string or a pre-existing XmlTextReader. The pre-existing XmlTextReader can point to a string, a file, a network stream, a database, etc...
*   A WDDX "array" element is deserialized into an ArrayList object. In the case of nested (or jagged) arrays, it will be an ArrayList object containing other ArrayList objects. Note that a single-dimensional ArrayList can be converted to an array of native types using the following syntax (C#):
    <tt>int[] intArray = (int[])theArrayList.ToArray(typeof(int));</tt>
*   A WDDX "struct" element is deserialized into a Hashtable object.
*   A WDDX "recordset" element is deserialized into a DataSet object. Data types and column names are preserved.
*   A WDDX "binary" element is deserialized into a byte array.

**Still to be done:**

*   Find a way to express ColdFusion's "loose" interpretation of ISO8601 dates in XSD form, so that WDDX can be reliably validated.
*   Implement support for the optional "type" attribute supported by most WDDX elements. This would allow type preservation for better round-trips, but would not benefit any non-.NET languages. 
*   Convert multi-dimensional arrays to nested/jagged arrays before serialization.
*   Optimize array deserialization to produce arrays of native types when possible, instead of ArrayList objects.
*   The above items are more likely to be done if I know someone out there needs/will use them. So if any of these sound good, [send me a note](mailto:jmueller@swiftk.com)!

LEGALESE: This library is provided without any warranty, expressed or implied. I hope it's useful to you, but if it's not, well, you get what you pay for.

