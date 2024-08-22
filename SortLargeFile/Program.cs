using System.Diagnostics;

namespace SortLargeFile;

public static class GenTest
{
    public static void GenDataTest(string filePath, int size)
    {
        Random random = new Random();
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            for (int i = 0; i < size; ++i)
            {
                writer.WriteLine(random.Next());
            }
        }
    }
}

public class QuickSort
{
    private static int Partition(List<int> array, int low, int high)
    {
        int pivot = array[high];
        int lowIndex = (low - 1);
        for (int j = low; j < high; j++)
        {
            if (array[j] <= pivot)
            {
                ++lowIndex;
                int temp = array[lowIndex];
                array[lowIndex] = array[j];
                array[j] = temp;
            }
        }
        int temp1 = array[lowIndex + 1];
        array[lowIndex + 1] = array[high];
        array[high] = temp1;
        return lowIndex + 1;
    }
    private static void SortRecursive(List<int> array, int low, int high)
    {
        if (low < high)
        {
            int partitionIndex = Partition(array, low, high);
            SortRecursive(array, low, partitionIndex - 1);
            SortRecursive(array, partitionIndex + 1, high);
        }
    }
    public static void Sort(List<int> array)
    {
        QuickSort.SortRecursive(array, 0, array.Count - 1);
    }
}


public static class FileUtils
{
    public static List<string> SplitFile(string originalFilePath, int numberOfFiles, string outputDir)
    {
        List<string> smallFiles = new List<string>(numberOfFiles);
        using (StreamReader reader = new StreamReader(originalFilePath))
        {
            List<StreamWriter> writers = new List<StreamWriter>();
            // Create StreamWriters for each small file
            for (int i = 0; i < numberOfFiles; i++)
            {
                string outputPath = Path.Combine(outputDir, $"file_{i}.txt");
                smallFiles.Add(outputPath);
                writers.Add(new StreamWriter(outputPath));
            }
            
            int currentFileIndex = 0;
            string line;

            while ((line = reader.ReadLine()) != null)
            {
                writers[currentFileIndex].WriteLine(line);
                currentFileIndex = (currentFileIndex + 1) % numberOfFiles;
            }

            // Close all StreamWriters
            foreach (var writer in writers)
            {
                writer.Close();
            } 
        }

        return smallFiles;
    }
    
    public static void SortFile(string filePath)
    {
        List<int> list = ExtractFile(filePath);
        QuickSort.Sort(list);
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            foreach (var item in list)
            {
                writer.WriteLine(item);
            }
        }
    }
    public static int GetNumFromReader(StreamReader reader)
    {
        string line = reader.ReadLine();
        if (line == null)
        {
            return int.MaxValue;
        }
        return int.Parse(line);
    }
    private static List<int> ExtractFile(string filePath)
    {
        List<int> list = new List<int>();
        using (StreamReader reader = File.OpenText(filePath))
        {
            while (true)
            {
                int num = GetNumFromReader(reader);
                if (num == int.MaxValue)
                {
                    break;
                }
                list.Add(num);
            }
        }
        
        return list;
    }
}
class Program
{
    private const string originalFilePath = @"F:\TestExternalSort\RawData.txt";
    private const int originalFileLength = 1000000000;
    private const string tempFolder = @"F:\TestExternalSort\Temp";
    private const string outFile = @"F:\TestExternalSort\SortedData.txt";
    private const int numberOfFiles = 20;

    static void Main(string[] args)
    {
        Stopwatch stopwatch = new Stopwatch();
        // start 
        stopwatch.Start();
        
        // gen data test: about 10GB data
        Console.WriteLine("Generating Test...");
        GenTest.GenDataTest(originalFilePath, originalFileLength);
        Console.WriteLine($"Time Elapsed {stopwatch.Elapsed}");
        
        Console.WriteLine();
        
        // split the raw file into 20 temp file
        Console.WriteLine("Splitting Raw File");
        List<string> files = FileUtils.SplitFile(originalFilePath, numberOfFiles, tempFolder);
        Console.WriteLine("Split File Successfully.");
        Console.WriteLine($"Time Elapsed {stopwatch.Elapsed}");
        
        Console.WriteLine();
        
        // sort all 20 temp files
        files.ForEach(FileUtils.SortFile);
        Console.WriteLine("Sort All Temp Files Successfully.");
        Console.WriteLine($"Time Elapsed {stopwatch.Elapsed}");
        
        Console.WriteLine();
        
        // merge 20 temp files into sorted large file
        Console.WriteLine("Merge temp files.");
        List<StreamReader> readers = new List<StreamReader>(numberOfFiles);
        List<int> minVals = new List<int>(numberOfFiles);
        files.ForEach(file =>
        {
            StreamReader reader = File.OpenText(file);
            readers.Add(reader);
            minVals.Add(FileUtils.GetNumFromReader(reader));
        });
        using (StreamWriter writer = new StreamWriter(outFile))
        {
            while (true)
            {
                int curMin = minVals.Min();
                if (curMin == int.MaxValue)
                {
                    break;
                }
                writer.WriteLine(curMin);
                int index = minVals.IndexOf(curMin);
                minVals[index] = FileUtils.GetNumFromReader(readers[index]);
            }
        }
        
        readers.ForEach(reader => { reader.Close(); });
        
        Console.WriteLine("Merge All Temp Files Successfully.");
        Console.WriteLine($"Time Elapsed {stopwatch.Elapsed}");
        
        Console.WriteLine();
        
        // clean temp folder
        Console.WriteLine("Cleaning Temp files");
        Directory.Delete(tempFolder, true);
        Console.WriteLine($"Time Elapsed {stopwatch.Elapsed}");
        
        Console.WriteLine();
        
        Console.WriteLine($"Time Elapsed {stopwatch.Elapsed}");
        
        stopwatch.Stop();
    }
}