using Amethyst_game_engine.Core;
using System.Text;

namespace UnitTests;

[TestClass]
public class JSONSerializer_Tests
{
    [TestMethod]
    public void CorrectJSON()
    {
        string dataset = Resources.Correct_file;
        JSONSerializer.JsonToObj(dataset);
        JSONSerializer.JsonToObj(dataset.ToCharArray());
        JSONSerializer.JsonToObj(Encoding.UTF8.GetBytes(dataset.ToCharArray()));
    }

    [TestMethod]
    public void IncorrectJSON()
    {
        for (int i = 1; i <= 24; i++)
        {
            try
            {
                JSONSerializer.JsonToObj(Resources.ResourceManager.GetString($"Incorrect file case {i}")!);
                JSONSerializer.JsonToObj(Resources.ResourceManager.GetString($"Incorrect file case {i}")!.ToCharArray());
                JSONSerializer.JsonToObj(Encoding.UTF8.GetBytes(Resources.ResourceManager.GetString($"Incorrect file case {i}")!.ToCharArray()));
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
