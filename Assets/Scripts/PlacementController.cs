using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class PlacementController : MonoBehaviour
{

    [SerializeField] private Camera mainCamera;
    [SerializeField] private Tilemap tilemap;
    [SerializeField] private GameObject _prefabImage;
    [SerializeField] private SpriteRenderer prefabSR;
    [SerializeField] private Manager manger;

    [SerializeField] private InputAction clickAction;

    private Vector3Int cellPosition;

    private void OnEnable()
    {
        clickAction.Enable();
    }
    private void OnDisable()
    {
        clickAction.Disable();
    }

    public void FixedUpdate()
    {

        Vector2 hoverPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        cellPosition = tilemap.WorldToCell(hoverPos);
        Vector3 prefabPos = tilemap.GetCellCenterWorld(cellPosition);
        _prefabImage.transform.position = prefabPos;
        if (manger.liveCells.Contains(new Vector2(cellPosition.x, cellPosition.y)))
        {
            prefabSR.color = new Color(96, 96, 96, 1f);
        }
        else
        {
            prefabSR.color = new Color(255, 255, 255, 1f);
        }   



    }
    public void ButtonIsPressed(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (manger.liveCells.Contains(new Vector2(cellPosition.x, cellPosition.y)))
            {
                manger.liveCells.Remove(new Vector2(cellPosition.x, cellPosition.y));
                tilemap.SetTile(cellPosition, null);
            }
            else
            {
                manger.liveCells.Add(new Vector2(cellPosition.x, cellPosition.y));
                tilemap.SetTile(cellPosition, manger.cellPrefab);
            }
        }
    }
}
