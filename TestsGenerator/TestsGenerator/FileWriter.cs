using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestsGenerator
{
    class FileWriter
    {
        public async void WriteFileAsync(Task<List<GeneratedTest>> generatedTests)
        {
            var resultTests = await generatedTests;
            foreach(var resultTest in resultTests)
            {
                using (StreamWriter sw = new StreamWriter(resultTest.Name))
                {
                    await sw.WriteAsync(resultTest.Text);
                }
            }
        }
    }
}
