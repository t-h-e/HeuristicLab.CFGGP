﻿#region License Information
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

using System.Collections.Generic;
using System.Text;
using HeuristicLab.Random;

namespace HeuristicLab.Problems.Instances.CFG {
  public static class StringValueGenerator {

    public static readonly char[] vowel = new char[] { 'a', 'e', 'i', 'o', 'u' };

    // https://en.wikipedia.org/wiki/ASCII#ASCII_printable_characters
    // characters used
    // 9 \t
    // 10 \n
    // 32 space
    // 33-126 other printable characters

    /// <summary>
    /// length is inclusive minLength and inclusive maxLength
    /// </summary>
    public static IEnumerable<string> GetRandomStrings(int n, int minLength, int maxLengthInclusive, FastRandom rand) {
      for (int i = 0; i < n; i++) {
        yield return StringValueGenerator.GetRandomString(rand.Next(minLength, maxLengthInclusive + 1), rand); // +1 , because rand.Next is exclusive upperBound
      }
    }

    public static string GetRandomString(int length, FastRandom rand) {
      StringBuilder strBuilder = new StringBuilder(length);
      for (int i = 0; i < length; i++) {
        strBuilder.Append(GetRandomChar(rand));
      }
      return strBuilder.ToString();
    }

    private static char GetRandomChar(FastRandom rand) {
      int value = rand.Next(0, 96 + 1); // +1 , because rand.Next is exclusive upperBound
      switch (value) {
        case 0:
          return '\t';
        case 1:
          return '\n';
        default:
          return (char)(value + 30);
      }
    }

    public static IEnumerable<string> GetRandomStringsWithoutSpaces(int n, int minLength, int maxLength, FastRandom rand) {
      for (int i = 0; i < n; i++) {
        yield return StringValueGenerator.GetRandomStringWithoutSpaces(rand.Next(minLength, maxLength + 1), rand); // +1 , because rand.Next is exclusive upperBound
      }
    }

    public static string GetRandomStringWithoutSpaces(int length, FastRandom rand) {
      StringBuilder strBuilder = new StringBuilder(length);
      for (int i = 0; i < length; i++) {
        strBuilder.Append(GetRandomCharWithoutSpace(rand));
      }
      return strBuilder.ToString();
    }

    public static char GetRandomCharWithoutSpace(FastRandom rand) {
      int value = rand.Next(0, 95 + 1); // +1 , because rand.Next is exclusive upperBound
      switch (value) {
        case 0:
          return '\t';
        case 1:
          return '\n';
        default:
          return (char)(value + 31);
      }
    }

    public static string GetRandomStringWithOnlyPrintableCharactersWithoutUpperCaseCharacters(int length, FastRandom rand) {
      StringBuilder strBuilder = new StringBuilder(length);
      for (int i = 0; i < length; i++) {
        strBuilder.Append(GetRandomPrintableCharExceptUpperCase(rand));
      }
      return strBuilder.ToString();
    }

    private static char GetRandomPrintableCharExceptUpperCase(FastRandom rand) {
      int value = rand.Next(0, 68 + 1) + 32; // +1 , because rand.Next is exclusive upperBound
      if (value < 65) return (char)value;
      return (char)(value + 26);
    }

    public static string GetRandomLowerCaseString(int length, FastRandom rand) {
      StringBuilder strBuilder = new StringBuilder(length);
      for (int i = 0; i < length; i++) {
        strBuilder.Append(GetRandomLowerCaseChar(rand));
      }
      return strBuilder.ToString();
    }

    public static char GetRandomLowerCaseChar(FastRandom rand) {
      int value = rand.Next(0, 25 + 1); // +1 , because rand.Next is exclusive upperBound
      return (char)(value + 97);
    }
  }
}
