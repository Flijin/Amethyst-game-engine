using Game_Engine.Core;
using System.Reflection;
using System.Resources;
using System.Resources.Extensions;

namespace UnitTests
{
    [TestClass]
    public class JSONSerializer_Tests
    {
        [TestMethod]
        public void CorrectJSON()
        {
            string dataset = Resources.Correct_file;
            JSONSerializer.JSONToObj(dataset);
        }

        [TestMethod]
        public void IncorrectJSON()
        {
            for (int i = 1; i <= 24; i++)
            {
                try
                {
                    JSONSerializer.JSONToObj(Resources.ResourceManager.GetString($"Incorrect file case {i}")!);
                }
                catch (ArgumentException)
                {
                    continue;
                }
                catch (Exception)
                {
                    Assert.Fail($"The file {i} caused an incorrect exception");
                }

                Assert.Fail($"The file {i} was able to be parsed");
            }
        }
    }
}