using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.SmartStandards {

  [TestClass()]
  public class ArgsResolvingExtensionsTests {

    [TestMethod()]
    public void TryResolveArgs_VariousTestPatterns_CreateExpectedStrings() {

      string template = "Hello {audience}, the answer is {answer}.";

      Assert.AreEqual("Hello World, the answer is 42.", template.TryResolveArgs(new object[] { "World", 42 }));

    }

  }
}
