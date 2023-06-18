using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class KeyBehavior : MonoBehaviour
{
    [SerializeField] private float speed;

    [SerializeField] private float target;
    [SerializeField] private List<AudioClip> tones;
    [SerializeField] private AudioClip failSound;

    private AudioSource audioSource;
    private Animator animator;
    private PianoTilesManager pianoTilesManager;

    private bool tapped;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = tones[Random.Range(0, tones.Count)];
        animator = GetComponentInChildren<Animator>();
        pianoTilesManager = GameObject.FindWithTag("Keyboard").GetComponent<PianoTilesManager>();
    }

    // Update is called once per frame
    private void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, new Vector3(transform.position.x, transform.position.y, target), speed * Time.deltaTime);
        
        if (transform.position.z < -0.5)
            FailedToTap();
    }

    public void SetSpeed(float speed)
    {
        this.speed = speed;
    }

    public void Tapped()
    {
        if(tapped) return;

        tapped = true;
        pianoTilesManager.GotTap();
        speed = 0;
        audioSource.Play();
        animator.SetBool("fade", true);
        Destroy(gameObject, 1);
    }

    private void FailedToTap()
    {
        audioSource.PlayOneShot(failSound);
        animator.SetBool("failed", true);
        pianoTilesManager.GameOver();
    }
}
