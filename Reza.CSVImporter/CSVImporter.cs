using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Collections.Specialized;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Reza.CSVImporter
{



    public partial class CSVImporter<T> where T : class
    {
        private const string Delimiter = ",";
        private readonly string[] lineDelimiters = new[] { "\r\n", "\n" };
        /// <summary>
        /// Extracts objects from file in the specified local path.
        /// </summary>
        /// <param name="localPath">The local path.</param>
        /// <param name="columnMappers">The column mappers.</param>
        /// <param name="convertors">The convertors.</param>
        /// <returns></returns>
        public IEnumerable<T> Extract(string localPath, NameValueCollection columnMappers, IDictionary<string, Expression<Func<object, object>>> convertors, int skip)
        {

            return Extract(localPath, columnMappers, convertors, "", skip);
        }

        public IEnumerable<T> Extract(string localPath)
        {

            return Extract(localPath, null, null, "", 0);
        }
        public IEnumerable<T> Extract(string localPath, NameValueCollection columnMappers, IDictionary<string, Expression<Func<object, object>>> convertors, string columnRow, int skip)
        {

            var csvContent = GetCSVContent(localPath);
            return ExtractData(csvContent, columnMappers, convertors, columnRow, skip);
        }


        /// <summary>
        /// Extracts the data.
        /// </summary>
        /// <param name="csvContent">Content of the CSV.</param>
        /// <param name="columnMappers">The column mappers.</param>
        /// <param name="convertors">The convertors.</param>
        /// <returns></returns>
        protected IEnumerable<T> ExtractData(IEnumerable<string> csvContent, NameValueCollection columnMappers, IDictionary<string, Expression<Func<object, object>>> convertors, string columnRow, int skip)
        {
            var startRow = String.IsNullOrEmpty(columnRow) ? 1 + skip : skip;
            if (String.IsNullOrEmpty(columnRow))
                columnRow = csvContent.First();
            var csvColumns = columnRow.Split(new string[] { Delimiter }, StringSplitOptions.RemoveEmptyEntries);
            csvColumns = csvColumns.Select(c => c.Trim()).ToArray();
            var csvData = csvContent.Skip(startRow).Take(csvContent.Count());
            foreach (var item in csvData)
            {
                var mappedData = MapData(item, columnMappers, csvColumns, convertors);
                if (mappedData != null)
                    yield return mappedData;
            }
        }
        /// <summary>
        /// Extracts the specified data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="columnMappers">The column mappers.</param>
        /// <param name="convertors">The convertors.</param>
        /// <returns></returns>
        public IEnumerable<T> Extract(byte[] data, NameValueCollection columnMappers, IDictionary<string, Expression<Func<object, object>>> convertors, string columnRow, int skip)
        {
            var memoryStream = new System.IO.MemoryStream(data);
            var reader = new StreamReader(memoryStream);
            return ExtractData(GetProperData(reader.ReadToEnd()), columnMappers, convertors, columnRow, skip);
        }

        /// <summary>
        /// Maps the data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="columnMappers">The column mappers.</param>
        /// <param name="csvColumns">The CSV columns.</param>
        /// <param name="convertors">The convertors.</param>
        /// <returns></returns>
        private T MapData(string data, NameValueCollection columnMappers, IEnumerable<string> csvColumns, IDictionary<string, Expression<Func<object, object>>> convertors)
        {
            var obj = Activator.CreateInstance<T>();
            //get read if stupid commas inside a cell
            Regex r = new Regex("\"(?<InQuote>[^\"]*)\"");
            var matches = r.Matches(data);

            for (int i = 0; i < matches.Count; i++)
            {
                var item = matches[i];
                if (item.Success)
                {


                    data = data.Replace(item.Value, item.Value.Replace(",", "-"));
                }
            }
            var csvValues = data.Split(new string[] { Delimiter }, StringSplitOptions.None);
            if (!csvValues.Any())
                return null;
            if (columnMappers != null && columnMappers.Count > 0)
                SetObjectValue(columnMappers, csvColumns, convertors, csvValues, obj);
            else
                SetObjectValue(csvColumns, convertors, csvValues, obj);
            return obj;

        }
        private static void SetObjectValue(IEnumerable<string> csvColumns, IDictionary<string, Expression<Func<object, object>>> convertors,
                                          string[] csvValues, T obj)
        {
            foreach (var item in csvColumns)
            {
                var index = csvColumns.IndexOf(item);
                var property = typeof(T).GetProperty(item);
                SetPropertyValue(convertors, csvValues, obj, property, item, index);
            }
        }

        private static void SetPropertyValue(IDictionary<string, Expression<Func<object, object>>> convertors, string[] csvValues, T obj, PropertyInfo property,
                                             string item, int index)
        {
            var value = csvValues.GetValue(index);
            if (convertors != null && convertors.ContainsKey(item))
                value = convertors[item].Compile().Invoke(value);

            property.SetValue(obj, Convert.ChangeType(value, property.PropertyType), new object[0]);
        }

        private static void SetObjectValue(NameValueCollection columnMappers, IEnumerable<string> csvColumns, IDictionary<string, Expression<Func<object, object>>> convertors,
                                           string[] csvValues, T obj)
        {
            foreach (var item in columnMappers.AllKeys)
            {
                var index = csvColumns.IndexOf(columnMappers[item].Trim().ToLower());
                var property = typeof(T).GetProperty(item);
                SetPropertyValue(convertors, csvValues, obj, property, item, index);
            }
        }

        /// <summary>
        /// Gets the content of the CSV.
        /// </summary>
        /// <param name="localPath">The local path.</param>
        /// <returns></returns>
        private IEnumerable<string> GetCSVContent(string localPath)
        {
            var content = "";
            using (var reader = File.OpenText(localPath))
            {
                content = reader.ReadToEnd();
            }
            return GetProperData(content);
        }
        /// <summary>
        /// Gets the proper data.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <returns></returns>
        private IEnumerable<string> GetProperData(string content)
        {

            return content.Split(lineDelimiters, StringSplitOptions.RemoveEmptyEntries);
        }

    }
}
