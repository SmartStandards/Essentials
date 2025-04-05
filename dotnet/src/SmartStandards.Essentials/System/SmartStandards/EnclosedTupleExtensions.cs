using System.Collections.Generic;
using System.Text;

namespace System.SmartStandards {

  public static class EnclosedTupleExtensions {

    public static int IndexOfTupleElement(
      this string extendee, string value, 
      char separator = '#', 
      char escapeChar = '\\'
    ) {

      if (extendee is null || string.IsNullOrEmpty(extendee)) return -1;

      // "\0" represents a container being null itself
      if (extendee.Length == 2 && extendee[0] == escapeChar && extendee[1] == '0') return -1;

      // Handle malformed tuples

      // "#" (separator only) is malformed
      if (extendee.Length == 1 && extendee[0] == separator) return -2;

      // "#a\#" ending with escaped separator is malformed      
      if (extendee.EndsWith($"{escapeChar}{separator}{escapeChar}")) return -2;

      // "a#" and "#a" are malformed
      if ((extendee[0] != separator) || (extendee[extendee.Length - 1] != separator)) return -2;

      bool escaping = false;
      bool anotherEscapeExpected = false;
      int cursor;
      char peek;
      int elementIndex = -1;
      int elementLength = 0; // Length without escaped chars
      int valueCursor = 0;
      bool charAccepted;
      bool matchFound = true;

      int loopTo = extendee.Length - 1;

      for (cursor = 0; cursor <= loopTo; cursor++) {

        peek = extendee[cursor];
        charAccepted = false;

        if (anotherEscapeExpected) {
          anotherEscapeExpected = false;
          if (peek == escapeChar) continue;
          // Reaching this line of code means, the tuple is malformed (e.g. "#Backslash\#AfterHashtagIsMissing#")
          // But we are forgiving and treat the peek char regularily (value would be "Backslash#AfterHashtagIsMissing")
        }

        if (escaping) { // Escaped char detected...

          if (peek == '0') { // \0 represents null
            if (value is null) return elementIndex;
            matchFound = false;
          } else if (peek == separator) {
            anotherEscapeExpected = true;
          }

          elementLength += 1;
          charAccepted = true;
          escaping = false;

        } else if (peek == escapeChar) { // Escape char detected => skip it
          escaping = true;

        } else if (peek == separator) { // Separator char detected => handle current element, start a new one

          if (elementIndex < 0) { // The first occurance of "#" is not the end of a tuple element => ignore
            elementIndex += 1;
            continue;
          }

          // Handle Special case empty element "##" represents an empty string (not a null value)
          if (elementLength == 0) {
            if (value == null || !string.IsNullOrEmpty(value)) matchFound = false;
          }

          if (value != null && value.Length != elementLength) matchFound = false;

          // Handle regular match:

          if (matchFound) return elementIndex;

          matchFound = true; // Optimitically lokking to the upcoming tuple

          valueCursor = 0;
          elementLength = 0;
          elementIndex++;

        } else { // Regular char detected

          elementLength += 1;

          if (!matchFound) continue;

          charAccepted = true;
        }

        if (charAccepted) {
          if (value is null || value.Length <= valueCursor || peek != value[valueCursor]) matchFound = false;
          valueCursor++;
        }

      }

      return -1;
    }

