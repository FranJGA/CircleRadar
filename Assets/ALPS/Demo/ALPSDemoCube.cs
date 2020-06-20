using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

/**
 * This this the main application script. All interactions with ALPS is coded into ALPSDemoListener class.
 * All AdMob related code is kept in ALPSDemoAdMobListener class. This class holds application logic.
 * The only ALPS calls made from this class is 'ALPS.setALPSListener()'.
 */
public class ALPSDemoCube : MonoBehaviour {
    private TextMesh mText;

    // Use this for initialization
    void Start () {
        mText = GameObject.Find("Coords").GetComponent<TextMesh>();

        //Initialize IAP. Although asynchronous, this call will add to the app startup time, so any app that doesn't use IAP must not use
        //this call. To speedup things, apps may persist last purchase status by themselves and use this saved information until IAP is ready.
        //The demo app starts AdMob after IAP initialization is over, so that we don't load AdMob if the sku is already owned.
        ALPS.iapConnect(new ALPSDemoIAPListener(), new string[] { "gopro" });

        //Register callbacks with ALPS. This is a mandate; until the setReady is called, ALPS wont show preferences activity. User will see
        //a 'Not ready' message if they clicked the settings button in live wallpaper preview.
        ALPS.setALPSListener(new ALPSDemoListener());
    }
	
	// Update is called once per frame
	void Update () {
        if (Input.touchCount > 0){
            Touch touch = Input.GetTouch(0);
            mText.text = "" + touch.position;
        }        
    }
}
