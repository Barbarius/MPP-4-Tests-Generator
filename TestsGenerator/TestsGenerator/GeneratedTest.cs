using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestsGenerator
{
    class GeneratedTest
    {
        private string name;
        private string text;

        public string Name
        {
            get { return name; }
        }

        public string Text
        {
            get { return text; }
        }

        public GeneratedTest(string name, string text)
        {
            this.name = name;
            this.text = text;
        }
    }
}
