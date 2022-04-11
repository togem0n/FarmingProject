using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : SingletonMonoBehaviour<InventoryManager>, ISaveable
{
    private Dictionary<int, ItemDetails> itemDetailsDictionary; // using itemCode to look up itemDetail

    [SerializeField] private ItemLibrary itemLibrary; // ItemLibrary

    [SerializeField] private UIInventoryBar inventoryBar;

    [SerializeField] private PlayerData data;

    private int selectedItemCode = -1;
    public int SelectedItemCode { get => selectedItemCode; set => selectedItemCode = value; }

    private int selectedItemIndex = -1;
    public int SelectedItemIndex { get => selectedItemIndex; set => selectedItemIndex = value; }

    private List<InventoryItem> inventoryList = new List<InventoryItem>(); // player's inventory list
    public List<InventoryItem> InventoryList { get => inventoryList; }

    private string iSaveableUniqueID;
    public string ISaveableUniqueID { get => iSaveableUniqueID; set => iSaveableUniqueID = value; }

    private GameObjectSave _gameObjectSave;
    public GameObjectSave GameObjectSave { get => _gameObjectSave; set => _gameObjectSave = value; }

    protected override void Awake()
    {
        base.Awake();

        ISaveableUniqueID = GetComponent<GenerateGUID>().GUID;

        GameObjectSave = new GameObjectSave();
    }

    private void OnEnable()
    {
        ISaveableRegister();
    }

    private void OnDisable()
    {
        ISaveableDeregister();
    }

    private void Start()
    {
        InitInventoryItemList();
        InitItemDetailsDictionary();
    }

    /// <summary>
    /// Init the itemDatilsDitionary from the sriptable object item list
    /// </summary>
    private void InitItemDetailsDictionary()
    {
        itemDetailsDictionary = new Dictionary<int, ItemDetails>();

        foreach (ItemDetails itemDetails in itemLibrary.itemDetailsLibrary)
        {
            itemDetailsDictionary.Add(itemDetails.itemCode, itemDetails);
        }
    }

    /// <summary>
    /// Init Inventory List with blank item with the size of current Inventory capacity
    /// </summary>
    public void InitInventoryItemList()
    {
        InventoryItem inventoryItem = new InventoryItem();

        inventoryItem.itemCode = 0;
        inventoryItem.itemQuantity = 0;

        for(int i = 0; i < data.currentInventoryCapacity; i++)
        {
            inventoryList.Add(inventoryItem);
        }
        PrintInventoryList(InventoryList);
    }

    public void AddItem(int itemCode, int itemQuantity, GameObject itemToDelete)
    {
        AddItem(itemCode, itemQuantity);
        
        Destroy(itemToDelete);
        
    }

    public void AddItem(int itemCode, int itemQuantity)
    {
        // Check if inventory already contains the item
        int itemIndex = GetItemIndexInInventory(itemCode);

        // if contain
        if (itemIndex != -1)
        {
            AddItemAtIndex(itemCode, itemIndex, itemQuantity);
        }
        // if not contain
        else
        {
            // check inventory full or not
            if (GetFirstBlankSlotIndex() != -1)
            {
                AddItemAtIndex(itemCode, GetFirstBlankSlotIndex(), itemQuantity);
            }
            else
            {
                return;
            }
        }
        // Send event that inventory has been updated
        EventHandler.CallInventoryUpdatedEvent(InventoryList);
        return;
    }

    /// <summary>
    /// Try to add an item to the invenotry
    /// return true if successful
    /// return false if inventory is full
    /// </summary>
    public bool TryAddItem(int itemCode, int itemQuantity)
    {
        // Check if inventory already contains the item
        int itemIndex = GetItemIndexInInventory(itemCode);

        // if contain
        if (itemIndex != -1)
        {
            return true;
            //AddItemAtIndex(itemCode, itemIndex, itemQuantity);
        }
        // if not contain
        else
        {
            // check inventory full or not
            if(GetFirstBlankSlotIndex() != -1)
            {
                return true;
                //AddItemAtIndex(itemCode, GetFirstBlankSlotIndex(), itemQuantity);
            }
            else
            {
                return false;
            }
        }

        // return true;
    }

    /// <summary>
    /// Add item to the inventory of a given quantity if item already exist;
    /// </summary>
    public void AddItemAtIndex(int itemCode, int itemIndex, int itemQuantity)
    {
        InventoryItem inventoryItem = new InventoryItem();

        inventoryItem.itemCode = itemCode;
        inventoryItem.itemQuantity = inventoryList[itemIndex].itemQuantity + itemQuantity;
        inventoryList[itemIndex] = inventoryItem;
        EventHandler.CallInventoryUpdatedEvent(InventoryList);
    }


    /// <summary>
    /// Remove number of item by 1
    /// if num of item is 0, replace by 0item
    /// </summary>
    public void RemoveItemByOneAtIndex(int itemIndex)
    {
        InventoryItem inventoryItem = new InventoryItem();

        int quantity = inventoryList[itemIndex].itemQuantity - 1;
        int itemCode = InventoryList[itemIndex].itemCode;

        if(quantity > 0)
        {
            inventoryItem.itemQuantity = quantity;
            inventoryItem.itemCode = itemCode;
            inventoryList[itemIndex] = inventoryItem;
        }
        else
        {
            inventoryItem.itemQuantity = 0;
            inventoryItem.itemCode = 0;
            inventoryList[itemIndex] = inventoryItem;
        }
        EventHandler.CallInventoryUpdatedEvent(InventoryList);
    }

    public void RemoveSelectedItemByOne()
    {
        InventoryItem inventoryItem = new InventoryItem();

        int quantity = inventoryList[SelectedItemIndex].itemQuantity - 1;
        int itemCode = InventoryList[SelectedItemIndex].itemCode;

        if (quantity > 0)
        {
            inventoryItem.itemQuantity = quantity;
            inventoryItem.itemCode = itemCode;
            inventoryList[SelectedItemIndex] = inventoryItem;
        }
        else
        {
            inventoryItem.itemQuantity = 0;
            inventoryItem.itemCode = 0;
            inventoryList[SelectedItemIndex] = inventoryItem;
            //ClearSelectedInventoryItem();
            inventoryBar.ClearHighLightOnInventorySlots();
        }
        EventHandler.CallInventoryUpdatedEvent(InventoryList);
    }

    /// <summary>
    /// Remove all items at index
    /// if num of item is 0, replace by 0item
    /// </summary>
    public void RemoveItemAtIndex(int itemIndex)
    {
        InventoryItem inventoryItem = new InventoryItem();

        inventoryItem.itemQuantity = 0;
        inventoryItem.itemCode = 0;
        inventoryList[itemIndex] = inventoryItem;
        
        EventHandler.CallInventoryUpdatedEvent(InventoryList);
    }

    public void SwapItem(int first, int second)
    {
        InventoryItem firstItem = new InventoryItem();
        firstItem.itemCode = InventoryList[first].itemCode;
        firstItem.itemQuantity = InventoryList[first].itemQuantity;

        InventoryItem secondItem = new InventoryItem();
        secondItem.itemCode = InventoryList[second].itemCode;
        secondItem.itemQuantity = InventoryList[second].itemQuantity;

        //InventoryItem tmpItem = new InventoryItem();
        //int tmpItemCode = firstItemCode;
        //int tmpItemQuantity = firstItemQuantity;

        inventoryList[first] = secondItem;
        inventoryList[second] = firstItem;
        EventHandler.CallInventoryUpdatedEvent(InventoryList);
    }


    /// <summary>
    /// return index if item in inventory list
    /// return -1 if not
    /// </summary>
    private int GetItemIndexInInventory(int itemCode)
    {
        for (int i = 0; i < InventoryList.Count; i++)
        {
            if (InventoryList[i].itemCode == itemCode && itemCode != 0)
            {
                return i;
            }
        }
        return -1;
    }

    /// <summary>
    /// find first blank slot's index
    /// return -1 if no blank slot
    /// </summary>
    /// <returns></returns>

    private int GetFirstBlankSlotIndex()
    {
        for(int i = 0; i < data.currentInventoryCapacity; i++)
        {
            if(inventoryList[i].itemCode == 0)
            {
                return i;
            }
        }
        return -1;
    }

    public ItemDetails GetItemDetails(int itemCode)
    {
        ItemDetails itemDetails;
        if (itemDetailsDictionary.TryGetValue(itemCode, out itemDetails))
        {
            return itemDetails;
        }
        else
        {
            return null;
        }
    }

    public ItemDetails GetSelectedItemDetails()
    {
        ItemDetails itemDetails;
        if (itemDetailsDictionary.TryGetValue(selectedItemCode, out itemDetails))
        {
            return itemDetails;
        }
        else
        {
            return null;
        }
    }


    public ItemDetails GetItemDetailsByLocation(int index)
    {
        int itemCode = GetItemCodeAtLocation(index);

        if(itemCode == -1)
        {
            return null;
        }
        else
        {
            return GetItemDetails(itemCode);
        }
    }

    public int GetItemCodeAtLocation(int index)
    {
        return inventoryList[index].itemCode;
    }

    public int GetItemQuantitiesAtLocation(int index)
    {
        return inventoryList[index].itemQuantity;
    }

    /// <summary>
    /// 
    /// </summary>

    public void SetSelectedInventoryItem(int itemCode, int index)
    {
        SelectedItemCode = itemCode;
        SelectedItemIndex = index;
    }

    public void ClearSelectedInventoryItem()
    {
        SelectedItemCode = -1;
        selectedItemIndex = -1;
    }

    public void PrintInventoryList(List<InventoryItem> inventoryList)
    {
        //Debug.Log(InventoryManager.Instance.InventoryList.Count);
        //foreach (InventoryItem inventoryItem in inventoryList)
        //{
        //    Debug.Log("Item Name: " + InventoryManager.Instance.GetItemDetails(inventoryItem.itemCode).itemName + "   Item Quantity: " + inventoryItem.itemQuantity);
        //}
        //Debug.Log("************************************************************************");
    }

    public void ISaveableRegister()
    {
        SaveLoadManager.Instance.iSaveableObjectList.Add(this);
    }

    public void ISaveableDeregister()
    {
        SaveLoadManager.Instance.iSaveableObjectList.Remove(this);
    }

    public GameObjectSave ISaveableSave()
    {
        SceneSave sceneSave = new SceneSave();

        GameObjectSave.sceneData.Remove(Settings.PersistentScene);

        sceneSave.listInventoryItem = InventoryList;

        GameObjectSave.sceneData.Add(Settings.PersistentScene, sceneSave);

        return GameObjectSave;
    }

    public void ISaveableLoad(GameSave gameSave)
    {
        if(gameSave.gameObjectData.TryGetValue(ISaveableUniqueID, out GameObjectSave gameObjectSave))
        {
            GameObjectSave = gameObjectSave;

            if(gameObjectSave.sceneData.TryGetValue(Settings.PersistentScene, out SceneSave sceneSave))
            {
                if(sceneSave.listInventoryItem != null)
                {
                    inventoryList = sceneSave.listInventoryItem;

                    EventHandler.CallInventoryUpdatedEvent(inventoryList);
                }

                //inventoryBar.ClearHighLightOnInventorySlots();
            }
        }
    }

    public void ISaveableStoreScene(string sceneName)
    {

    }

    public void ISaveableRestoreScene(string sceneName)
    {

    }
}
