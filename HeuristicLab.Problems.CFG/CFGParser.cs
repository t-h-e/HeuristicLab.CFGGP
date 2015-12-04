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

namespace HeuristicLab.Problems.CFG {
  public class CFGParser {

    //private const string RULESYMBOL = "::=";
    //private static char[] CHECKCHARS = new char[] { ' ', '\'', '"', '<' };
    //private static char[] QUOTATION = new char[] { '\'', '"' };

    //private const string COMMENT = "//";
    //private const char PRODUCTIONSPLIT = '|';

    //private Dictionary<String, CFGSymbol> symbolDictionary = new Dictionary<string, CFGSymbol>();
    //private Dictionary<String, IEnumerable<CFGSymbol>> ruleDictionary = new Dictionary<String, IEnumerable<CFGSymbol>>();
    //private GroupSymbol ruleSymbols;

    private CFGExpressionGrammar treeGrammar;

    public CFGExpressionGrammar readGrammarBNF(String bnf) {
      try {
        //return readGrammarBNFPrivate(bnf);
        return readGrammarBNFPrivate2(bnf);
      }
      catch (Exception e) {
        Console.WriteLine(e.StackTrace);
        return new CFGExpressionGrammar();
      }
    }

    private const string rulePattern = @"(?<rulename><\S+>)\s*::=\s*(?<production>(?(?=\/\/)\/\/[^\r\n]*$|.+?)+?(?(?=<\S+>\s*::=)(?=<\S+>\s*::=)|\Z))";

    private const string productionPattern = @"(?(?=\/\/)(?:\/\/.*$)|\s*(?<production>(?:[^'""\|\/\/]+|'.*?'|"".*?"")+))";

    private const string ruleInProdPattern = @"\ *([\r\n]+)\ *|([^'""<\r\n]+)|'(.*?)'|""(.*?)""|(?<subrule><[^>|\s]+>)|([<]+)";

    private static Regex ruleRegex = new Regex(rulePattern, RegexOptions.Singleline);

    private static Regex productionRegex = new Regex(productionPattern, RegexOptions.Multiline);

    private static Regex ruleInProdRegex = new Regex(ruleInProdPattern);

