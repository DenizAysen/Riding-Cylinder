using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarketController : MonoBehaviour
{
    public static MarketController Current;
    public List<MarketItem> items;
    public List<Item> equippedItems;
    public GameObject marketMenu;

    public void InitializeMarketController()
    {
        Current = this;

        foreach(MarketItem item in items)
        {
            item.InitializeItem();
        }
    }

    public void ActivateMarketMenu(bool active)
    {
        if (active)
        {
            marketMenu.SetActive(true);
            active = false;
        }
        else
        {
            marketMenu.SetActive(false);
            active = false;
        }
        
    }
}
