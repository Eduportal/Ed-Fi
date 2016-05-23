namespace EdFi.Ods.CodeGen.XsdToWebApi.Parse
{
    using System;
    using System.Xml.Schema;

    public interface ISchemaValidation {}

    public class MaxStringLength : ISchemaValidation
    {
        public int? MaxLength { get; set; }

        public override string ToString()
        {
            if (this.MaxLength == null)
                return "(max)";

            return string.Format("({0})", this.MaxLength);
        }


        public static MaxStringLength GetMaxStringLength(XmlSchemaSimpleType schemaType)
        {
            if (schemaType.TypeCode == XmlTypeCode.Duration)
            {
                return new MaxStringLength
                           {
                                   MaxLength = 30
                           };
            }

            var restriction = schemaType.Content as XmlSchemaSimpleTypeRestriction;
            foreach (var facet in restriction.Facets)
            {
                var maxLenth = facet as XmlSchemaMaxLengthFacet;
                if (maxLenth != null && !string.IsNullOrWhiteSpace(maxLenth.Value))
                {
                    return new MaxStringLength
                               {
                                 MaxLength = Convert.ToInt32(maxLenth.Value)
                               };
                }
            }
            return new MaxStringLength();
        }
    }

    public class DecimalPrecision : ISchemaValidation
    {
        public int? Precision { get; set; }
        public int? Scale { get; set; }
        public string Maximum { get; set; }
        public string Minimum { get; set; }

        public override string ToString()
        {
            return string.Format("({0}, {1})", this.Precision, this.Scale);
        }


        public static DecimalPrecision GetDecimalPrecision(XmlSchemaSimpleType schemaType)
        {
            if (schemaType.Name == "Currency")
            {
                return new DecimalPrecision
                    {
                        // If anything, we should verify decimal precision based on Precision and Scale derived 
                        // from database schema, not against database-vendor-specific data types ranges.  
                        // While SQL Server only supports the values listed below for the "Currency" data type, 
                        // this should not be the basis for the range verification in the XSD parsing.  
                        // Defaulting this range artificially is not appropriate.
                        //
                        // In reality, the discrepancy between the Precision/Scale approach and the "money" data
                        // type in SQL Server is irrelevant because this does not represent a range of values that
                        // is useful in the education domain. 
                        // 
                        // However, if supplied by the caller, they will result in SQL Server errors upon saving.
                        Maximum = "999999999999999.9999", // Matching decimal(19, 4)
                        Minimum = "-999999999999999.9999"
                               
                        // NOTE: Original values matched SQL Server money data type range, but this is not appropriate (see above).
                        //Maximum = "922337203685477.5807",
                        //Minimum = "-922337203685477.5808"
                    };
            }
            var restriction = schemaType.Content as XmlSchemaSimpleTypeRestriction;
            var result = new DecimalPrecision();

            foreach (var facet in restriction.Facets)
            {
                var totalDigits = facet as XmlSchemaTotalDigitsFacet;
                if (totalDigits != null && !string.IsNullOrWhiteSpace(totalDigits.Value))
                    result.Precision = Convert.ToInt32(totalDigits.Value);

                var fractionDigits = facet as XmlSchemaFractionDigitsFacet;
                if (fractionDigits != null && !string.IsNullOrWhiteSpace(fractionDigits.Value))
                    result.Scale = Convert.ToInt32(fractionDigits.Value);
            }

            if (result.Precision.HasValue && result.Scale.HasValue)
            {
                var range = GetRange(result.Precision.Value, result.Scale.Value);
                result.Maximum = range;
                result.Minimum = "-" + range;
            }


            return result;
        }

        private static string GetRange(int precision, int scale)
        {   
            return (string.Format("{0}.{1}", new string('9', precision - scale), new string('9', scale)));
        }
    }
}