using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EdFi.Ods.Swagger.Models
{

    //Swagger Resource declaration's Model is created dynamically using IList, but swagger does not need it to be output as json array, but rather
    //as named properties of the model object, so need this custom conversion
    public class CustomConverterForSwaggerModels : JsonConverter
    {
        private string AddQuotedText(string input)
        {
            return "\"" + input + "\"";
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            
            var model = value as ModelDictionarySpec;
            writer.WriteStartObject();
            foreach (var o in model)
            {
                writer.WritePropertyName(o.Key);
                writer.WriteStartObject();
                writer.WritePropertyName("id");
                writer.WriteValue(o.Value.ID);
                writer.WritePropertyName("properties");

                var json = new StringBuilder();
                json.Append("{" + Environment.NewLine);
                var propertySeparator = string.Empty;
                foreach (var p in o.Value.Properties)
                {
                    json.AppendLine(new String(' ',8) + propertySeparator + AddQuotedText(p.Key) + ": {");
                    if (p.Value.Type != null) json.Append(new String(' ', 10) + AddQuotedText("type") + ": " + AddQuotedText(p.Value.Type) + "," + Environment.NewLine);
                    json.Append(new String(' ', 10) + AddQuotedText("required") + ": " + p.Value.Required.ToString().ToLower());
                    if (p.Value.Description != null) json.Append("," + Environment.NewLine + new String(' ', 10) + AddQuotedText("description") + ": " + AddQuotedText(p.Value.Description) + Environment.NewLine);
                    if (p.Value.Items != null) json.Append("," + Environment.NewLine + new String(' ', 10) + AddQuotedText("items") + ":{" + AddQuotedText("$ref") + ": " + AddQuotedText(p.Value.Items.Ref) + "}" + Environment.NewLine);
                    else json.Append(Environment.NewLine);
                    json.Append(new String(' ', 8) + "}" + Environment.NewLine);
                    propertySeparator = ", ";
                }
                json.Append(new String(' ', 6) + "}");
                writer.WriteRawValue(json.ToString());
                writer.WriteEnd();
            }
            writer.WriteEnd();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType.Name.Equals("ModelDictionarySpec");
        }
    }
}
