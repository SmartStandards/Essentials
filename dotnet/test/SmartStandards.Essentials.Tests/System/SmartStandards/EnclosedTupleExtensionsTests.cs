using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Text;

namespace System.SmartStandards {

  [TestClass()]
  public class EnclosedTupleExtensionsTests {

    [TestMethod()]
    public void IndexOfTupleElement_VariousTestPatterns_CreateExpectedValues() {

      String nullString = null;

      Assert.AreEqual(-2, "#".IndexOfTupleElement("(egal)"));
      Assert.AreEqual(-2, "a#".IndexOfTupleElement("(egal)"));
      Assert.AreEqual(-2, "#a".IndexOfTupleElement("(egal)"));
      Assert.AreEqual(-2, @"#a\#\".IndexOfTupleElement("(egal)"));

      Assert.AreEqual(-1, nullString.IndexOfTupleElement(null));
      Assert.AreEqual(-1, nullString.IndexOfTupleElement(""));
      Assert.AreEqual(-1, nullString.IndexOfTupleElement("DoesNotExist"));

      Assert.AreEqual(-1, "".IndexOfTupleElement(null));
      Assert.AreEqual(-1, "".IndexOfTupleElement(""));
      Assert.AreEqual(-1, "".IndexOfTupleElement("DoesNotExist"));

      Assert.AreEqual(-1, "##".IndexOfTupleElement(null));
      Assert.AreEqual(0, "##".IndexOfTupleElement(""));
      Assert.AreEqual(1, "#a##".IndexOfTupleElement(""));
      Assert.AreEqual(2, "#a#b##".IndexOfTupleElement(""));
      Assert.AreEqual(-1, "##".IndexOfTupleElement("DoesNotExist"));

      Assert.AreEqual(0, "#hello#".IndexOfTupleElement("hello"));
      Assert.AreEqual(1, "##hello#".IndexOfTupleElement("hello"));
      Assert.AreEqual(-1, "#helloo#".IndexOfTupleElement("hello"));
      Assert.AreEqual(-1, "#hell#".IndexOfTupleElement("hello"));

      Assert.AreEqual(-1, "#a#hello#".IndexOfTupleElement("hello#"));
      Assert.AreEqual(1, @"#a#hello\#\#".IndexOfTupleElement("hello#"));

      Assert.AreEqual(1, @"#a#\0#b#".IndexOfTupleElement(null));
      Assert.AreEqual(-1, @"#a#\0#b#".IndexOfTupleElement(""));

      Assert.AreEqual(0, @"#Mambo\#\Five#HeyJude#".IndexOfTupleElement("Mambo#Five"));
      Assert.AreEqual(0, @"#Mambo\#\#".IndexOfTupleElement("Mambo#"));

    }

    [TestMethod()]
    public void AppendManyToEnclosedTuple_VariousTestPatterns_CreateExpectedStrings() {

      long[] longValues = { 2083349424987289070, 2083349429634175975, 2083349433585439538 };

      Assert.AreEqual(
        "#2083349424987289070#2083349429634175975#2083349433585439538#",
        new StringBuilder().AppendManyToEnclosedTuple(longValues).ToString()
      );

      List<DateTime> dateValues = new List<DateTime> { new DateTime(1973, 12, 9), new DateTime(2000, 1, 1), new DateTime(2022, 2, 22) };

      Assert.AreEqual(
        "#1973-12-09#2000-01-01#2022-02-22#",
        new StringBuilder().AppendManyToEnclosedTuple(dateValues, (d) => ((DateTime)d).ToString("yyyy-MM-dd")).ToString()
      );

    }

    [TestMethod()]
    public void AppendToEnclosedTuple_VariousTestPatterns_CreateExpectedStrings() {

      Assert.AreEqual("#Foo#", new StringBuilder().AppendToEnclosedTuple("Foo").ToString());
      Assert.AreEqual("#Foo#Bar#", new StringBuilder().AppendToEnclosedTuple("Foo").AppendToEnclosedTuple("Bar").ToString());
      Assert.AreEqual("#Foo#Bar#Batz#", new StringBuilder().AppendToEnclosedTuple("Foo").AppendToEnclosedTuple("Bar").AppendToEnclosedTuple("Batz").ToString());

      Assert.AreEqual("##", new StringBuilder().AppendToEnclosedTuple("").ToString());
      Assert.AreEqual(@"#\0#", new StringBuilder().AppendToEnclosedTuple(null).ToString());
      Assert.AreEqual(@"#Mambo\#\Five#Ba\\r#", new StringBuilder().AppendToEnclosedTuple("Mambo#Five").AppendToEnclosedTuple(@"Ba\r").ToString());

    }

    [TestMethod()]
    public void SplitEnclosedTuple_MalformedTestPatterns_CreateExpectedArrays() {

      // "#" => malformed => ???  

      String[] unclosedSeparator = @"#".SplitEnclosedTuple();
      Assert.IsNull(unclosedSeparator);

      // "#\0Foo#" => malformed ("#\0#" should stand alone) => Gracefully return the original string

      String[] malformedNullAtStart = @"#\0Foo#".SplitEnclosedTuple();

      Assert.AreEqual(1, malformedNullAtStart.Length);
      Assert.AreEqual(@"\0Foo", malformedNullAtStart[0]);

      // "#Foo\0#" => malformed ("#\0#" should stand alone) => Gracefully return the original string

      String[] malformedNullAtEnd = @"#Foo\0#".SplitEnclosedTuple();
      Assert.AreEqual(1, malformedNullAtEnd.Length);
      Assert.AreEqual(@"Foo\0", malformedNullAtEnd[0]);

      // "#Foo\0Foo#" => malformed ("#\0#" should stand alone) => Gracefully return the original string

      String[] malformedNullInbetween = @"#Foo\0Bar#".SplitEnclosedTuple();
      Assert.AreEqual(1, malformedNullInbetween.Length);
      Assert.AreEqual(@"Foo\0Bar", malformedNullInbetween[0]);

      // "#\#Foo#B\#ar#B\\atz\##" => escaping was not done symmetrically => ???

      String[] bEscapedMalformed = @"#\#Foo#B\#ar#B\\atz\##".SplitEnclosedTuple();

      Assert.AreEqual(3, bEscapedMalformed.Length);
      Assert.AreEqual("#Foo", bEscapedMalformed[0]);
      Assert.AreEqual("B#ar", bEscapedMalformed[1]);
      Assert.AreEqual(@"B\atz#", bEscapedMalformed[2]);

      // Having not escaped escape chars directly before the closing # => ???

      String[] escapedEscapeChars = @"#C:\\Temp\#D:\\Data\\Foo#".SplitEnclosedTuple();

    }

    [TestMethod()]
    public void SplitEnclosedTuple_ProperTestPatterns_CreateExpectedArrays() {

      // Tuple itself Is null

      String nullString = null;

      Assert.IsNull(nullString.SplitEnclosedTuple());

      // "\0" => null tuple itself

      Assert.IsNull(@"\0".SplitEnclosedTuple());

      // Empty string => empty collection

      String[] emptyCollection = "".SplitEnclosedTuple();

      Assert.AreEqual(-1, emptyCollection.GetUpperBound(0));

      // "#\0#" => collection containing one null element

      String[] onlyOneNull = @"#\0#".SplitEnclosedTuple();

      Assert.AreEqual(1, onlyOneNull.Length);
      Assert.IsNull(onlyOneNull[0]);

      // "##" => collection containing one empty string

      String[] onlyOneEmpty = "##".SplitEnclosedTuple();

      Assert.AreEqual(1, onlyOneEmpty.Length);
      Assert.AreEqual("", onlyOneEmpty[0]);

      // Collection contains one string

      String[] onlyOne = "#onlyOne#".SplitEnclosedTuple();

      Assert.AreEqual(0, onlyOne.GetUpperBound(0));
      Assert.AreEqual("onlyOne", onlyOne[0]);

      String[] threeSimple = "#Foo#Bar#Batz#".SplitEnclosedTuple();

      Assert.AreEqual(3, threeSimple.Length);
      Assert.AreEqual("Foo", threeSimple[0]);
      Assert.AreEqual("Bar", threeSimple[1]);
      Assert.AreEqual("Batz", threeSimple[2]);

      String[] leadingEmpty = "##Bar#Batz#".SplitEnclosedTuple();

      Assert.AreEqual(3, leadingEmpty.Length);
      Assert.AreEqual("", leadingEmpty[0]);
      Assert.AreEqual("Bar", leadingEmpty[1]);
      Assert.AreEqual("Batz", leadingEmpty[2]);

      String[] trailingEmpty = "#Foo#Bar##".SplitEnclosedTuple();

      Assert.AreEqual(3, trailingEmpty.Length);
      Assert.AreEqual("Foo", trailingEmpty[0]);
      Assert.AreEqual("Bar", trailingEmpty[1]);
      Assert.AreEqual("", trailingEmpty[2]);

      String[] twoEmpty = "###".SplitEnclosedTuple();

      Assert.AreEqual(2, twoEmpty.Length);
      Assert.AreEqual("", twoEmpty[0]);
      Assert.AreEqual("", twoEmpty[1]);

      // Having an escaped # in the middle of an element => should not cause a split

      String[] escapedSeparators = @"#\#\Foo#B\#\ar#B\\atz\#\#IEndWithBackslahZero\\0#".SplitEnclosedTuple();

      Assert.AreEqual(4, escapedSeparators.Length);
      Assert.AreEqual("#Foo", escapedSeparators[0]);
      Assert.AreEqual("B#ar", escapedSeparators[1]);
      Assert.AreEqual(@"B\atz#", escapedSeparators[2]);
      Assert.AreEqual(@"IEndWithBackslahZero\0", escapedSeparators[3]);

      // Having escaped escape chars => should unescape the escape char

      String[] escapedEscapeChars = @"#C:\\Temp\\#D:\\Data\\Foo#".SplitEnclosedTuple();

      Assert.AreEqual(2, escapedEscapeChars.Length);
      Assert.AreEqual("C:\\Temp\\", escapedEscapeChars[0]);
      Assert.AreEqual("D:\\Data\\Foo", escapedEscapeChars[1]);
    }

    [TestMethod()]
    public void SplitEnclosedTupleToInt() {

      int[] intValues = "#1#2#3#".SplitEnclosedTupleToInt();
      Assert.AreEqual(3, intValues.Length);
      Assert.AreEqual(1, intValues[0]);
      Assert.AreEqual(2, intValues[1]);
      Assert.AreEqual(3, intValues[2]);

      // Error case: Not a number

      int[] intValuesNotANumber = "#1#abc#3#".SplitEnclosedTupleToInt();
      Assert.IsNull(intValuesNotANumber);

      // Error case: Too big for int

      int[] intValuesTooBig = "#1#2083349429634175975#3#".SplitEnclosedTupleToInt();
      Assert.IsNull(intValuesTooBig);
    }

    [TestMethod()]
    public void SplitEnclosedTupleToLong() {

      long[] longValues = "#1#2#3#".SplitEnclosedTupleToLong();
      Assert.AreEqual(3, longValues.Length);
      Assert.AreEqual(1, longValues[0]);
      Assert.AreEqual(2, longValues[1]);
      Assert.AreEqual(3, longValues[2]);

      // Error case: Not a number

      long[] longValuesNotANumber = "#1#abc#3#".SplitEnclosedTupleToLong();
      Assert.IsNull(longValuesNotANumber);

      // Error case: Too big for long

      long[] longValuesTooBig = "#1#22222222083349429634175975#3#".SplitEnclosedTupleToLong();
      Assert.IsNull(longValuesTooBig);
    }

    [TestMethod()]
    public void ToEnclosedTuple_IEnumerables_CreateExpectedResults() {

      long[] longValues = { 2083349424987289070, 2083349429634175975, 2083349433585439538 };

      Assert.AreEqual(
        "#2083349424987289070#2083349429634175975#2083349433585439538#",
        longValues.ToEnclosedTuple()
      );

      List<DateTime> dateValues = new List<DateTime> { new DateTime(1973, 12, 9), new DateTime(2000, 1, 1), new DateTime(2022, 2, 22) };

      Assert.AreEqual(
        "#1973-12-09#2000-01-01#2022-02-22#",
        dateValues.ToEnclosedTuple((d) => ((DateTime)d).ToString("yyyy-MM-dd"))
      );

    }

    [TestMethod()]
    public void ToEnclosedTuple_StringArrays_CreateExpectedResults() {

      String[] stringArray;

      stringArray = new String[] { "First", "Mambo#Five" };
      Assert.AreEqual(@"#First#Mambo\#\Five#", stringArray.ToEnclosedTuple());
    }

    [TestMethod()]
    public void ForEachEnclosedTupleElement_OnlyCounting_CreateExpectedCounts() {

      String nullString = null;

      Assert.AreEqual(-1, nullString.ForEachEnclosedTupleElement(null));

      Assert.AreEqual(-2, "#".ForEachEnclosedTupleElement(null));
      Assert.AreEqual(-2, @"#\#\".ForEachEnclosedTupleElement(null));

      Assert.AreEqual(0, "".ForEachEnclosedTupleElement(null));

      Assert.AreEqual(0, @"\0".ForEachEnclosedTupleElement(null));
      Assert.AreEqual(1, @"#\0#".ForEachEnclosedTupleElement(null));

      Assert.AreEqual(-2, "a#".ForEachEnclosedTupleElement(null));
      Assert.AreEqual(-2, "#a".ForEachEnclosedTupleElement(null));
      Assert.AreEqual(1, "##".ForEachEnclosedTupleElement(null));
      Assert.AreEqual(1, "#a#".ForEachEnclosedTupleElement(null));

    }

    [TestMethod()]
    public void TupleReflectsNullTests() {

      String nullString = null;

      Assert.AreEqual(false, nullString.TupleReflectsNull());
      Assert.AreEqual(false, "".TupleReflectsNull());
      Assert.AreEqual(false, @"#\0#".TupleReflectsNull());
      Assert.AreEqual(false, @"\00".TupleReflectsNull());
      Assert.AreEqual(true, @"\0".TupleReflectsNull());

    }

  }

}
