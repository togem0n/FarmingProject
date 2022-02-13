using UnityEngine;

public class Item : MonoBehaviour
{
    [ItemCodeName]
    [SerializeField]
    private int _itemCode;

    private SpriteRenderer spriteRenderer;
    public int ItemCode { get => _itemCode; set => _itemCode = value; }

    private void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    private void Start()
    {
        if(ItemCode != 0)
        {
            Init(ItemCode);
        }
    }

    public void Init(int itemCode)
    {

    }
}
