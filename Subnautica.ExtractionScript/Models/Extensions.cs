using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Subnautica.ExtractionScript.Models
{
    internal static class Extensions
    {

        public static void UpdateTable(this StringBuilder stringBuilder, string table, string column, object value, int? id)
        {
            if (value is string)
            {
                value = $"'{value}'";
            }

            if (id == null)
            {
                stringBuilder.Append($"UPDATE {table} SET {column} = {value};");
            }
            else
            {
                stringBuilder.Append($"UPDATE {table} SET {column} = {value} WHERE Id = {id};");
            }


        }

    }
}
