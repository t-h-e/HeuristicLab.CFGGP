#region License Information
/* HeuristicLab
 * Copyright (C) 2002-2015 Heuristic and Evolutionary Algorithms Laboratory (HEAL)
 *
 * This file is part of HeuristicLab.
 *
 * HeuristicLab is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * HeuristicLab is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with HeuristicLab. If not, see <http://www.gnu.org/licenses/>.
 */
#endregion

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using HeuristicLab.Data;
using HeuristicLab.Encodings.SymbolicExpressionTreeEncoding;
using Newtonsoft.Json;

namespace HeuristicLab.Problems.CFG.Python {
  public class PythonHelper {
    private const String INDENTSPACE = "  ";
    private static string LINESEPARATOR = Environment.NewLine;

    private const string LOOPBREAK = "loopBreak";
    private const string LOOPBREAKUNNUMBERED = "loopBreak%";
    private const string LOOPBREAK_INITIALISE = "loopBreak% = 0";
    private const string LOOPBREAK_IF = "if loopBreak% >";
    private const string LOOPBREAK_INCREMENT = "loopBreak% += 1";

    private const string LOOPBREAK_CONST_COMMENT = "# constant defines allowed maximum number of all loops performed";
    private const string LOOPBREAK_CONST = "loopBreakConst = ";
    private const string LOOPBREAK_INITIAL_COMMENT = "# initialises variable which should be used to count the number of loop iterations";
    private const string LOOPBREAK_INITIAL = "loopBreak = 0";

    private const string FUNCTIONSTART = "def evolve():";

    private const string FORCOUNTER = "forCounter";
    private const string FORCOUNTERUNNUMBERED = "forCounter%";

    public static string FormatToProgram(ISymbolicExpressionTree tree, int loopbreakConst, string header, string footer = null) {
      StringBuilder strBuilder = new StringBuilder();
      string indent = String.Empty;
      if (header != null) {
        strBuilder.Append(header);
        int lastNewLine = header.LastIndexOf(Environment.NewLine);
        if (lastNewLine > 0) {
          indent = header.Substring(lastNewLine + Environment.NewLine.Length, header.Length - lastNewLine - Environment.NewLine.Length);
        } else {
          indent = header;
        }
        if (!String.IsNullOrWhiteSpace(indent)) {
          indent = "";
        }
      }

      // add loop break condition variables
      strBuilder.AppendLine(LOOPBREAK_CONST_COMMENT);
      strBuilder.AppendLine(indent + LOOPBREAK_CONST + loopbreakConst);
      strBuilder.AppendLine(indent + LOOPBREAK_INITIAL_COMMENT);
      strBuilder.AppendLine(indent + LOOPBREAK_INITIAL);
      strBuilder.Append(indent);

      string program = ConvertBracketsToIndent(
                       CFGSymbolicExpressionTreeStringFormatter.StaticFormat(
                       tree), indent);

      strBuilder.Append(program);
      if (footer != null) {
        strBuilder.Append(footer);
      }
      return strBuilder.ToString();
    }

