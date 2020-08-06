using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Advertisements;

public class ScriptAdsManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if(Advertisement.IsReady())
        {
            Advertisement.Show();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
