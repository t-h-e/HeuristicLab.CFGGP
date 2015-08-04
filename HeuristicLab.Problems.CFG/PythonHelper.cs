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
using System.Text;

namespace HeuristicLab.Problems.CFG {
  public class PythonHelper {
    private const String INDENTSPACE = "  ";
    private static String LINESEPARATOR = Environment.NewLine;

    private const String LOOPBREAK = "loopBreak";
    private const String LOOPBREAKUNNUMBERED = "loopBreak%";
    private const String LOOPBREAK_INITIALISE = "loopBreak% = 0";
    private const String LOOPBREAK_IF = "if loopBreak% >";
    private const String LOOPBREAK_INCREMENT = "loopBreak% += 1";

    private const String FUNCTIONSTART = "def evolve():";

    private const String FORCOUNTER = "forCounter";
    private const String FORCOUNTERUNNUMBERED = "forCounter%";

    public static String convertBracketsToIndent(String code, string additionalIndent = "") {
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
  }
}
