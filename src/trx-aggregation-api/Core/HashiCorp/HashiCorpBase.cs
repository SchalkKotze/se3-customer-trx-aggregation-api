using System.Diagnostics.CodeAnalysis;

namespace aggregate_api.Core.HashiCorp;

[ExcludeFromCodeCoverage]
public static class HashiCorpBase
{
    public static void LoadSecrets(string filePath)
    {
        if (!File.Exists(filePath)) return;
        
        using var streamReader = new StreamReader(filePath);
            
        while (!streamReader.EndOfStream)
        {
            var line = streamReader.ReadLine();

            if (string.IsNullOrEmpty(line) || line.Length < 2) continue;
                    
            var values = line.Split(',');
                        
            Environment.SetEnvironmentVariable(values[0].Replace("\"", "").Trim(), values[1].Replace("\"", "").Trim());
        }
    }
}