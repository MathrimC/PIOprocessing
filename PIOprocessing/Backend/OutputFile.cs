using System.IO;

namespace PIOprocessing {
    class OutputFile
    {
        protected string filePath;
        protected StreamWriter sw;
        public OutputFile(string filePath) {
            this.filePath = filePath;
            sw = new StreamWriter(filePath);
        }

        public void writeCsvLine(string[] data) {
            sw.WriteLine(string.Join(",",data));
        }


    }
}