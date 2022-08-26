using Microsoft.MixedReality.Toolkit.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogController : MonoBehaviour 
{
    public GameObject dialogPrefag;
    public void OpenDialog(string title, string message)
    {
        Dialog.Open(dialogPrefag, DialogButtonType.OK, title, message, false);
    }
    public void OpenConfirmDialog(string title, string message, System.Action<DialogResult> callback)
    {
        Dialog d = Dialog.Open(dialogPrefag, DialogButtonType.Yes | DialogButtonType.No, title, message, true);
        d.OnClosed += callback;
    }
}
