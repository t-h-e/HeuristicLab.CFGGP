using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HeuristicLab.Problems.Instances.CFG {
  public class PythonGrammarConstructor {

    private const string Structure = "structure.bnf";
    private const string StructureTree = "structure_tree.bnf";
    private const string RootRule = "<predefined>";

    private const string ArchiveName = "Python.Python.zip";

    private const string OutputPrefix = "res";
    private const string InputPrefix = "in";

    private Dictionary<string, List<DataType>> grammarMapping = new Dictionary<string, List<DataType>>() {
      { "bool.bnf", new List<DataType>() { DataType.Boolean } },
      { "integer.bnf", new List<DataType>() { DataType.Integer } },
      { "float.bnf", new List<DataType>() { DataType.Float } },
      { "string.bnf", new List<DataType>() { DataType.String } },
      { "list_bool.bnf", new List<DataType>() { DataType.ListBoolean } },
      { "list_integer.bnf", new List<DataType>() { DataType.ListInteger } },
      { "list_float.bnf", new List<DataType>() { DataType.ListFloat } },
      { "list_string.bnf", new List<DataType>() { DataType.ListString } }
    };

    private class DataTypeMapping {
      public string VariableRule { get; }
      public string Rule { get; }
      public string Variable { get; }
      public string Type { get; }
      public DataTypeMapping(string variableRule, string rule, string variable, string type) {
        VariableRule = variableRule;
        Rule = rule;
        Variable = variable;
        Type = type;
      }
    }

    private Dictionary<DataType, DataTypeMapping> dataTypeMapping = new Dictionary<DataType, DataTypeMapping>() {
      {DataType.Boolean,     new DataTypeMapping("<bool_var>",        "<bool>",        "b{0}", "bool()")},
      {DataType.Integer,     new DataTypeMapping("<int_var>",         "<int>" ,        "i{0}", "int()")},
      {DataType.Float,       new DataTypeMapping("<float_var>",       "<float>",       "f{0}", "float()")},
      {DataType.String,      new DataTypeMapping("<string_var>",      "<string>",      "s{0}", "str()")},
      {DataType.ListBoolean, new DataTypeMapping("<list_bool_var>",   "<list_bool>",   "lb{0}", "[]")},
      {DataType.ListInteger, new DataTypeMapping("<list_int_var>",    "<list_int>",    "li{0}", "[]")},
      {DataType.ListFloat,   new DataTypeMapping("<list_float_var>",  "<list_float>",  "lf{0}", "[]")},
      {DataType.ListString,  new DataTypeMapping("<list_string_var>", "<list_string>" ,"ls{0}", "[]")}
    };

    public PythonGrammarConstructor() { }

    public Grammar CombineDataTypes(Options options) {
      // parse grammars again to make sure they have not been changed in the meantime
      Dictionary<string, Grammar> pythonGrammars = GrammarParser.ParseGrammarsByEmbededArchive(ArchiveName);
      var grammar = options.Tree ? pythonGrammars[StructureTree] : pythonGrammars[Structure];

      var combinations = GetCombinations(options.Datatypes);
      foreach (var grammarName in combinations) {
        var cur = pythonGrammars[grammarName];
        grammar.Combine(cur);
      }

      var tempVariables = GetTempVariables(options.Datatypes, options.NumberOfInputVariables);
      var inputVariables = GetVariables(options.Input, InputPrefix);
      var outputVariables = GetVariables(options.Output, OutputPrefix);

      // add initialisation to variables
      foreach (var p in grammar.Rules[grammar.Root].Productions) {
        p.Parts[0] = tempVariables.Item1 + outputVariables.Item1 + p.Parts[0];
      }

      // add temp variables
      var tempVariableNames = tempVariables.Item2;
      foreach (var item in tempVariableNames) {
        var p = grammar.Rules[dataTypeMapping[item.Key].VariableRule].Productions;
        foreach (var v in item.Value) {
          p.Add(new Production() { Parts = new List<string>() { v } });
        }
      }

      // add output variables
      var outputVariableNames = outputVariables.Item2;
      foreach (var item in outputVariableNames) {
        var p = grammar.Rules[dataTypeMapping[item.Key].VariableRule].Productions;
        foreach (var v in item.Value) {
          p.Add(new Production() { Parts = new List<string>() { v } });
        }
      }

      // add input variables
      var inputVariableNames = inputVariables.Item2;
      foreach (var item in inputVariableNames) {
        var p = grammar.Rules[dataTypeMapping[item.Key].VariableRule].Productions;
        foreach (var v in item.Value) {
          p.Add(new Production() { Parts = new List<string>() { v } });
        }
      }

      // Remove types that might have been added, but that are not set as data types
      // will be completely removed after trimming
      foreach (var dataType in Enum.GetValues(typeof(DataType)).Cast<DataType>().Except(options.Datatypes)) {
        if (grammar.Rules.ContainsKey(dataTypeMapping[dataType].Rule)) grammar.Rules.Remove(dataTypeMapping[dataType].Rule);
      }

      return grammar;
    }

    private Tuple<string, Dictionary<DataType, List<string>>> GetTempVariables(IEnumerable<DataType> dataTypes, int initialeVariableCount) {
      if (initialeVariableCount <= 0) {
        return new Tuple<string, Dictionary<DataType, List<string>>>(String.Empty, new Dictionary<DataType, List<string>>());
      }
      var distinctDTs = dataTypes.Distinct();
      var variableNames = new Dictionary<DataType, List<string>>();
      StringBuilder strBuilder = new StringBuilder();
      foreach (var dt in distinctDTs) {
        List<string> varList = new List<string>();
        for (int i = 0; i < initialeVariableCount; i++) {
          varList.Add(String.Format(dataTypeMapping[dt].Variable, i));
        }
        variableNames.Add(dt, varList);
        strBuilder.AppendLine(String.Join("; ", varList.Select(x => String.Format("{0} = {1}", x, dataTypeMapping[dt].Type))));
      }
      return new Tuple<string, Dictionary<DataType, List<string>>>(strBuilder.ToString(), variableNames);
    }

    private Tuple<string, Dictionary<DataType, List<string>>> GetVariables(IEnumerable<DataType> variable, string prefix) {
      List<string> varList = new List<string>();
      var variableNames = new Dictionary<DataType, List<string>>();
      int i = 0;
      foreach (var v in variable) {
        if (!variableNames.ContainsKey(v)) { variableNames.Add(v, new List<string>()); }
        variableNames[v].Add(prefix + i);
        varList.Add(String.Format("{0}{1} = {2}", prefix, i, dataTypeMapping[v].Type));
        i++;
      }
      return new Tuple<string, Dictionary<DataType, List<string>>>(String.Join("; ", varList) + Environment.NewLine, variableNames);
    }

    private IEnumerable<string> GetCombinations(IEnumerable<DataType> dataTypes) {
      var requiredTypes = dataTypes.SelectMany(x => x.Requires()).Distinct().Except(dataTypes).ToList();
      if (requiredTypes.Any()) {
        System.Console.WriteLine("Some data types are required due to your selection and have been added");
        foreach (var dt in requiredTypes) {
          System.Console.WriteLine(dt);
        }
        requiredTypes.AddRange(dataTypes);
        dataTypes = requiredTypes;
      }
      return grammarMapping.Where(x => x.Value.All(y => dataTypes.Contains(y))).Select(x => x.Key);
    }
  }
}