    /// <summary>
    ///   Appends value to a StringBuilder instance, following the conventions of an enclosed tuple.
    /// </summary>
    /// <param name="value"> 
    ///   The string to append. 
    ///   If it is null, it will be escaped to "\0".
    ///   If it is an empty string, it will be appended as empty string.
    ///   If the string contains occurances of the separator or the escape char, they will be escaped.
    /// </param>
    /// <remarks>
    ///   Examples:
    ///   {null} => "#\0#"
    ///   {""} => "##"
    ///   {"Foo"} => "#Foo#"
    ///   {"Foo","Bar"} => "#Foo#Bar#"
    ///   {"Fo#o","Ba\r"} => "#Fo\#o#Ba\\r#"
    /// </remarks>
    public static StringBuilder AppendToEnclosedTuple(
      this StringBuilder extendee, string value, 
      char separator = '#', 
      char escapeChar = '\\'
    ) {

      if (extendee is null) return null;

      if (extendee.Length == 0) extendee.Append(separator);

      if (value is null) {
        extendee.Append(escapeChar).Append("0").Append(separator);
        return extendee;
      }

      for (int i = 0, loopTo = value.Length - 1; i <= loopTo; i++) {

        char c = value[i];

        if (c == escapeChar || c == separator) extendee.Append(escapeChar);

        extendee.Append(c);

        // Separator must be escaped symmetrically
        if (c == separator) extendee.Append(escapeChar); // "Mambo#Five" => "#Mambo\#\Five#"

      }

      extendee.Append(separator);

      return extendee;
    }

    /// <summary>
    ///   Renders an enclosed tuple representation from the given array.
    /// </summary>
    /// <param name="allowNull"> Default = false. True => Returns the nullRepresentation, if the array is null (false => Throw an exception) </param>
    /// <param name="nullRepresentation"> Default = "\0" </param>
    /// <returns>
    ///   null => exception or "\0"
    ///   {} => ""
    ///   {""} => "##"
    ///   {null} => "#\0#"
    ///   {"Foo"} => "#Foo#"
    ///   {"Foo","Bar"} => "#Foo#Bar#"
    ///   {"Fo#o","Ba\r"} => "#Fo\#o#Ba\\r#"
    /// </returns>
    public static String ToEnclosedTuple(
      this String[] extendee, 
      bool allowNull = false, 
      string nullRepresentation = "\0"
    ) {

      if (extendee == null) {
        if (!allowNull) throw new ArgumentNullException(nameof(extendee));
        return nullRepresentation;
      }

      StringBuilder enclosedTupleBuilder = new StringBuilder(250);

      foreach (string i in extendee) {
        enclosedTupleBuilder.AppendToEnclosedTuple(i);
      }

      return enclosedTupleBuilder.ToString();
    }

    public static bool TupleReflectsNull(this string extendee, char escapeChar = '\\') {

      if (extendee is null) return false;

      if (extendee.Length == 2 && extendee[0] == escapeChar && extendee[1] == '0') return true;

      return false;
    }

