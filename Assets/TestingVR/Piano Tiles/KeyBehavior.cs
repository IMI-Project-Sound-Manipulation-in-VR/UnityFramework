using System;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR.Interaction.Toolkit;
using Random = UnityEngine.Random;

public class KeyBehavior : MonoBehaviour
{
    [SerializeField] private int position;
    [SerializeField] private float speed;

    [SerializeField] private float target;
    [SerializeField] private List<AudioClip> tones;
    [SerializeField] private AudioClip failSound;

    private AudioSource _audioSource;
    private MeshRenderer _meshRenderer;
    private Animator _animator;
    private PianoTilesManager _pianoTilesManager;

    private bool _tapped;
    private bool _failedToTap;
    private bool hovered;
    
    private void Start()
    {
        _audioSource = GetComponent<AudioSource>();

        var toneIndex = Random.Range(0, 2) == 0 ? position : position + 4;
        _audioSource.clip = tones[toneIndex];
        
        _animator = GetComponentInChildren<Animator>();
        _meshRenderer = GetComponentInChildren<MeshRenderer>();
        _pianoTilesManager = GameObject.FindWithTag("Keyboard").GetComponent<PianoTilesManager>();
    }

    // Update is called once per frame
    private void Update()
    {
        if(speed != 0)
            transform.position = Vector3.MoveTowards(transform.position, new Vector3(target, transform.position.y, transform.position.z), speed * Time.deltaTime);

        if (transform.position.x < -0.5 && !_failedToTap)
        {
            FailedToTap();
        }
    }

    public void SetSpeed(float speed)
    {
        if(_failedToTap || _tapped) return;
        
        this.speed = speed;
    }

    public void SetSound(int pos)
    {
        this.position = pos;
    }
    
    public void Hovered(SelectExitEventArgs args)
    {
        hovered = true;
    }
    
    public void Tapped(SelectEnterEventArgs args)
    {
        //if(args.interactorObject.transform.position.y <= 1) return; // just allow poking from top to bottom
        
        if (!hovered) return;
         
        if(_tapped || _failedToTap) return;

        _tapped = true;
        _pianoTilesManager.GotTap();
        speed = 0;
        _audioSource.Play();
        _animator.SetBool("fade", true);
        DisableObject();
        Destroy(gameObject, 5);
    }

    private void FailedToTap()
    {
        _failedToTap = true;
        _audioSource.PlayOneShot(failSound);
        _animator.SetBool("failed", true);
        _pianoTilesManager.FailedTap();
        speed = 0;
        DisableObject();
        Destroy(gameObject, 5);
    }

    private async Task DisableObject()
    {
        await Task.Delay(1000);

        _meshRenderer.enabled = false;
    }
}
