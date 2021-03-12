using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Inventory : MonoBehaviour
{
    public GameObject gSlot;
    public GameObject test;
    private void Update()
    {

    }
    private void Start()
    {
        foreach(Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        Coroutine coroutine = StartCoroutine(CreateHUDSlots());
    }

    public IEnumerator CreateHUDSlots()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
            Debug.Log("killed all Objects");
        }
        float right = 0;
        float height = gSlot.GetComponent<RectTransform>().rect.height;
        for (int i = 0; i <= 10; i++)
        {
            GameObject.Instantiate(gSlot, new Vector3(transform.GetComponent<RectTransform>().rect.width - right, transform.position.y, transform.position.z), transform.rotation, transform);
            right += transform.GetComponent<RectTransform>().rect.width / 10 - 3;
        }
        yield return new WaitForSeconds(5f);

    }


}
