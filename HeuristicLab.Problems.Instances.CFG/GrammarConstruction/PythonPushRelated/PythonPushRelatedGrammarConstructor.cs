using System.Collections.Generic;

namespace HeuristicLab.Problems.Instances.CFG {
  public class PythonPushRelatedGrammarConstructor : PythonGrammarConstructor {

    protected override string ArchiveName => "PythonPushRelated.PushRelated.zip";

    protected override Dictionary<string, List<DataType>> grammarMapping {
      get {
        return new Dictionary<string, List<DataType>>() {
          {"bool.bnf", new List<DataType>() {DataType.Boolean}},
          {"integer.bnf", new List<DataType>() {DataType.Integer}},
          {"float.bnf", new List<DataType>() {DataType.Float}},
          {"char.bnf", new List<DataType>() {DataType.Char}},
          {"string.bnf", new List<DataType>() {DataType.String}},
          {"list_bool.bnf", new List<DataType>() {DataType.ListBoolean}},
          {"list_integer.bnf", new List<DataType>() {DataType.ListInteger}},
          {"list_float.bnf", new List<DataType>() {DataType.ListFloat}},
          {"list_string.bnf", new List<DataType>() {DataType.ListString}}
        };
      }
    }

    protected override Dictionary<DataType, DataTypeMapping> dataTypeMapping {
      get {
        return new Dictionary<DataType, DataTypeMapping>() {
          {DataType.Boolean, new DataTypeMapping("<bool_var>", "<bool>", "b{0}", "bool()")},
          {DataType.Integer, new DataTypeMapping("<int_var>", "<int>", "i{0}", "int()")},
          {DataType.Float, new DataTypeMapping("<float_var>", "<float>", "f{0}", "float()")},
          {DataType.Char, new DataTypeMapping("<char_var>", "<char>", "c{0}", "' '")},
          {DataType.String, new DataTypeMapping("<string_var>", "<string>", "s{0}", "str()")},
          {DataType.ListBoolean, new DataTypeMapping("<list_bool_var>", "<list_bool>", "lb{0}", "[]")},
          {DataType.ListInteger, new DataTypeMapping("<list_int_var>", "<list_int>", "li{0}", "[]")},
          {DataType.ListFloat, new DataTypeMapping("<list_float_var>", "<list_float>", "lf{0}", "[]")},
          {DataType.ListString, new DataTypeMapping("<list_string_var>", "<list_string>", "ls{0}", "[]")}
        };
      }
    }
  }
}
