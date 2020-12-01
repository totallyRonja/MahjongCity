using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Scriptbag {
	public static class SaveSystem {

		static List<string> solvedLevels;
		const char Separator = ';';
	
		public static bool IsSolved(string levelKey) {
			Load();
			return solvedLevels.Contains(levelKey);
		}
		
		public static void SetSolved(string levelKey, bool solved = true) {
			if (solved) {
				//add unique
				if (!solvedLevels.Contains(levelKey)) {
					solvedLevels.Add(levelKey);
				}
			} else {
				//remove (if doesnt exist its still happy and returns false)
				solvedLevels.Remove(levelKey);
			}

			Save();
		}

		public static void Reset() {
			solvedLevels.Clear();
			Save();
		}

		public static void Load() {
			string solvedEncoded = PlayerPrefs.GetString("SolvedLevels", "");
			solvedLevels = solvedEncoded.Split(Separator).ToList();
			//remove edge case from our simplistic parsing
			if (solvedLevels.Count == 1 && solvedLevels[0] == "") {
				solvedLevels.Clear();
			}
		}
		
		public static void Save() {
			string solvedEncoded = string.Join(""+Separator, solvedLevels);
			PlayerPrefs.SetString("SolvedLevels", solvedEncoded);
		}
	}
}