using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Sound Instance Tag Library", menuName = "Sound Instance Tag Library")]
public class SoundInstanceTagLibrary : ScriptableObject
{
    public List<SoundInstanceTag> soundInstanceTags = new List<SoundInstanceTag>();
}
