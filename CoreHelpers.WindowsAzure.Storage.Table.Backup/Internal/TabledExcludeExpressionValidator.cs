using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace CoreHelpers.WindowsAzure.Storage.Table.Backup.Internal;

internal class TabledExcludeExpressionValidator
{
    private List<string> _excludes = new List<string>();
    public TabledExcludeExpressionValidator(string[] excludes)
    {
        if (excludes != null)
            _excludes = excludes.Select(e => e.ToLower()).ToList();
    }

    public bool IsTableExcluded(string tableName)
    {
        // verify if the tablename is in the exclude list
        if (_excludes.Contains(tableName.ToLower()))
            return true;
        
        // verify if the tablename matches a regex pattern
        foreach (var exclude in _excludes)
        { 
            if (Regex.IsMatch(tableName, exclude, RegexOptions.IgnoreCase)) 
                return true;
        }
        
        // all the rest don't exclude
        return false;
    } 
}