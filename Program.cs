using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

var sw = Stopwatch.StartNew();
var lines = TraverseTree(args[0]);
sw.Stop();

Console.WriteLine("Total lines: {0}", lines);
Console.WriteLine("Total time elapsed: {0}s", sw.Elapsed.TotalSeconds);
Console.WriteLine("\r\nrPress any key to exit!");
Console.ReadKey();

/// <summary>
///     Applies a traversal looping through all the directories/files found in the specified directory.
/// </summary>
static long TraverseTree(string root)
{
    // Data structure to hold names of sub-folders to be examined for files.
    var dirs = new Stack<string>(20);
    var totalLines = 0L;

    if (!Directory.Exists(root))
    {
        throw new ArgumentException($"The specified directory '{root}' doesn't exist.");
    }

    dirs.Push(root);

    while (dirs.Count > 0)
    {
        var currentDir = dirs.Pop();
        var totalElapsed = TimeSpan.FromSeconds(0);

        Console.WriteLine("Searching in '{0}':", currentDir);

        // Discover the sub-directories in the current directory..
        string[] subDirs;
        try
        {
            subDirs = Directory.GetDirectories(currentDir);
        }
        catch (UnauthorizedAccessException e)
        {
            Console.WriteLine(e.Message);
            continue;
        }
        catch (DirectoryNotFoundException e)
        {
            Console.WriteLine(e.Message);
            continue;
        }

        // Discover the files in the current directory..
        string[] files;
        try
        {
            files = Directory.GetFiles(currentDir);
        }
        catch (UnauthorizedAccessException e)
        {
            Console.WriteLine(e.Message);
            continue;
        }
        catch (DirectoryNotFoundException e)
        {
            Console.WriteLine(e.Message);
            continue;
        }

        // Apply the necessary operations of the found files..
        foreach (var file in files)
        {
            try
            {
                var fi = new FileInfo(file);
                var sw = Stopwatch.StartNew();
                var fs = File.Open(fi.FullName, FileMode.Open);

                var lines = CountLines(fs);

                sw.Stop();

                Console.WriteLine("{0} | {1} | {2} lines | {3}ms", fi.Name, fi.Length, lines, sw.ElapsedMilliseconds);
                
                totalElapsed += sw.Elapsed;
                totalLines += lines;
                sw.Reset();
            }
            catch (FileNotFoundException e)
            {
                // If file was deleted by a separate application or thread
                // since the call to TraverseTree() then just continue.
                Console.WriteLine(e.Message);
                continue;
            }
        }

        Console.WriteLine();
        Console.WriteLine("Time elapsed: {0}ms", totalElapsed.TotalMilliseconds);
        Console.WriteLine();

        // Push the sub-directories onto the stack fro traversal.
        foreach (var dir in subDirs)
            dirs.Push(dir);
    }
    return totalLines;
}

/// <summary>
///     Counts and returns the number of lines if the specified stream.
/// </summary>
static long CountLines(Stream stream)
{
    if (stream is null)
        throw new ArgumentNullException(nameof(stream));

    var lineCount = 0L;

    const int BytesAtTheTime = 4;
    const char CR = '\r';
    const char LF = '\n';
    const char NULL = (char)0;


var byteBuffer = new byte[1024 * 1024];
    char detectedEOL = NULL;
    char currentChar = NULL;

    int bytesRead;
    while ((bytesRead = stream.Read(byteBuffer, 0, byteBuffer.Length)) > 0)
    {
        var i = 0;
        for (; i <= bytesRead - BytesAtTheTime; i += BytesAtTheTime)
        {
            currentChar = (char)byteBuffer[i];

            if (detectedEOL != NULL)
            {
                if (currentChar == detectedEOL) { lineCount++; }

                currentChar = (char)byteBuffer[i + 1];
                if (currentChar == detectedEOL) { lineCount++; }

                currentChar = (char)byteBuffer[i + 2];
                if (currentChar == detectedEOL) { lineCount++; }

                currentChar = (char)byteBuffer[i + 3];
                if (currentChar == detectedEOL) { lineCount++; }
            }
            else
            {
                if (currentChar == LF || currentChar == CR)
                {
                    detectedEOL = currentChar;
                    lineCount++;
                }
                i -= BytesAtTheTime - 1;
            }
        }

        for (; i < bytesRead; i++)
        {
            currentChar = (char)byteBuffer[i];

            if (detectedEOL != NULL)
            {
                if (currentChar == detectedEOL) { lineCount++; }
            }
            else
            {
                if (currentChar == LF || currentChar == CR)
                {
                    detectedEOL = currentChar;
                    lineCount++;
                }
            }
        }
    }

    if (currentChar != LF && currentChar != CR && currentChar != NULL)
    {
        lineCount++;
    }
    return lineCount;
}