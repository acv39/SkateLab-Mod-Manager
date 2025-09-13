using System;
using System.Diagnostics;
using System.IO;

 public class PakExtractor
 {
     private readonly string unrealPakPath;

     public PakExtractor()
     {
         // UnrealPak.exe should be in the same folder as your app
         unrealPakPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "UnrealPak.exe");
         if (!File.Exists(unrealPakPath))
             throw new FileNotFoundException("UnrealPak.exe not found in application directory.");
     }

     public void ExtractModFiles(string pakPath, string outputDir)
     {
         if (!File.Exists(pakPath))
             throw new FileNotFoundException($"Pak file not found: {pakPath}");

         Directory.CreateDirectory(outputDir);

         // Step 1: List all files inside the pak
         string pakList = RunProcess(unrealPakPath, $"\"{pakPath}\" -List");

         // Step 2: Search for modinfo + modimg
         foreach (var line in pakList.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
         {
             string lowerLine = line.ToLowerInvariant();

             if (lowerLine.Contains("modinfo.txt") ||
                 lowerLine.Contains("modimg.png") ||
                 lowerLine.Contains("modimg.jpg") ||
                 lowerLine.Contains("modimg.jpeg"))
             {
                 string filepath = line.Split(' ')[0].Trim(); // path is the first token

                 Console.WriteLine($"[PakExtractor] Extracting {filepath}");

                 RunProcess(unrealPakPath,
                     $"\"{pakPath}\" -Extract \"{outputDir}\" -Filter=\"*{filepath}\"");
             }
         }
     }

     private string RunProcess(string exePath, string arguments)
     {
         var process = new Process
         {
             StartInfo = new ProcessStartInfo
             {
                 FileName = exePath,
                 Arguments = arguments,
                 RedirectStandardOutput = true,
                 RedirectStandardError = true,
                 UseShellExecute = false,
                 CreateNoWindow = true
             }
         };

         process.Start();
         string output = process.StandardOutput.ReadToEnd();
         string error = process.StandardError.ReadToEnd();
         process.WaitForExit();

         if (!string.IsNullOrEmpty(error))
             Console.WriteLine($"[PakExtractor] Error: {error}");

         return output;
     }
 }
//thanks chatgpt im lazy