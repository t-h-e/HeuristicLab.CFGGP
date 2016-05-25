    int steps = 0;
      int swaps = 65;

      int LONGSIZE = 64;
      int N_SORTING_NETWORK = 12;

      long fitnessCases = (long)Math.Pow(2, N_SORTING_NETWORK);
      int arrayLength = (int)(fitnessCases / LONGSIZE);
      arrayLength = (fitnessCases % LONGSIZE) != 0
              ? arrayLength + 1
              : arrayLength;

      // create data
      if (Input.ActualValue == null || Output.ActualValue == null
        || ((LongMatrix)Input.ActualValue).Rows != N_SORTING_NETWORK || ((LongMatrix)Input.ActualValue).Columns != arrayLength
        || ((LongMatrix)Output.ActualValue).Rows != N_SORTING_NETWORK || ((LongMatrix)Output.ActualValue).Columns != arrayLength) {
        long[,] helpInputs;
        long[,] helpOutputs;

        helpInputs = new long[N_SORTING_NETWORK, arrayLength];
        helpOutputs = new long[N_SORTING_NETWORK, arrayLength];

        for (long i = 0; i < fitnessCases; i++) {
          int arrayPos = (int)(i / LONGSIZE);
          int bitPos = (int)(i % LONGSIZE);

          for (int j = 0; j < N_SORTING_NETWORK; j++) {
            helpInputs[j, arrayPos] |= ((i >> j) & 1L) << bitPos;
          }

          long help = i;
          help = help - ((help >> 1) & 0x5555555555555555);
          help = (help & 0x3333333333333333) + ((help >> 2) & 0x3333333333333333);
          int bitCount = (int)((((help + (help >> 4)) & 0xF0F0F0F0F0F0F0F) * 0x101010101010101) >> 56);


          for (int j = N_SORTING_NETWORK - bitCount; j < N_SORTING_NETWORK; j++) {
            helpOutputs[j, arrayPos] |= 1L << bitPos;
          }
        }
        Input.Value = new LongMatrix(helpInputs);
        Output.Value = new LongMatrix(helpOutputs);
      }

      LongMatrix inputs = (LongMatrix)Input.ActualValue.Clone();
      LongMatrix outputs = (LongMatrix)Output.ActualValue.Clone();

      string phenotype = CFGSymbolicExpressionTreeStringFormatter.StaticFormat(Program.ActualValue);

      // minimize sorting network from phenotype
      string[] split = phenotype.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
      int[] temp = new int[split.Length];
      int pos = 0;
      for (int i = 0; i < split.Length; i += 2) {
        int first = int.Parse(split[i]);
        int second = int.Parse(split[i + 1]);
        if (first == second) {
          continue;
        }
        if (pos >= 2 && temp[pos - 2] == first && temp[pos - 1] == second) {
          continue;
        }
        temp[pos] = first;
        temp[pos + 1] = second;
        pos += 2;

      }
      int[] sortingNetwork = new int[pos];
      Array.Copy(temp, sortingNetwork, pos);


      // copy porblem data
      LongMatrix sorting = inputs;


      // execute sorting network
      int curswap = 1;
      for (int i = 0; i < sortingNetwork.Length; i += 2) {
        int firstLine = sortingNetwork[i];
        int secondLine = sortingNetwork[i + 1];

        //Todo: check time improvement if sorting[firstLine] and sorting[secondLine] are set to variables
        for (int j = 0; j < sorting.Columns; j++) {
          long max = sorting[firstLine, j] & sorting[secondLine, j];
          long min = sorting[firstLine, j] | sorting[secondLine, j];
          sorting[firstLine, j] = min;
          sorting[secondLine, j] = max;
        }
        steps++;
        curswap++;
        if (curswap > swaps) {
          break;
        }
      }


      // calculate fitness cases
      for (int i = 0; i < outputs.Rows; i++) {
        for (int j = 0; j < outputs.Columns; j++) {
          sorting[i, j] ^= outputs[i, j];
        }
      }

      // starts at 1
      for (int i = 1; i < sorting.Rows; i++) {
        for (int j = 0; j < sorting.Columns; j++) {
          sorting[0, j] |= sorting[i, j];
        }
      }

      double fitness = 0;
      bool[] cases = new bool[fitnessCases];
      for (int j = 0; j < sorting.Columns; j++) {
        int k = 0;
        while ((j + 1 >= sorting.Columns && k < fitnessCases % LONGSIZE)
          || (j + 1 < sorting.Columns && k < LONGSIZE)) {
          cases[j * LONGSIZE + k] = (sorting[0, j] & (1 << k)) != 0;
          if (cases[j * LONGSIZE + k]) fitness++;

          k++;
        }
      }

      Cases.ActualValue = new BoolArray(cases);
	  CaseQualities.ActualValue = new DoubleArray(cases.Select(x => x ? 0.0 : 1.0).ToArray());
      Quality.ActualValue = new DoubleValue(fitness + 0.01 * (curswap - 1));