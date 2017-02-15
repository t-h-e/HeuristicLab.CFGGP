using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace HeuristicLab.Problems.Instances.CFG {
  /// <summary>
  /// Grammar Praser that produces a Grammar object which can be easily manipulated and provides extra functionallity over HL grammars
  /// </summary>
  public class GrammarParser {
    private const string rulePattern = @"(?<rulename><\S+>)\s*::=\s*(?<production>(?:(?=\/\/)\/\/[^\r\n]*|(?!<\S+>\s*::=).+?)+)";
    private const string productionPattern = @"(?=\/\/)(?:\/\/.*$)|(?!\/\/)\s*(?<production>(?:[^'""\|\/\/]+|'.*?'|"".*?"")+)";
    private const string ruleInProdPattern = @"\ *([\r\n]+)\ *|([^'""<\r\n]+)|'(.*?)'|""(.*?)""|(?<subrule><[^>|\s]+>)|([<]+)";

    private static Regex ruleRegex = new Regex(rulePattern, RegexOptions.Singleline);
    private static Regex productionRegex = new Regex(productionPattern, RegexOptions.Multiline);
    private static Regex ruleInProdRegex = new Regex(ruleInProdPattern);

    private GrammarParser() { }

    public static Dictionary<String, Grammar> ParseGrammarsByPath(string path) {
      Dictionary<String, Grammar> grammars = new Dictionary<string, Grammar>();
      foreach (var bnf in Directory.GetFiles(path).Where(f => f.EndsWith(".bnf"))) {
        grammars.Add(Path.GetFileName(bnf), ReadGrammarBNF(File.ReadAllText(bnf)));
      }
      return grammars;
    }

    public static Dictionary<String, Grammar> ParseGrammarsByEmbededArchive(string archiveName) {
      Dictionary<String, Grammar> grammars = new Dictionary<string, Grammar>();
      var instanceArchiveName = GetResourceName(archiveName);
      using (var instancesZipFile = new ZipArchive(Assembly.GetExecutingAssembly().GetManifestResourceStream(instanceArchiveName), ZipArchiveMode.Read)) {
        foreach (var bnf in instancesZipFile.Entries.Where(x => x.Name.EndsWith(".bnf"))) {
          using (var stream = new StreamReader(bnf.Open())) {
            grammars.Add(Path.GetFileName(bnf.Name), ReadGrammarBNF(stream.ReadToEnd()));
          }
        }
      }
      return grammars;
    }

    private static string GetResourceName(string fileName) {
      return Assembly.GetExecutingAssembly().GetManifestResourceNames()
              .Where(x => Regex.Match(x, @".*\.GrammarConstruction\." + fileName).Success).SingleOrDefault();
    }

    public static Grammar ReadGrammarBNF(String bnf) {
      Grammar grammar = null;
      List<string> productions = new List<string>();
      var rule = ruleRegex.Match(bnf);
      while (rule.Success) {
        string ruleName = rule.Groups["rulename"].Value;
        if (grammar == null) {
          grammar = new Grammar(ruleName);
        } else if (!grammar.Rules.ContainsKey(ruleName)) {
          grammar.Rules.Add(ruleName, new Rule() { Name = ruleName });
        }
        var production = productionRegex.Match(rule.Groups["production"].Value);
        while (production.Success) {
          string productionString = production.Groups["production"].Value.Trim();
          if (!String.IsNullOrWhiteSpace(productionString)) {

            List<string> parts = new List<string>();
            parts.Add(String.Empty);
            var subRule = ruleInProdRegex.Match(productionString);

            List<string> rulesInProduction = new List<string>();
            while (subRule.Success) {
              if (!String.IsNullOrEmpty(subRule.Groups["subrule"].Value)) {
                rulesInProduction.Add(subRule.Groups["subrule"].Value);
                subRule = subRule.NextMatch();
                parts.Add(String.Empty);
                continue;
              }

              var part = String.Join(String.Empty, Enumerable.Range(1, subRule.Groups.Count - 1).Select(x => subRule.Groups[x].Value));
              parts[parts.Count - 1] = parts[parts.Count - 1] + part;

              subRule = subRule.NextMatch();
            }

            grammar.Rules[ruleName].Productions.Add(new Production() { RuleDependency = rulesInProduction, Parts = parts });
          }
          production = production.NextMatch();
        }
        rule = rule.NextMatch();
      }
      return grammar;
    }
  }
}