    private CFGExpressionGrammar readGrammarBNFPrivate2(String bnf) {
      treeGrammar = new CFGExpressionGrammar();

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

                parts[parts.Count - 1] = parts[parts.Count - 1] + String.Join(String.Empty, Enumerable.Range(1, 6).Select(x => subRule.Groups[x].Value));

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

    //private CFGExpressionGrammar readGrammarBNFPrivate(String bnf) {
    //  treeGrammar = new CFGExpressionGrammar();
    //  ruleSymbols = new GroupSymbol();
    //  ruleSymbols.Name = "Rule symbols";

    //  treeGrammar.AddSymbol(ruleSymbols);

    //  String curLine;
    //  CFGSymbol rule = null;
    //  GroupSymbol curRuleSymbolGroup = null;
    //  String productionRuleName = null;
    //  List<string> productionRuleParts = new List<string>();
    //  ICollection<CFGSymbol> productionsForRule = new HashSet<CFGSymbol>();
    //  List<CFGSymbol> symbolsForProduction = new List<CFGSymbol>();

    //  string[] lines = bnf.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
    //  foreach (string line in lines) {
    //    curLine = line.Trim();
    //    if (String.IsNullOrEmpty(curLine) || curLine.StartsWith(COMMENT)) { continue; }

    //    if (IsRuleLine(curLine)) {
    //      //if the previous production rule has not been added yet, because the it could have continued. It is added now.
    //      if (productionRuleName != null) {
    //        if (rule == null) {
    //          throw new Exception("No rule specified.");
    //        }
    //        productionsForRule.Add(CreateProductionRule(productionRuleName, productionRuleParts, symbolsForProduction, curRuleSymbolGroup));
    //        productionRuleName = null;
    //        productionRuleParts = new List<string>();

    //        CreateRule(rule, productionsForRule);
    //      }

    //      int indexOfRuleSymbol = curLine.IndexOf(RULESYMBOL);

    //      String ruleName = curLine.Substring(0, indexOfRuleSymbol).Trim();
    //      if (rule == null) {
    //        rule = GetRule(ruleName);
    //        treeGrammar.AddAllowedChildSymbol(treeGrammar.StartSymbol, rule, 0);
    //      } else {
    //        rule = GetRule(ruleName);
    //      }
    //      curRuleSymbolGroup = new GroupSymbol();
    //      curRuleSymbolGroup.Name = ruleName.Substring(1, ruleName.Length - 2);
    //      treeGrammar.AddSymbol(curRuleSymbolGroup);

    //      int removeChars = indexOfRuleSymbol + RULESYMBOL.Length;
    //      curLine = curLine.Substring(removeChars, curLine.Length - removeChars).Trim();
    //    }

    //    if (curLine.ElementAt(0) == PRODUCTIONSPLIT && productionRuleName != null) {
    //      productionsForRule.Add(CreateProductionRule(productionRuleName, productionRuleParts, symbolsForProduction, curRuleSymbolGroup));
    //      productionRuleName = null;
    //      productionRuleParts = new List<string>();
    //    }

    //    IList<String> split = splitToProcutionRules(curLine);
    //    for (int i = 0; i < split.Count; i++) {
    //      String productionString = split[i].Trim();
    //      if (String.IsNullOrEmpty(productionString)) { continue; }

    //      if (productionRuleName == null) {
    //        productionRuleName = productionString;
    //      } else {
    //        productionRuleName += Environment.NewLine + productionString;
    //        if (productionRuleParts.Count == symbolsForProduction.Count) {
    //          productionRuleParts.Add(Environment.NewLine);
    //        } else if (productionRuleParts.Count > symbolsForProduction.Count) {
    //          productionRuleParts[productionRuleParts.Count - 1] += Environment.NewLine;
    //        } else {
    //          throw new ArgumentException("ParserError: Should not happen.");
    //        }
    //      }


    //      while (!String.IsNullOrEmpty(productionString)) {
    //        char firstChar = productionString.ElementAt(0);
    //        int endIndex = -1;
    //        bool quoted = false;
    //        bool terminalSymbol = true;
    //        switch (firstChar) {
    //          case '<':
    //            endIndex = productionString.IndexOf('>', 1);
    //            // increment to add '>' Symbol
    //            terminalSymbol = false;
    //            endIndex++;
    //            break;
    //          case '\'':
    //            endIndex = productionString.IndexOf('\'', 1);
    //            quoted = true;
    //            break;
    //          case '"':
    //            endIndex = productionString.IndexOf('"', 1);
    //            quoted = true;
    //            break;
    //          default:
    //            endIndex = productionString.Length;
    //            foreach (char curChar in CHECKCHARS) {
    //              int indexOfChar = productionString.IndexOf(curChar);
    //              if (indexOfChar > 0 && endIndex > indexOfChar) {
    //                endIndex = indexOfChar;
    //              }
    //            }
    //            break;
    //        }

    //        endIndex = endIndex <= 0
    //                   ? productionString.Length
    //                   : endIndex;

    //        // increment to include specifiec symbol
    //        String symbolName = quoted
    //                            ? productionString.Substring(1, endIndex - 1)
    //                            : productionString.Substring(0, endIndex);

    //        // unescape characters so that tab and newline can be defined in the grammar
    //        if (symbolName.Contains('\\')) {
    //          symbolName = symbolName.Replace("\\n", Environment.NewLine);
    //          symbolName = Regex.Unescape(symbolName);
    //        }

    //        if (terminalSymbol) {
    //          if (productionRuleParts.Count == symbolsForProduction.Count) {
    //            productionRuleParts.Add(symbolName);
    //          } else if (productionRuleParts.Count > symbolsForProduction.Count) {
    //            productionRuleParts[productionRuleParts.Count - 1] += symbolName;
    //          } else {
    //            throw new ArgumentException("ParserError: Should not happen.");
    //          }
    //        } else {
    //          if (productionRuleParts.Count == 0 || productionRuleParts.Count == symbolsForProduction.Count) {
    //            productionRuleParts.Add(String.Empty);
    //          }
    //          symbolsForProduction.Add(GetRule(symbolName));
    //        }

    //        endIndex = quoted ? endIndex + 1 : endIndex;
    //        productionString = productionString.Substring(endIndex, productionString.Length - endIndex);
    //        String trimmedProduction = productionString.Trim();

    //        //add whitespaces within a production
    //        int whitespaces = productionString.Length - trimmedProduction.Length;
    //        if (whitespaces > 0) {
    //          String whitespacePart = productionString.Substring(0, whitespaces);
    //          if (terminalSymbol) {
    //            productionRuleParts[productionRuleParts.Count - 1] += whitespacePart;
    //          } else {
    //            productionRuleParts.Add(whitespacePart);
    //          }
    //        }

    //        productionString = trimmedProduction;
    //      }

    //      if (i < split.Count - 1) {
    //        productionsForRule.Add(CreateProductionRule(productionRuleName, productionRuleParts, symbolsForProduction, curRuleSymbolGroup));
    //        productionRuleName = null;
    //        productionRuleParts = new List<string>();
    //      }
    //    }
    //  }

    //  productionsForRule.Add(CreateProductionRule(productionRuleName, productionRuleParts, symbolsForProduction, curRuleSymbolGroup));
    //  CreateRule(rule, productionsForRule);

    //  // reduce depth
    //  var allSymbols = treeGrammar.AllowedSymbols.Where(x => !(x is GroupSymbol));
    //  foreach (var symbol in allSymbols) {
    //    if (symbol == treeGrammar.StartSymbol || symbol == treeGrammar.ProgramRootSymbol) {
    //      // startsymbol only contains exactly one subtree, but maximum arity is set to 255 and cannot be changed
    //      var allowedSymbols = treeGrammar.GetAllowedChildSymbols(symbol, 0);
    //      var firstSymbol = allowedSymbols.FirstOrDefault() as CFGSymbol;
    //      if (allowedSymbols.Count() == 1 && firstSymbol != null) {
    //        treeGrammar.RemoveAllowedChildSymbol(symbol, firstSymbol, 0);
    //        foreach (var production in ruleDictionary[firstSymbol.Name]) {
    //          treeGrammar.AddAllowedChildSymbol(symbol, production, 0);
    //        }
    //      }
    //    } else {
    //      for (int index = 0; index < symbol.MaximumArity; index++) {
    //        var allowedSymbols = treeGrammar.GetAllowedChildSymbols(symbol, index);
    //        var firstSymbol = allowedSymbols.FirstOrDefault() as CFGSymbol;
    //        if (firstSymbol != null && ruleDictionary.ContainsKey(firstSymbol.Name)) {
    //          treeGrammar.RemoveAllowedChildSymbol(symbol, firstSymbol, index);
    //          foreach (var production in ruleDictionary[firstSymbol.Name]) {
    //            treeGrammar.AddAllowedChildSymbol(symbol, production, index);
    //          }
    //        }
    //      }
    //    }
    //  }

    //  foreach (var ruleNameSymbols in ruleSymbols.Symbols) {
    //    ruleNameSymbols.Enabled = false;
    //  }

    //  return treeGrammar;
    //}

    //private void CreateRule(CFGSymbol rule, ICollection<CFGSymbol> productions) {
    //  ruleDictionary.Add(rule.Name, productions.ToList());
    //  productions.Clear();
    //}

    //public CFGSymbol CreateProductionRule(String name, List<string> parts, List<CFGSymbol> symbols, GroupSymbol curRuleSymbolGroup) {
    //  if (parts.Count == symbols.Count) {
    //    parts.Add(String.Empty);
    //  }
    //  if (parts.Count - 1 != symbols.Count) {
    //    throw new ArgumentException("ParserError: Should not happen.");
    //  }
    //  CFGSymbol production = null;
    //  if (!curRuleSymbolGroup.SymbolsCollection.Any(x => x.Name.Equals(name))) {
    //    production = new CFGProduction(name, parts);
    //    curRuleSymbolGroup.SymbolsCollection.Add(production);
    //    treeGrammar.SetSubtreeCount(production, symbols.Count, symbols.Count);
    //    for (int i = 0; i < symbols.Count; i++) {
    //      treeGrammar.AddAllowedChildSymbol(production, symbols[i], i);
    //    }
    //  } else {
    //    production = (CFGSymbol)curRuleSymbolGroup.SymbolsCollection.First(x => x.Name.Equals(name));
    //    production.InitialFrequency++;
    //  }
    //  symbols.Clear();
    //  return production;
    //}

    //private static IList<string> splitToProcutionRules(string line) {
    //  List<int> positions = new List<int>();
    //  bool quoted = false;
    //  char quotationChar = '"'; // needs to be initialized for the compiler

    //  for (int i = 0; i < line.Length; i++) {
    //    char curChar = line.ElementAt(i);
    //    if (!quoted && curChar == PRODUCTIONSPLIT) {
    //      positions.Add(i);
    //      continue;
    //    }
    //    if (!quoted && QUOTATION.Contains(curChar)) {
    //      quotationChar = curChar;
    //      quoted = true;
    //      continue;
    //    }
    //    if (quoted && quotationChar == curChar) {
    //      quoted = false;
    //      continue;
    //    }
    //  }

    //  List<String> parts = new List<string>();
    //  int countPos = 0;
    //  foreach (var position in positions) {
    //    parts.Add(line.Substring(countPos, position - countPos));
    //    countPos = position + 1;
    //  }
    //  parts.Add(line.Substring(countPos, line.Length - countPos));
    //  return parts;
    //}

    //private bool IsRuleLine(string line) {
    //  int ruleSymbolIndex = line.IndexOf(RULESYMBOL);
    //  if (ruleSymbolIndex < 0) { return false; }

    //  foreach (char quotationchar in QUOTATION) {
    //    int quotationIndex = line.IndexOf(quotationchar);
    //    if (quotationIndex >= 0 && quotationIndex < ruleSymbolIndex) {
    //      return false;
    //    }
    //  }
    //  return true;
    //}

    //public CFGSymbol GetRule(String name) {
    //  name = "\"" + name + "\"";
    //  if (!symbolDictionary.ContainsKey(name)) {
    //    CFGSymbol rule = new CFGSymbol(name, 1);
    //    ruleSymbols.SymbolsCollection.Add(rule);
    //    symbolDictionary.Add(name, rule);
    //  }
    //  return symbolDictionary[name];
    //}
  }
}
