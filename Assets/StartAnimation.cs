using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartAnimation : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Animation anim = gameObject.GetComponent<Animation>();
        anim.clip.legacy = true;
        anim.clip.wrapMode = WrapMode.Loop;
        anim.Play();
    }

}
