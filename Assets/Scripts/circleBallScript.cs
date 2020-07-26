using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class circleBallScript : MonoBehaviour { 
    public AudioSource source { get { return GetComponent<AudioSource>(); } }
    public GameObject canvas { get { return GetComponent<GameObject>(); } }
    public AudioClip clip;


    // Start is called before the first frame update
    void Start()
    {

    gameObject.AddComponent<AudioSource>();


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
