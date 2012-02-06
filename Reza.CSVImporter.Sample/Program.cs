using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;

namespace Reza.CSVImporter.Sample
{
    class Program
    {
        static void Main(string[] args)
        {
            var csvImporter = new CSVImporter<Customer>();
            var customers = csvImporter.Extract("..\\..\\sample.csv");
            foreach (var c in customers)
            {
                Console.WriteLine("Customer({0}) : {1} lives at {2}",c.Id,c.Name,c.Address);
            }
        }
    }
}
