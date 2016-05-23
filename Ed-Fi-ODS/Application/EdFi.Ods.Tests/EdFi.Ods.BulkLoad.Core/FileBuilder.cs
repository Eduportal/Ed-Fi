namespace EdFi.Ods.Tests.EdFi.Ods.BulkLoad.Core
{
    using System;
    using System.IO;

    public static class FileBuilder
    {
        private static readonly string Filename;

        static FileBuilder()
        {
            Filename = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + "_Test.xml");
        }

        public static string Create()
        {
            using (var file = File.CreateText(Filename))
            {
                file.WriteLine(@"<?xml version=""1.0"" encoding=""utf-8"" ?>");
                file.WriteLine(@"
                    <rootNode>
                        <firstLevelNode id=""123"">
                            <secondLevelNodeElement>1</secondLevelNodeElement>
                            <secondLevelNodeElement2>2</secondLevelNodeElement2>
                      </firstLevelNode>
                    </rootNode>");
            }

            return Filename;
        }

        public static void Delete()
        {
            if (File.Exists(Filename))
                File.Delete(Filename);
        }
    }
}