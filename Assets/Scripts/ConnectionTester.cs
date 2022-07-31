using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectionTester : MonoBehaviour
{
    public Material successMat;
    public Material failureMat;
    public GameObject serverSettingsMenu;
    private ServerSettings serverSettingsScript;
    // Start is called before the first frame update

    private void Start()
    {
        serverSettingsScript = serverSettingsMenu.GetComponent<ServerSettings>();
    }
    // Update is called once per frame
    void Update()
    {
        if (serverSettingsScript.connectionSuccessful) {
            GetComponent<MeshRenderer>().material = successMat;
        }
        else
        {
            GetComponent<MeshRenderer>().material = failureMat;
        }

    }

}
