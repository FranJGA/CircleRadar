using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ZoomScript : MonoBehaviour { 

    public AudioSource source { get{ return GetComponent<AudioSource>(); }}
    public Button btn { get { return GetComponent<Button>(); } }
    public AudioClip clip;


    // Start is called before the first frame update
    void Start()
    {
        gameObject.AddComponent<AudioSource> ();
        btn.onClick.AddListener(PlaySound);
       
    }

    // Update is called once per frame
    void Update()
    {
       
    }


    void PlaySound()
    {
        source.PlayOneShot(clip);
    }

}

