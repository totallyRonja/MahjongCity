using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Scriptbag;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GameField : MonoBehaviour {

	public Tile TilePrefab;
	public Line Lines;
	public Sprite[] TileFaces;
	
	const char Nothing = '0';
	const char Tile = 'X';
	const char NewRow = '\n';
	
	const string IgnoreCharacters = "\r"; //dont compain about windows line breaks 

	TileInfo[,] tiles; //grid data. important, first index is the row, second one the column!
	Vector2Int gridSize;
	RectTransform ownTransform;
	Vector2 maxSize;
	Vector2 tileSize;

	static readonly Vector2Int NoSelection = new Vector2Int(-1, -1);
	Vector2Int SelectedTile = NoSelection;

	void Awake() {
		ownTransform = (RectTransform) transform;
		maxSize = ((RectTransform) ownTransform.parent).rect.size;
	}

	void Start() {
		var data = ParseLevelData();
		BuildField(data);
		
	}
	
	public void TilePressed(Vector2Int gridPos) {
		//deselect
		if (SelectedTile == gridPos) {
			tiles[gridPos.x, gridPos.y].UiTile.SetSelected(false);
			SelectedTile = NoSelection;
			return;
		}
		//select first
		if (SelectedTile == NoSelection) {
			SelectedTile = gridPos;
			tiles[gridPos.x, gridPos.y].UiTile.SetSelected(true);
			return;
		}
		//select second
		Vector2Int[] path = FindPathBetween(gridPos, SelectedTile);
		if (path != null) {
			Vector2[] worldPath = path.Select(pos => (Vector2)ownTransform.position - ownTransform.sizeDelta + pos * tileSize)
			.ToArray();
			Lines.SetLine(worldPath);
		}

		tiles[SelectedTile.x, SelectedTile.y].UiTile.SetSelected(false);
		SelectedTile = NoSelection;
	}

	void BuildField(List<List<bool>> data) {
		int width = data[0].Count;
		int height = data.Count;
		
		gridSize = new Vector2Int(width, height);
		var tileSize = ((RectTransform)TilePrefab.transform).sizeDelta;
		
		//adjust size
		var fieldSize = new Vector2(width * tileSize.x, height * tileSize.y);
		float scale = Mathf.Min(maxSize.x / fieldSize.x, maxSize.y / fieldSize.y);
		ownTransform.sizeDelta = fieldSize * scale;
		this.tileSize = tileSize *= scale;

		var facelessTiles = new List<TileInfo>();
		//generate tiles
		tiles = new TileInfo[width, height];
		for (int x = 0; x < width; x++) {
			for (int y = 0; y < height; y++) {
				//skip empty fields
				if (!data[y][x]) continue;
				//create game object tile
				var tile = GameObject.Instantiate(TilePrefab, transform);
				tile.SetData(this, new Vector2Int(x, y));
				var tileTransform = (RectTransform) tile.transform;
				tileTransform.sizeDelta *= scale;
				tileTransform.anchoredPosition = new Vector2(tileSize.x * x, tileSize.y * -y);
				//remember data
				tiles[x, y] = new TileInfo {
					FaceIndex = -1,
					UiTile = tile,
					GridPos = new Vector2Int(x, y),
				};
				facelessTiles.Add(tiles[x, y]);
			}
		}
		
		//generate face pairs
		for (var i = facelessTiles.Count - 1; i >= 0; i--) {
			var tile = facelessTiles[i];
			if(tile.FaceIndex >= 0)
				continue;
			int faceIndex = Random.Range(0, TileFaces.Length);
			if (facelessTiles.Count < 0) {
				throw new Exception("level data has uneven amount of tiles or something went wrong.");
			}

			List<int> randomIndices = Enumerable.Range(0, i).ToList();
			randomIndices.Shuffle();
			TileInfo sibling = tile;
			foreach (int index in randomIndices) {
				sibling = facelessTiles[index];
				//if we like the sibling, break out of the loop to use it
				if (sibling.FaceIndex < 0 && FindPathBetween(tile.GridPos, sibling.GridPos, true) != null) {
					break;
				}
			}

			tile.FaceIndex = faceIndex;
			tile.UiTile.SetFace(TileFaces[faceIndex]);
			sibling.FaceIndex = faceIndex;
			sibling.UiTile.SetFace(TileFaces[faceIndex]);
		}
	}

	Vector2Int[] FindPathBetween(Vector2Int fromTile, Vector2Int toTile, bool onlyFacelessBlock = false) {
		//check direct routes
		if (fromTile.x == toTile.x || fromTile.y == toTile.y) {
			var directPath = new Vector2Int[] {fromTile, toTile};
			if (IsPathFree(directPath, onlyFacelessBlock)) {
				return directPath;
			}
		}
		//around one corner
		var corner = new Vector2Int(fromTile.x, toTile.y);
		var cornerPath = new Vector2Int[] {fromTile, corner, toTile};
		if (IsPathFree(cornerPath, onlyFacelessBlock)) {
			return cornerPath;
		}
		corner = new Vector2Int(toTile.x, fromTile.y);
		cornerPath = new Vector2Int[] {fromTile, corner, toTile};
		if (IsPathFree(cornerPath, onlyFacelessBlock)) {
			return cornerPath;
		}
		//around two corners
		Vector2Int diffDir = new Vector2Int(Math.Sign(toTile.x - fromTile.x), Math.Sign(toTile.y - fromTile.y));
		//first check inbetween in x
		for (int x = fromTile.x + diffDir.x; x != toTile.x; x += diffDir.x) {
			Vector2Int corner1 = new Vector2Int(x, fromTile.y);
			Vector2Int corner2 = new Vector2Int(x, toTile.y);
			var path = new Vector2Int[] {fromTile, corner1, corner2, toTile};
			if (IsPathFree(path, onlyFacelessBlock)) {
				return cornerPath;
			}
		}
		//then check inbetween in y
		for (int y = fromTile.y + diffDir.y; y != toTile.y; y += diffDir.y) {
			Vector2Int corner1 = new Vector2Int(fromTile.x, y);
			Vector2Int corner2 = new Vector2Int(toTile.x, y);
			var path = new Vector2Int[] {fromTile, corner1, corner2, toTile};
			if (IsPathFree(path, onlyFacelessBlock)) {
				return cornerPath;
			}
		}
		//then check left
		for (int x = Mathf.Min(fromTile.x, toTile.x) - 1; x >= -1; x--) {
			Vector2Int corner1 = new Vector2Int(x, fromTile.y);
			Vector2Int corner2 = new Vector2Int(x, toTile.y);
			var path = new Vector2Int[] {fromTile, corner1, corner2, toTile};
			if (IsPathFree(path, onlyFacelessBlock)) {
				return cornerPath;
			}
		}
		//then check right
		for (int x = Mathf.Max(fromTile.x, toTile.x) + 1; x <= gridSize.x; x++) {
			Vector2Int corner1 = new Vector2Int(x, fromTile.y);
			Vector2Int corner2 = new Vector2Int(x, toTile.y);
			var path = new Vector2Int[] {fromTile, corner1, corner2, toTile};
			if (IsPathFree(path, onlyFacelessBlock)) {
				return cornerPath;
			}
		}
		//then check down
		for (int y = Mathf.Min(fromTile.y, toTile.y) - 1; y >= -1; y--) {
			Vector2Int corner1 = new Vector2Int(fromTile.x, y);
			Vector2Int corner2 = new Vector2Int(toTile.x, y);
			var path = new Vector2Int[] {fromTile, corner1, corner2, toTile};
			if (IsPathFree(path, onlyFacelessBlock)) {
				return cornerPath;
			}
		}
		//then check up
		for (int y = Mathf.Max(fromTile.y, toTile.y) + 1; y <= gridSize.y; y++) {
			Vector2Int corner1 = new Vector2Int(fromTile.x, y);
			Vector2Int corner2 = new Vector2Int(toTile.x, y);
			var path = new Vector2Int[] {fromTile, corner1, corner2, toTile};
			if (IsPathFree(path, onlyFacelessBlock)) {
				return cornerPath;
			}
		}
		//if all of that is blocked, return null
		return null;
	}

	bool IsPathFree(Vector2Int[] path, bool onlyFacelessBlock = false) {
		if (path == null)
			return false;
		
		for (int i = 1; i < path.Length; i++) {
			Vector2Int from = path[i-1];
			Vector2Int to = path[i];
			if (from.x != to.x && from.y != to.y) {
				Debug.LogError("invalid path!");
				return false;
			}
			Vector2Int diffDir = new Vector2Int(Math.Sign(to.x - from.x), Math.Sign(to.y - from.y));
			Vector2Int walkLocation = from + diffDir;
			while (walkLocation != to) {
				//tiles outside of the grid are never blocked
				if (walkLocation.x >= 0 && walkLocation.y >= 0 && walkLocation.x < gridSize.x && walkLocation.y < gridSize.y) {
					//if we find a blocking tile, return false
					var tile = tiles[walkLocation.x, walkLocation.y];
					if (tile != null && (tile.FaceIndex < 0 || !onlyFacelessBlock)) {
						return false;
					}
				}

				//otherwise keep walking
				walkLocation += diffDir;
			}
		}
		//if nothing blocked us, its a free path
		return true;
	}

	List<List<bool>> ParseLevelData() {
		//setup
		var data = new List<List<bool>>();
		data.Add(new List<bool>());
		string levelAsset = LevelSystem.CurrentLevelData.text;
		//walk through the file char by char
		int row = 0;
		foreach (char character in levelAsset) {
			switch (character) {
				case NewRow:
					//when we encounter a newline we go to the next row
					row++;
					data.Add(new List<bool>());
					break;
				case Nothing:
					data[row].Add(false);
					break;
				case Tile:
					data[row].Add(true);
					break;
				default:
					if(IgnoreCharacters.Contains(""+character)) break;
					Debug.LogWarning($"Non-Normative character '{character}' in level data (will be ignored)");
					break;
			}
		}
		//validate
		int width = data[0].Count;
		if(data.Any(list => list.Count != width))
			Debug.LogError("Invalid file!");
		if(data.Count == 0 || data[0].Count == 0)
			Debug.LogError("Empty file!");

		return data;
	}
}

public class TileInfo {
	public Vector2Int GridPos;
	public int FaceIndex;
	public Tile UiTile;
}