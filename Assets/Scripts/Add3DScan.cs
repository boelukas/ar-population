using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Add3DScan : MonoBehaviour
{
    public GameObject scan;
    public GameObject scanSelector;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void AddScan()
    {
        Instantiate(scan, Camera.main.transform.position, Quaternion.identity);
        scanSelector.SetActive(false);
    }
}