    public static string ConvertBracketsToIndent(string code, string additionalIndent = "") {
      StringBuilder stringBuilder = new StringBuilder();
      String[] split = code.Split(new string[] { Environment.NewLine }, StringSplitOptions.None); ;

      int indent = 0;

      // break loops individually
      Stack<int> loopBreakStack = new Stack<int>();
      //int absoluteLoopBreak = 0;

      int forCounterNumber = 0;

      bool first = true;

      foreach (String part in split) {
        String line = part.Trim();
        // remove intentation if bracket is at the beginning of the line
        while (line.StartsWith(":}")) {
          indent--;
          line = line.Substring(2, line.Length - 2).Trim();
        }

        // add indent
        if (!first) {
          stringBuilder.Append(additionalIndent);
        } else {
          first = false;
        }
        for (int i = 0; i < indent; i++) {
          stringBuilder.Append(INDENTSPACE);
        }

        // add indentation
        while (line.EndsWith("{:")) {
          indent++;
          line = line.Substring(0, line.Length - 2).Trim();
        }
        // remove indentation if bracket is at the end of the line
        while (line.EndsWith(":}")) {
          indent--;
          line = line.Substring(0, line.Length - 2).Trim();
        }

        // break loops individually
        //            if(line.contains(LOOPBREAKUNNUMBERED)) {
        //                if(line.contains(LOOPBREAK_INITIALISE)) {
        //                    loopBreakStack.push(absoluteLoopBreak);
        //                    line = line.replace(LOOPBREAKUNNUMBERED, LOOPBREAK + absoluteLoopBreak);
        //                    absoluteLoopBreak++;
        //                } else if (line.contains(LOOPBREAK_IF)) {
        //                    line = line.replace(LOOPBREAKUNNUMBERED, LOOPBREAK + loopBreakStack.peek());
        //                } else if (line.contains(LOOPBREAK_INCREMENT)) {
        //                    line = line.replace(LOOPBREAKUNNUMBERED, LOOPBREAK + loopBreakStack.pop());
        //                } else {
        //                    throw new IllegalArgumentException("Python 'while break' is malformed.");
        //                }
        //            } else if(line.contains(FORCOUNTERUNNUMBERED)) {
        //                line = line.replace(FORCOUNTERUNNUMBERED, FORCOUNTER + forCounterNumber);
        //                forCounterNumber++;
        //            }

        // break all loops after a certain number of iterations
        //if (line.Contains(FUNCTIONSTART)) {
        //    // initialise while break
        //    line += LINESEPARATOR + INDENTSPACE + LOOPBREAK + " = 0";
        //}
        if (line.Contains(LOOPBREAKUNNUMBERED)) {
          if (line.Contains(LOOPBREAK_INITIALISE)) {
            line = ""; // remove line
          } else if (line.Contains(LOOPBREAK_IF)) {
            line = line.Replace(LOOPBREAKUNNUMBERED, LOOPBREAK);
          } else if (line.Contains(LOOPBREAK_INCREMENT)) {
            line = line.Replace(LOOPBREAKUNNUMBERED, LOOPBREAK);
          } else {
            throw new ArgumentException("Python 'while break' is malformed.");
          }
        } else if (line.Contains(FORCOUNTERUNNUMBERED)) {
          line = line.Replace(FORCOUNTERUNNUMBERED, FORCOUNTER + forCounterNumber);
          forCounterNumber++;
        }

        // add line to code
        stringBuilder.Append(line);
        stringBuilder.Append(LINESEPARATOR);
      }

      return stringBuilder.ToString();
    }

    public static string ConvertToPythonValues(StringArray array, IEnumerable<int> indices) {
      StringBuilder strBuilder = new StringBuilder("[");
      foreach (int row in indices) {
        strBuilder.Append("[");
        strBuilder.Append(array[row]);
        strBuilder.Append("],");
      }
      strBuilder.Append("]");
      return strBuilder.ToString();
    }

    private static JsonSerializerSettings settings = new JsonSerializerSettings { Converters = new List<JsonConverter>() { new BoolConverter() } };
    private static JsonSerializer serializer = JsonSerializer.CreateDefault(settings);
    public static string SerializeCSToPythonJson(object value) {
      StringBuilder sb = new StringBuilder(256);
      StringWriter sw = new StringWriter(sb, CultureInfo.InvariantCulture);
      using (var nWriter = new NullWriter(sw)) {
        serializer.Serialize(nWriter, value);
      }
      return sw.ToString();
    }

    private class BoolConverter : JsonConverter {
      public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
        if (value is bool) {
          writer.WriteRawValue(((bool)value) ? "True" : "False");
        } else {
          throw new ArgumentException(String.Format("Expecting bool value got {0}", value == null ? "null" : value.GetType().ToString()));
        }
      }

      public override bool CanConvert(Type objectType) {
        return objectType == typeof(bool);
      }

      public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
        throw new NotImplementedException();
      }

      public override bool CanRead { get { return false; } }
      public override bool CanWrite { get { return true; } }
    }

    private class NullWriter : JsonTextWriter {
      public NullWriter(TextWriter textWriter) : base(textWriter) { }
      public override void WriteNull() {
        WriteRawValue("None");
      }
    }
  }
}
