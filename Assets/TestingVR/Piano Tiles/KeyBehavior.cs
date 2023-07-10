using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class KeyBehavior : MonoBehaviour
{
    [SerializeField] private int position;
    [SerializeField] private float speed;

    [SerializeField] private float target;
    [SerializeField] private List<AudioClip> tones;
    [SerializeField] private AudioClip failSound;

    private AudioSource audioSource;
    private MeshRenderer meshRenderer;
    private Animator animator;
    private PianoTilesManager pianoTilesManager;

    private bool tapped;
    private bool failedToTap;
    
    private void Start()
    {
        audioSource = GetComponent<AudioSource>();

        var toneIndex = Random.Range(0, 2) == 0 ? position : position + 4;
        audioSource.clip = tones[toneIndex];
        
        animator = GetComponentInChildren<Animator>();
        meshRenderer = GetComponentInChildren<MeshRenderer>();
        pianoTilesManager = GameObject.FindWithTag("Keyboard").GetComponent<PianoTilesManager>();
    }

    // Update is called once per frame
    private void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, new Vector3(transform.position.x, transform.position.y, target), speed * Time.deltaTime);

        if (transform.position.z < -0.5 && !failedToTap)
        {
            Debug.Log(transform.position);
            Debug.Log("out: " + failedToTap);
            FailedToTap();
        }
    }

    public void SetSpeed(float speed)
    {
        if(failedToTap || tapped) return;
        
        this.speed = speed;
    }

    public void SetSound(int pos)
    {
        this.position = pos;
    }

    public void Tapped()
    {
        if(tapped || failedToTap) return;

        tapped = true;
        pianoTilesManager.GotTap();
        speed = 0;
        audioSource.Play();
        animator.SetBool("fade", true);
        StartCoroutine(DisableObject());
        Destroy(gameObject, 5);
    }

    private void FailedToTap()
    {
        failedToTap = true;
        Debug.Log("in: " + failedToTap);
        audioSource.PlayOneShot(failSound);
        animator.SetBool("failed", true);
        pianoTilesManager.FailedTap();
        speed = 0;
        StartCoroutine(DisableObject());
        Destroy(gameObject, 5);
    }

    private IEnumerator DisableObject()
    {
        yield return new WaitForSeconds(1);
        meshRenderer.enabled = false;
    }
}
