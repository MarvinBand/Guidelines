using System;
using Xunit.Abstractions;

namespace Ef.LazyLoading.Tests.Logging
{
    public interface IWriter
    {
        void WriteLine(string str);
    }

    public class LoggingTestBase : IWriter
    {
        public ITestOutputHelper Output { get; }

        public LoggingTestBase(ITestOutputHelper output)
        {
            Output = output;
        }

        public void WriteLine(string str)
        {
            Output.WriteLine(str ?? Environment.NewLine);
        }
    }

}