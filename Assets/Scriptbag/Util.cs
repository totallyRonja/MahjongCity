using System.Collections.Generic;
using UnityEngine;
public static class Util {
	public static void Shuffle<T>(this List<T> list) {
		for (int i = 0; i < list.Count; i++) {
			list.Swap(i, Random.Range(0, list.Count));
		}
	}

	public static void Swap<T>(this List<T> list, int indexA, int indexB) {
		T temp = list[indexA];
		list[indexA] = list[indexB];
		list[indexB] = temp;
	}
}
