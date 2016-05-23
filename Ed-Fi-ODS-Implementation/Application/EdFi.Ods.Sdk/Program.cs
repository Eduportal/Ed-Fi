using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdFi.Ods.Sdk
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(@"To generate the Ed-Fi ODS SDK, perform the following steps.
1. If prompted, restore the solution nuget packages, then reload the solution.
2. Ensure that you have set the SwaggerUI project as a startup project.
3. Select the Debug | Start Without Debugging menu item.
4. Wait until the SwaggerUI page loads in your browser.
5. Type the following command in the Package Manager Console (see View | Other Windows... | Package Manager Console):
        Generate-Sdk.ps1

Press any key to exit.
");

            Console.ReadKey();
        }
    }
}
