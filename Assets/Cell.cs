using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Cell:MonoBehaviour
{
    public int x;
    public int y;
    [SerializeField] private Image occupant;

    public Cell(int x, int y)
    {
        this.x = x;
        this.y = y;
        ClearOccupation();
    }

    public void SetOccupation(Sprite sprite)
    {
        Debug.Log(occupant == null ? "occupant is null" : "occupant ok");
        occupant.gameObject.SetActive(true);
    }

    public void ClearOccupation()
    {

            occupant.gameObject.SetActive(false);
    }
}