    /// <summary>
    ///   Calls a delegate for each substring from extendee that is delimited by a separator Unicode character.
    ///   If the separator is escaped, it will not lead to a split, but appear in the substring (unescaped).
    /// </summary>
    /// <param name="extendee"> 
    ///   The string to be split. If it's null, no callback occurs. If it's empty, one callback with an emtpy stringBuilder will occur.
    /// </param>
    /// <param name="separator"> An Unicode character that delimits the substrings. </param>
    /// <param name="escapeChar"> An Unicode character that can escape the separator or itself. </param>
    /// <param name="onEachElementMethod">
    ///   A callback delegate that will be called for each substring. A StringBuilder containing the unescaped substring is passed. 
    ///   If the the delegate returns true, it will cancel the further iteration of substrings.
    ///   If the delegate is null, the function will nevertheless count the elements and return the count.
    /// </param>
    /// <returns> 
    ///   The number of elements.
    ///   If the delegate cancelled the full iteration, the numer of actual iterations is returned.
    /// </returns>
    public static int ForEachEnclosedTupleElement(this string extendee, Func<int, StringBuilder, bool> onEachElementMethod, char separator = '#', char escapeChar = '\\') {

      if (extendee is null) return -1;

      if (string.IsNullOrEmpty(extendee)) return 0; // This is an empty tuple => 0 elements ("##") would be 1 empty element

      // "\0" represents a container being null itself
      if (extendee.Length == 2 && extendee[0] == escapeChar && extendee[1] == '0') return 0;

      // "#" (separator only) is malformed
      if (extendee.Length == 1 && extendee[0] == separator) return -2;

      // "#a\#" ending with escaped separator is malformed      
      if (extendee.EndsWith($"{escapeChar}{separator}{escapeChar}")) return -2;

      // "a#" and "#a" are malformed
      if ((extendee[0] != separator) || (extendee[extendee.Length - 1] != separator)) return -2;

      StringBuilder elementBuilder = null;

      bool escaping = false;
      bool anotherEscapeExpected = false;
      bool nulled = false;
      int cursor;
      char peek;
      int elementIndex = -1;

      int loopTo = extendee.Length - 1;

      for (cursor = 0; cursor <= loopTo; cursor++) {

        peek = extendee[cursor];

        if (anotherEscapeExpected) {
          anotherEscapeExpected = false;
          if (peek == escapeChar) continue;
          // Reaching this line of code means, the tuple is malformed (e.g. "#Backslash\#AfterHashtagIsMissing#")
          // But we are forgiving and treat the peek char regularily (value would be "Backslash#AfterHashtagIsMissing")
        }

        if (escaping) { // Escaped char detected...

          if (peek == '0') {
            nulled = true;
            elementBuilder = null;
          } else {
            if (peek != separator && peek != escapeChar) elementBuilder.Append(escapeChar); // Preserve escape char if non-escapable char followed
            elementBuilder?.Append(peek);
            if (peek == separator) anotherEscapeExpected = true;
          }

          escaping = false;

        } else if (peek == escapeChar) { // Escape char detected => skip it
          escaping = true;

        } else if (peek == separator) { // Separator char detected => handle current element, start a new one

          if (nulled) {
            if ((bool)(onEachElementMethod?.Invoke(elementIndex, null))) return elementIndex + 1;
          } else if (elementBuilder != null) {
            if (onEachElementMethod.Invoke(elementIndex, elementBuilder)) return elementIndex + 1;
          }

          elementIndex += 1;
          nulled = false;

          if (onEachElementMethod != null) elementBuilder = new StringBuilder(80);

        } else {
          elementBuilder?.Append(peek);

        } // Regular char detected => append to the current element
      }

      return elementIndex;
    }

    /// <summary>
    ///   Returns a string array that contains the substrings from extendee that are delimited by a separator Unicode character.
    ///   If the separator is escaped, it will not lead to a split, but appear in the substring (unescaped).
    /// </summary>
    /// <param name="extendee"> The string to be split. If it's null, null will be returned. If it's empty, an emtpy array will be returned. </param>
    /// <param name="separator"> An Unicode character that delimits the substrings. </param>
    /// <param name="escapeChar"> An Unicode character that can escape the separator or itself. </param>
    /// <returns> 
    ///   Null, if extendee was null.
    ///   An empty array, if extendee was an empty string. 
    ///   An array whose elements contain the substrings from extendee that are delimited by the separator.
    ///   The substrings are unescaped.
    /// </returns>
    /// <remarks>
    ///   Bad performance! The SplitUnlessEscaped() extension offering a callback function is faster.
    /// </remarks>
    public static string[] SplitEnclosedTuple(this string extendee, char separator = '#', char escapeChar = '\\') {

      if (extendee is null) return null;

      // "\0" represents a container being null itself
      if (extendee.Length == 2 && extendee[0] == escapeChar && extendee[1] == '0') return null;

      if (string.IsNullOrEmpty(extendee)) return Array.Empty<string>();

      List<StringBuilder> elementBuilders = new List<StringBuilder>();

      extendee.ForEachEnclosedTupleElement((tokenIndex, elementBuilder) => {
        elementBuilders.Add(elementBuilder);
        return false;
      }, separator, escapeChar);

      int upperBound = elementBuilders.Count - 1;

      string[] tokens = new string[upperBound + 1];

      for (int i = 0, loopTo = upperBound; i <= loopTo; i++) tokens[i] = elementBuilders[i]?.ToString();

      return tokens;
    }

  }

}