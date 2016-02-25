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
using System.Linq;
using System.Text.RegularExpressions;
using HeuristicLab.Encodings.SymbolicExpressionTreeEncoding;
using HeuristicLab.PluginInfrastructure;

namespace HeuristicLab.Problems.CFG {
  public class CFGParser {
    //private const string rulePattern = @"(?<rulename><\S+>)\s*::=\s*(?<production>(?(?=\/\/)\/\/[^\r\n]*$|.+?)+?(?(?=<\S+>\s*::=)(?=<\S+>\s*::=)|\Z))";
    private const string rulePattern = @"(?<rulename><\S+>)\s*::=\s*(?<production>(?(?=\/\/)\/\/[^\r\n]*|.+?)+?(?(?!<\S+>\s*::=)\Z))";

    private const string productionPattern = @"(?(?=\/\/)(?:\/\/.*$)|\s*(?<production>(?:[^'""\|\/\/]+|'.*?'|"".*?"")+))";

    private const string ruleInProdPattern = @"\ *([\r\n]+)\ *|([^'""<\r\n]+)|'(.*?)'|""(.*?)""|(?<subrule><[^>|\s]+>)|([<]+)";

    private static Regex ruleRegex = new Regex(rulePattern, RegexOptions.Singleline);

    private static Regex productionRegex = new Regex(productionPattern, RegexOptions.Multiline);

    private static Regex ruleInProdRegex = new Regex(ruleInProdPattern);

    public CFGExpressionGrammar readGrammarBNF(String bnf) {
      try {
        return readGrammarBNFPrivate(bnf);
      }
      catch (Exception e) {
        ErrorHandling.ShowErrorDialog(e);
        return new CFGExpressionGrammar();
      }
    }

    private CFGExpressionGrammar readGrammarBNFPrivate(String bnf) {
      CFGExpressionGrammar treeGrammar = new CFGExpressionGrammar();

      Dictionary<CFGProduction, List<string>> productions = new Dictionary<CFGProduction, List<string>>();
      GroupSymbol root = null;
      var rule = ruleRegex.Match(bnf);
      while (rule.Success) {
        GroupSymbol curRuleSymbolGroup = new GroupSymbol();
        curRuleSymbolGroup.Name = "Rule: " + rule.Groups["rulename"].Value;

        treeGrammar.AddSymbol(curRuleSymbolGroup);
        if (root == null) {
          root = curRuleSymbolGroup;
        }

        var production = productionRegex.Match(rule.Groups["production"].Value);

        while (production.Success) {
          string productionName = production.Groups["production"].Value.Trim();
          if (!String.IsNullOrWhiteSpace(productionName)) {
            // production rule already exists
            // increase initial frequency
            if (curRuleSymbolGroup.SymbolsCollection.Any(x => x.Name.Equals(productionName))) {
              var symbol = (CFGSymbol)curRuleSymbolGroup.SymbolsCollection.First(x => x.Name.Equals(productionName));
              symbol.InitialFrequency++;
            } else {

              List<string> parts = new List<string>();
              parts.Add(String.Empty);

              var subRule = ruleInProdRegex.Match(productionName);

              List<string> rulesInProduction = new List<string>();
              while (subRule.Success) {
                if (!String.IsNullOrEmpty(subRule.Groups["subrule"].Value)) {
                  rulesInProduction.Add(subRule.Groups["subrule"].Value);
                  subRule = subRule.NextMatch();
                  parts.Add(String.Empty);
                  continue;
                }

                var part = String.Join(String.Empty, Enumerable.Range(1, 6).Select(x => subRule.Groups[x].Value));
                // unescape characters so that tab and newline can be defined in the grammar
                if (part.Contains('\\')) {
                  part = part.Replace("\\n", Environment.NewLine);
                  part = Regex.Unescape(part);
                }
                parts[parts.Count - 1] = parts[parts.Count - 1] + part;

                subRule = subRule.NextMatch();
              }

              CFGProduction productionSymbol = new CFGProduction(productionName, parts.Count == 0
                                                                                 ? new List<string>() { productionName }
                                                                                 : parts);
              productions.Add(productionSymbol, rulesInProduction);
              curRuleSymbolGroup.SymbolsCollection.Add(productionSymbol);
            }
          }
          production = production.NextMatch();
        }
        rule = rule.NextMatch();
      }

      // assign subrules for start symbol
      var ruleGroupSymbol = treeGrammar.Symbols.Where(x => x is GroupSymbol && x.Name == root.Name).First() as GroupSymbol;
      foreach (var subProduction in ruleGroupSymbol.SymbolsCollection) {
        treeGrammar.AddAllowedChildSymbol(treeGrammar.StartSymbol, subProduction, 0);
      }

      // assign subrules
      foreach (var allowedSymbol in treeGrammar.AllowedSymbols.Where(x => x is CFGProduction)) {
        var cfgProduction = allowedSymbol as CFGProduction;
        var rulesInProduction = productions[cfgProduction];
        if (rulesInProduction.Count == 0) continue;

        treeGrammar.SetSubtreeCount(cfgProduction, rulesInProduction.Count, rulesInProduction.Count);
        for (int i = 0; i < rulesInProduction.Count; i++) {
          ruleGroupSymbol = treeGrammar.Symbols.Where(x => x is GroupSymbol && x.Name.Substring("Rule: ".Length, x.Name.Length - "Rule: ".Length) == rulesInProduction[i]).First() as GroupSymbol;

          foreach (var subProduction in ruleGroupSymbol.SymbolsCollection) {
            treeGrammar.AddAllowedChildSymbol(cfgProduction, subProduction, i);
          }
        }
      }

      return treeGrammar;
    }
  }
}
