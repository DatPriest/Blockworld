using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToolBar : MonoBehaviour
{
    public UIItemSlot[] slots;
    public RectTransform highlight;
    public Player ply;
    public int slotIndex = 0;

    private void Start()
    {
        byte indexer = 1;
        foreach(UIItemSlot s in slots)
        {
            ItemStack stack = new ItemStack(indexer, Random.Range(2, 65))       ;
            ItemSlot slot = new ItemSlot(slots[indexer - 1], stack);
            indexer++;


        }

    }
    private void Update()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll != 0)
        {
            if (scroll > 0)
                slotIndex--;
            else
                slotIndex++;
            if (slotIndex > slots.Length - 1)
                slotIndex = 0;
            if (slotIndex < 0)
                slotIndex = slots.Length - 1;

            highlight.position = slots[slotIndex].slotIcon.transform.position;            
        }
    }

}
