using System;
using System.IO;
using System.Messaging;
using System.Text;
using Newtonsoft.Json;

namespace EdFi.Messaging.MSMQ
{
    /// <summary>
    /// This code was borrowed from the following gist: https://gist.github.com/jchadwick/2430984
    /// </summary>
    public class JSonMessageFormatter : IMessageFormatter
    {
        private static readonly JsonSerializerSettings DefaultSerializerSettings =
            new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects
            };

        private readonly JsonSerializerSettings _serializerSettings;


        public JSonMessageFormatter(Encoding encoding = null)
            : this(encoding, null)
        {
        }

        internal JSonMessageFormatter(Encoding encoding, JsonSerializerSettings serializerSettings = null)
        {
            Encoding = encoding ?? Encoding.UTF8;
            _serializerSettings = serializerSettings ?? DefaultSerializerSettings;
        }

        public Encoding Encoding { get; set; }


        public bool CanRead(Message message)
        {
            if (message == null)
                throw new ArgumentNullException("message");

            Stream stream = message.BodyStream;

            return stream != null
                   && stream.CanRead
                   && stream.Length > 0;
        }

        public object Clone()
        {
            return new JSonMessageFormatter(Encoding, _serializerSettings);
        }

        public object Read(Message message)
        {
            if (message == null)
                throw new ArgumentNullException("message");

            if (CanRead(message) == false)
                return null;

            using (var reader = new StreamReader(message.BodyStream, Encoding))
            {
                string json = reader.ReadToEnd();
                return JsonConvert.DeserializeObject(json, _serializerSettings);
            }
        }

        public void Write(Message message, object obj)
        {
            if (message == null)
                throw new ArgumentNullException("message");

            if (obj == null)
                throw new ArgumentNullException("obj");

            string json = JsonConvert.SerializeObject(obj, Formatting.None, _serializerSettings);

            message.BodyStream = new MemoryStream(Encoding.GetBytes(json));

            //Need to reset the body type, in case the same message
            //is reused by some other formatter.
            message.BodyType = 0;
        }
    }
}