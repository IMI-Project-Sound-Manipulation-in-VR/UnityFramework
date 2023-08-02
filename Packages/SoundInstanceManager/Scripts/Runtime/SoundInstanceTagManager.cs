using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundInstanceTagManager
{
    private static readonly Lazy<SoundInstanceTagManager> lazy = new Lazy<SoundInstanceTagManager>(() => new SoundInstanceTagManager());
    public static SoundInstanceTagManager Instance { get { return lazy.Value; } }

    public List<SoundInstanceTag> soundInstanceTags = new List<SoundInstanceTag>();

    public void AddSoundInstanceTag(SoundInstanceTag soundInstanceTag)
    {
        // check if soundInstanceTag already exists before adding it to the list of soundInstanceTags
        if (GetSoundInstanceTag(soundInstanceTag.name) == null)
        {
            soundInstanceTags.Add(soundInstanceTag);
        }
    }

    public void RemoveSoundInstanceTag(SoundInstanceTag soundInstanceTag)
    {
        soundInstanceTags.Remove(soundInstanceTag);
    }

    public SoundInstanceTag GetSoundInstanceTag(string soundInstanceTagName)
    {
        foreach (SoundInstanceTag soundInstanceTag in soundInstanceTags)
        {
            if (soundInstanceTag.name == soundInstanceTagName)
            {
                return soundInstanceTag;
            }
        }
        return null;
    }

    public List<SoundInstanceTag> GetSoundInstanceTags()
    {
        LoadSoundInstanceTags();
        return soundInstanceTags;
    }

    public void LoadSoundInstanceTags()
    {
        // load all sound instance tag library from resources folder and get their sound instance tags list
        SoundInstanceTagLibrary[] soundInstanceTagLibraries = Resources.LoadAll<SoundInstanceTagLibrary>("");

        // create a list of sound instances tags from all sound instance tag libraries
        soundInstanceTags = new List<SoundInstanceTag>();
        foreach (SoundInstanceTagLibrary soundInstanceTagLibrary in soundInstanceTagLibraries)
        {
            soundInstanceTags.AddRange(soundInstanceTagLibrary.soundInstanceTags);
        }
    }

}
