using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace System.SmartStandards {

  public static class PlaceholderExtensions {

    /// <summary>
    ///   Executes a callback method for each placeholder in a template string.
    /// </summary>
    /// <param name="extendee">
    ///   A template string containing named placeholders. 
    ///   E.g. "Hello {audience}, the answer is {answer}."
    /// </param>
    /// <param name="onPlaceholderFound">
    ///   bool onPlaceholderFound(string placeholderName).
    ///   Will be called for each placeholder in order of appearance.
    ///   (e.g. "audience", "answer").
    ///   The placeholder name will be passed (without braces), unless omitPlaceholderNames is set (then null will be passed).
    ///   If the callback function returns true (= cancel), the processing will stop immedieately.
    /// </param>
    /// <param name="onRegularSegmentFound">
    ///   void onRegularSegmentFound(int pos, int length).
    ///   Optional. Will be called for each seqgment of the template that is not a placeholder.
    ///   (e.g. "Hello ", ", the answer is ", ".").
    /// </param>
    /// <param name="omitPlaceholderNames">
    ///   Performance optimization. If true, the placeholder name is not extracted from the template.
    /// </param>
    /// <remarks>
    ///   v 0.1.0
    /// </remarks>
    public static void ForEachPlaceholder(
      this string extendee,
      Func<string, bool> onPlaceholderFound, Action<int, int> onRegularSegmentFound = null, bool omitPlaceholderNames = false
    ) {

      if (extendee is null || extendee.Length < 3) return;

      int cursor = 0;

      do {

        int leftPos = extendee.IndexOf("{", cursor);

        if (leftPos < 0) break;

        int rightPos = extendee.IndexOf("}", cursor);

        if (rightPos < 0 || rightPos < leftPos + 1) return;

        string placeholderName = null;

        if (!omitPlaceholderNames) placeholderName = extendee.Substring(leftPos + 1, rightPos - leftPos - 1);

        onRegularSegmentFound?.Invoke(cursor, leftPos - cursor);

        if (onPlaceholderFound.Invoke(placeholderName)) return;

        cursor = rightPos + 1;
      }
      while (cursor < extendee.Length);

      onRegularSegmentFound?.Invoke(cursor, extendee.Length - cursor);
    }

    /// <summary>
    ///   Resolves named placeholders in a template string from arguments.
    /// </summary>
    /// <param name="extendee">
    ///   A template string containing named placeholders. 
    ///   E.g. "Hello {audience}, the answer is {answer}."
    /// </param>
    /// <param name="args">
    ///   Arguments containing the placeholder values in order of appearance in the template. Example:
    ///   "World", 42
    /// </param>
    /// <returns>
    ///   Null or a new string instance with resolved placeholders. The example would be resolved to:
    ///   "Hello World, the answer is 42."
    /// </returns>
    /// <remarks>
    ///   v 0.1.0
    /// </remarks>
    public static string ResolvePlaceholders(this string extendee, params object[] args) {

      int maxIndex = args != null ? args.GetUpperBound(0) : -1;

      if (extendee is null || extendee.Length < 3 || maxIndex < 0) return extendee;

      int i = -1;

      return extendee.ResolvePlaceholdersByCallback(
        dummy => {
          i++;
          if (i <= maxIndex) {
            return args[i]?.ToString();
          } else {
            return null;
          } // Der Platzhalter wurd gar nicht gefunden => null
        },
        true
      );
    }

    /// <remarks>
    ///   v 0.1.0
    /// </remarks>
    public static StringBuilder ResolvePlaceholders(this StringBuilder extendee, params object[] args) {

      if (extendee == null || args == null) { return extendee; }

      if (args.Length == 0) { return extendee; }

      int cursor = 0;

      foreach (object boxedValue in args) {

        int left = -1;

        for (int i = cursor; i < extendee.Length; i++) {
          if (extendee[i] == '{') { left = i; break; };
        }

        if (left == -1) { break; }

        int right = -1;

        for (int i = left; i < extendee.Length; i++) {
          if (extendee[i] == '}') { right = i; break; };
        }

        if (right == -1) { break; }

        extendee.Remove(left, right - left + 1);

        string value = boxedValue.ToString();

        extendee.Insert(left, value);

        cursor += value.Length;
      }

      return extendee;
    }

    /// <remarks>
    ///   v 0.1.0
    /// </remarks>
    public static string ResolvePlaceholdersByDictionary(this string extendee, IDictionary<string, string> placeholders) {

      if (extendee is null || extendee.Length < 3 || placeholders is null || placeholders.Count == 0) {
        return extendee;
      }

      return extendee.ResolvePlaceholdersByCallback(key => {
        string value = null;
        if (placeholders.TryGetValue(key, out value)) {
          return value ?? ""; // Der Platzhalterwert is null => Leerstring
        } else {
          return null;
        } // Der Platzhalter wurd gar nicht gefunden => null
      });
    }

    /// <remarks>
    ///   v 0.1.0
    /// </remarks>
    public static string ResolvePlaceholdersByPropertyBag(this string extendee, object propertyBag) {

      if (extendee is null || extendee.Length < 3 || propertyBag is null) {
        return extendee;
      }

      return extendee.ResolvePlaceholdersByCallback(key => {
        var propertyInfo = propertyBag.GetType().GetProperty(key, BindingFlags.Public | BindingFlags.IgnoreCase | BindingFlags.Instance);
        if (propertyInfo != null) {
          return propertyInfo.GetValue(propertyBag).ToString() ?? ""; // Der Platzhalterwert is null => Leerstring
        } else {
          return null;
        } // Der Platzhalter wurd gar nicht gefunden => null
      });
    }

    /// <summary>
    ///   Resolves named placeholders in a template string by calling back a resolving method for each placeholder.
    /// </summary>
    /// <param name="extendee"> A template string containing named placeholders. E.g. "Hello {audience}, the answer is {answer}."</param>
    /// <param name="onResolvePlaceholder">
    ///   string onResolvePlaceholder(name).
    ///   Will be called for each placeholder in order of appearance.
    ///   (e.g. "audience", "answer").
    ///   The placeholder name will be passed (or null, if omitPlaceholderNames is set).
    ///   The resolved placeholder value should be returned. If null is returned, the template is left unchanged once, but the resolving is continued.
    ///   </param>
    /// <param name="omitPlaceholderNames">
    ///   Performance optimization. If true, the placeholder name is not extracted from the template.
    /// </param>
    /// <returns> The resolved string. </returns>
    /// <remarks>
    ///   v 0.1.0
    /// </remarks>
    public static string ResolvePlaceholdersByCallback(
      this string extendee, Func<string, string> onResolvePlaceholder, bool omitPlaceholderNames = false
    ) {

      if (extendee is null || extendee.Length < 3) return extendee;

      var targetStringBuilder = new StringBuilder((int)Math.Round(extendee.Length * 1.5d));

      bool onPlaceholderFound(string placeholderName) {
        string value = onResolvePlaceholder.Invoke(placeholderName);
        if (value != null) {
          targetStringBuilder.Append(value);
        } else {
          targetStringBuilder.Append("{").Append(placeholderName).Append("}");
        }
        return false;
      }

      void onRegularSegmentFound(int pos, int length) => targetStringBuilder.Append(extendee, pos, length);

      extendee.ForEachPlaceholder(onPlaceholderFound, onRegularSegmentFound, omitPlaceholderNames);

      return targetStringBuilder.ToString();
    }

  }
}
