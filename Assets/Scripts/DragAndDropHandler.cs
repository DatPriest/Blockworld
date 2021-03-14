using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class DragAndDropHandler : MonoBehaviour
{
    [SerializeField]
    private UIItemSlot cursorSlot = null;
    private ItemSlot cursorItemSlot;

    [SerializeField]
    private GraphicRaycaster m_Raycaster = null;
    private PointerEventData m_PointerEventData;
    [SerializeField] private EventSystem m_EventSystem = null;

    World world;

    private void Start ()
    {
        world = GameObject.Find("World").GetComponent<World>();

        cursorItemSlot = new ItemSlot(cursorSlot);
    }

    private void Update()
    {
        if (!world.inUI)
            return;

        cursorSlot.transform.position = Input.mousePosition;

        if (Input.GetMouseButtonDown(0)) {
            HandleSlotClick(CheckForSlot());
        }
    }

    private void HandleSlotClick (UIItemSlot clickedSlot)
    {
        if (clickedSlot == null)
            return;

        if (!cursorSlot.HasItem && !clickedSlot.HasItem)
            return;

        if (clickedSlot.itemSlot.isCreative)
        {
            cursorItemSlot.EmptySlot();
            cursorItemSlot.InsertStack(clickedSlot.itemSlot.stack);
        }

        if (!cursorSlot.HasItem && clickedSlot.HasItem)
        {
            cursorItemSlot.InsertStack(clickedSlot.itemSlot.TakeAll());
            return;
        }

        if (cursorSlot.HasItem && !clickedSlot.HasItem)
        {
            clickedSlot.itemSlot.InsertStack(cursorItemSlot.TakeAll());
        }

        if (cursorItemSlot.HasItem && clickedSlot.HasItem)
        {
            if (cursorSlot.itemSlot.stack.id != clickedSlot.itemSlot.stack.id)
            {
                ItemStack oldCursorSlot = cursorSlot.itemSlot.TakeAll();
                ItemStack oldSlot = clickedSlot.itemSlot.TakeAll();

                clickedSlot.itemSlot.InsertStack(oldCursorSlot);
                cursorItemSlot.InsertStack(oldSlot);
            } else
            {                
                int maxStack = world.blockTypes[cursorItemSlot.stack.id].maxStackSize;
                if ((clickedSlot.itemSlot.stack.amount + cursorItemSlot.stack.amount) >= maxStack)
                {
                    if (clickedSlot.itemSlot.stack.amount == maxStack)
                    {
                        int leftAmount = clickedSlot.itemSlot.stack.amount + cursorItemSlot.stack.amount - maxStack;
                        cursorItemSlot.InsertStack(new ItemStack(cursorItemSlot.stack.id, maxStack));
                        clickedSlot.itemSlot.stack.amount = leftAmount;
                        clickedSlot.itemSlot.InsertStack(clickedSlot.itemSlot.TakeAll());
                    } else
                    {
                        int leftAmount = clickedSlot.itemSlot.stack.amount + cursorItemSlot.stack.amount - maxStack;
                        clickedSlot.itemSlot.InsertStack(new ItemStack(clickedSlot.itemSlot.stack.id, maxStack));
                        cursorItemSlot.stack.amount = leftAmount;
                        cursorItemSlot.InsertStack(cursorItemSlot.TakeAll());
                    }

                } else if ((clickedSlot.itemSlot.stack.amount + cursorItemSlot.stack.amount) < maxStack)
                {
                    clickedSlot.itemSlot.stack.amount += cursorItemSlot.TakeAll().amount;
                    clickedSlot.itemSlot.InsertStack(clickedSlot.itemSlot.TakeAll());
                }

            }
        }


    }

    private UIItemSlot CheckForSlot()
    {
        m_PointerEventData = new PointerEventData(m_EventSystem);
        m_PointerEventData.position = Input.mousePosition;

        List<RaycastResult> results = new List<RaycastResult>();
        m_Raycaster.Raycast(m_PointerEventData, results);

        foreach (RaycastResult result in results)
        {
            if (result.gameObject.tag == "UIItemSlot")
                return result.gameObject.GetComponent<UIItemSlot>();
        }

        return null;
    }
}
