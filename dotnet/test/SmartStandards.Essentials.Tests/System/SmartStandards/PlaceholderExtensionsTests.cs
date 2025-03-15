using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.SmartStandards {

  [TestClass()]
  public class PlaceholderExtensionsTests {

    [TestMethod()]
    public void ResolvePlaceholders_VariousTestPatterns_CreateExpectedStrings() {

      string template = "Hello {audience}, the answer is {answer}.";

      Assert.AreEqual("Hello World, the answer is 42.", template.ResolvePlaceholders("World", 42));
      Assert.AreEqual("Hello World, the answer is 42.", template.ResolvePlaceholders(new object[] { "World", 42 }));

    }

    [TestMethod()]
    public void ResolvePlaceholdersByPropertyBag_VariousTestPatterns_ReturnExpectedStuff() {

      var propertyBag1 = new { Foo = "FooValue", bar = "barValue", Batz = "BatzValue" };

      Assert.AreEqual("", "".ResolvePlaceholdersByPropertyBag(propertyBag1));
      Assert.AreEqual("Hello World.", "Hello World.".ResolvePlaceholdersByPropertyBag(propertyBag1));
      Assert.AreEqual("FooValue", "{Foo}".ResolvePlaceholdersByPropertyBag(propertyBag1));
      Assert.AreEqual("Hello FooValue barValue World.", "Hello {Foo} {Bar} World.".ResolvePlaceholdersByPropertyBag(propertyBag1));

      // Missing values & malformed

      Assert.AreEqual("{Original} string.", "{Original} string.".ResolvePlaceholdersByPropertyBag(null));
     
      Assert.AreEqual(
        "Hello FooValue barValue {NotExisting} World.",
        "Hello {Foo} {Bar} {NotExisting} World.".ResolvePlaceholdersByPropertyBag(propertyBag1)
      );
      
      Assert.AreEqual("{}", "{}".ResolvePlaceholdersByPropertyBag(propertyBag1));
      
      Assert.AreEqual("{} FooValue", "{} {Foo}".ResolvePlaceholdersByPropertyBag(propertyBag1));
      
      Assert.AreEqual("{Foo {bar}", "{Foo {bar}".ResolvePlaceholdersByPropertyBag(propertyBag1));

    }

  }
}
