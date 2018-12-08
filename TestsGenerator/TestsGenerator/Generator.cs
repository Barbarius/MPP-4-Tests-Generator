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
        private readonly FileReader fileReader;

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

            var readerTransformBlock = new TransformBlock<string, Task<string>>(readPath => fileReader.ReadFileAsync(readPath), readerOptions);

            readerTransformBlock.Complete();
        }

        public Generator(int readerFilesCount, int writerFilesCount)
        {
            this.readerFilesCount = readerFilesCount;
            this.writerFilesCount = writerFilesCount;

            fileReader = new FileReader();
        }
    }
}
