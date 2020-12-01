using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Tile : MonoBehaviour, IPointerClickHandler
{
    public Color SelectedTint = Color.grey;
    public Image TintTile;
    public Image Face;

    GameField field;
    Vector2Int gridPos;

    public void SetFace(Sprite faceSprite) {
        Face.sprite = faceSprite;
    }

    public void SetSelected(bool selected) {
        TintTile.color = selected ? SelectedTint : Color.white;
    }

    public void SetData(GameField field, Vector2Int gridPos) {
        this.field = field;
        this.gridPos = gridPos;
    }

    public void OnPointerClick(PointerEventData eventData) {
        field.TilePressed(gridPos);
    }
}
