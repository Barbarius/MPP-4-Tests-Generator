using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace TestsGenerator
{
    public class Generator
    {
        private readonly int readerFilesCount;
        private readonly int writerFilesCount;
        private readonly int maxTasksCount;
        private readonly FileReader fileReader;
        private readonly FileWriter fileWriter;

        public Task Generate(List<string> paths)
        {
            ExecutionDataflowBlockOptions readerOptions = new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = readerFilesCount
            };
            ExecutionDataflowBlockOptions writerOptions = new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = writerFilesCount
            };
            ExecutionDataflowBlockOptions maxTasksOptions = new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = maxTasksCount
            };

            var readerTransformBlock = new TransformBlock<string, Task<string>>(readPath => fileReader.ReadFileAsync(readPath), readerOptions);

            //var writerTransformBlock = new TransformBlock<string, Task<string>>();

            var generatorTransformBlock = new TransformBlock<Task<string>, Task<List<GeneratedTest>>>(
                new Func<Task<string>, Task<List<GeneratedTest>>>(GenerateFileAsync), maxTasksOptions);



            readerTransformBlock.Complete();
        }

        private async Task<List<GeneratedTest>> GenerateFileAsync(Task<string> readSourseFile)
        {
            string sourceFile = await readSourseFile;
            var result = new List<GeneratedTest>();

            //

            return result;
        }

        public Generator(int readerFilesCount, int writerFilesCount, int maxTasksCount)
        {
            this.readerFilesCount = readerFilesCount;
            this.writerFilesCount = writerFilesCount;
            this.maxTasksCount = maxTasksCount;

            fileReader = new FileReader();
            fileWriter = new FileWriter();
        }
    }
}
