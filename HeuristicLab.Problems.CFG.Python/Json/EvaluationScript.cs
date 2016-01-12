using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace HeuristicLab.Problems.CFG.Python {
  public class EvaluationScript {

      [JsonProperty("id")]
      public int Id { get; set; }

      [JsonProperty("script")]
      public string Script { get; set; }

      [JsonProperty("variables")]
      public IList<string> Variables { get; set; }
  }
}
