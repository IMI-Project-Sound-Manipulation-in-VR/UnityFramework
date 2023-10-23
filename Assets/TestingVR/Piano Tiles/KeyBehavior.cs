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

        // get random the higher or lower tone of the position -> each position has two tones
        var toneIndex = Random.Range(0, 2) == 0 ? position : position + 4;
        _audioSource.clip = tones[toneIndex];
        
        _animator = GetComponentInChildren<Animator>();
        _meshRenderer = GetComponentInChildren<MeshRenderer>();
        _pianoTilesManager = GameObject.FindWithTag("Keyboard").GetComponent<PianoTilesManager>();
    }

    // Update is called once per frame
    private void Update()
    {
        // move key towards the piano
        if(speed != 0)
            transform.position = Vector3.MoveTowards(transform.position, new Vector3(target, transform.position.y, transform.position.z), speed * Time.deltaTime);

        // if the key gets over the limit (border of the piano) call the FailedToTap method
        if (transform.position.x < -0.5 && !_failedToTap)
        {
            FailedToTap();
        }
    }

    // sets the speed for this key if it is not already tapped or failed to tap
    public void SetSpeed(float speed)
    {
        if(_failedToTap || _tapped) return;
        
        this.speed = speed;
    }

    // sets the sound depending on its position
    public void SetSound(int pos)
    {
        this.position = pos;
    }
    
    // if the key is hovered, set boolean to determine the tap is coming from top to bottom
    public void Hovered(SelectExitEventArgs args)
    {
        hovered = true;
    }
    
    // When the key is tapped, call the GotTap method to increase score and speed if its hovered, not tapped or failed to tap
    public void Tapped(SelectEnterEventArgs args)
    {
        if (!hovered) return;
         
        if(_tapped || _failedToTap) return;

        _tapped = true;
        _pianoTilesManager.GotTap();
        speed = 0;
        
        // play the sound of the key and destroy it after fading animation
        _audioSource.Play();
        _animator.SetBool("fade", true);
        DisableObject();
        Destroy(gameObject, 5);
    }

    // When failing to tap, play the failSound, inform the pianoTiles manager to decrease score and speed and destroy the key
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

    // disable key after one second by disabling its mesh renderer
    private async Task DisableObject()
    {
        await Task.Delay(1000);

        _meshRenderer.enabled = false;
    }
}
