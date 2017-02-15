using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HeuristicLab.Problems.Instances.CFG {
  public class Grammar {
    private string root;
    public string Root { get { return root; } }
    public Dictionary<String, Rule> Rules { get; set; }
    public Grammar(string root) {
      this.root = root;
      Rules = new Dictionary<string, Rule>();
      Rules.Add(this.root, new Rule() { Name = this.root });
    }

    /// <summary>
    /// Removes rules without productions and removes productions which rely on rules which are not available.
    /// 
    /// Should only be called before printing the grammar.
    /// </summary>
    public void TrimGrammar(bool removeUnitProductions) {
      bool change = true;
      while (change) {
        change = false;
        // every rule has to have a production
        // and
        // a rule has to be Root or has to be required (RuleDependency) by a production that is not its own
        // does not remove independent cycles in the grammar
        var simplifiedRules = Rules.Where(x => x.Value.Productions.Count > 0 &&
                                          (x.Key == Root || Rules.Any(y => y.Key != x.Key && y.Value.Productions.Any(p => p.RuleDependency.Contains(x.Key)))))
                                   .ToDictionary(x => x.Key, y => y.Value);
        if (Rules.Count != simplifiedRules.Count) {
          Rules = simplifiedRules;
          change = true;
        }

        var ruleNames = Rules.Select(x => x.Key);
        foreach (var r in Rules) {
          var simplifiedProductions = r.Value.Productions.Where(x => !x.RuleDependency.Except(ruleNames).Any()).ToList();
          if (r.Value.Productions.Count != simplifiedProductions.Count) {
            r.Value.Productions = simplifiedProductions;
            change = true;
          }
        }
      }

      if (removeUnitProductions) {
        var unitProductionRules = Rules.Where(x => x.Key != Root && x.Value.Productions.Count == 1);
        foreach (var upr in unitProductionRules) {
          var dependentProductions = Rules.SelectMany(x => x.Value.Productions.Where(p => p.RuleDependency.Contains(upr.Key)));
          foreach (var p in dependentProductions) {
            var i = p.RuleDependency.IndexOf(upr.Key);
            var production = upr.Value.Productions[0];
            while (i >= 0) {
              var newRuleDependency = p.RuleDependency.Take(i).ToList();
              newRuleDependency.AddRange(production.RuleDependency);
              newRuleDependency.AddRange(p.RuleDependency.Skip(i + 1));
              p.RuleDependency = newRuleDependency;

              var newParts = p.Parts.Take(i + 1).ToList();
              newParts[newParts.Count - 1] = newParts[newParts.Count - 1] + production.Parts.First();
              newParts.AddRange(production.Parts.Skip(1));
              newParts[newParts.Count - 1] = newParts[newParts.Count - 1] + p.Parts.Skip(i + 1).First();
              newParts.AddRange(p.Parts.Skip(i + 2));
              p.Parts = newParts;

              i = p.RuleDependency.IndexOf(upr.Key);
            }
          }
        }
        var removeRules = unitProductionRules.Select(x => x.Key);
        Rules = Rules.Where(x => !removeRules.Contains(x.Key)).ToDictionary(x => x.Key, y => y.Value);
      }
    }

    public void Combine(Grammar cur) {
      foreach (var rule in cur.Rules) {
        if (!Rules.ContainsKey(rule.Key)) {
          Rules.Add(rule.Key, rule.Value);
        } else {
          var existing = Rules[rule.Key];
          foreach (var p in rule.Value.Productions) {
            existing.Productions.Add(p);
          }
        }
      }
    }

    public string PrintGrammar() {
      StringBuilder strBuilder = new StringBuilder();
      strBuilder.AppendLine(Rules[Root].ToString());
      foreach (var rule in Rules.Values.Where(r => r.Name != Root)) {
        strBuilder.AppendLine(rule.ToString());
      }
      return strBuilder.ToString();
    }
  }

  public class Rule {
    public string Name { get; set; }
    public List<Production> Productions { get; set; }
    public Rule() {
      Productions = new List<Production>();
    }

    public override string ToString() {
      return String.Format("{0} ::= {1}", Name, String.Join("|", Productions));
    }
  }

  public class Production {
    public List<string> Parts { get; set; }
    public List<string> RuleDependency { get; set; }
    public Production() {
      RuleDependency = new List<string>();
      Parts = new List<string>();
    }

    public override string ToString() {
      if (Parts.Count == 1 && String.IsNullOrEmpty(Parts[0])) return "''"; //empty rule
      StringBuilder strBuilder = new StringBuilder();
      var partsEnumerator = Parts.GetEnumerator();
      var ruleDependencyEnumerator = RuleDependency.GetEnumerator();
      while (partsEnumerator.MoveNext() && ruleDependencyEnumerator.MoveNext()) {
        strBuilder.Append(Quote(partsEnumerator.Current));
        strBuilder.Append(ruleDependencyEnumerator.Current);
      }
      strBuilder.Append(Quote(partsEnumerator.Current));
      return strBuilder.ToString();
    }

    private string Quote(string part) {
      if (String.IsNullOrEmpty(part)) return part;
      if (!part.Contains('\'')) {
        return Quote(part, '\'');
      } else {
        return Quote(part, '"');
      }
    }

    private string Quote(string part, char quote) {
      part = part.Replace(Environment.NewLine, String.Format("{0}{1}{0}", quote, Environment.NewLine));
      part = String.Format("{0}{1}{0}", quote, part);
      part = part.Replace(String.Format("{0}{0}", quote), String.Empty);
      return part;
    }
  }
}
