using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public class SoundInstanceManager : MonoBehaviour
{
    // Array to hold instances of SoundInstanceEditor
    private SoundInstanceEditor[] soundInstances = new SoundInstanceEditor[0];
    private bool[] soundInstanceTagFoldouts;

    // private MonoBehaviour script;
    // private Type scriptType;

    // Manager level for controlling sound instances
    private float managerLevel;
    private bool managerLevelScriptControl = false;
    private bool managerLevelActive = false;
    private bool managerLevelScriptActive = false;
    private PropertyInfo managerLevelProperty;

    // Update is called once per frame
    void Update()
    {
        // Update sound instances and their methods
        UpdateSoundInstances();
        UpdateSoundInstanceMethods();
    }

    private void Awake()
    {
        // Initialize array to hold foldout state for each sound instance tag
        soundInstanceTagFoldouts = new bool[0];
    }

    public void UpdateSoundInstanceMethods()
    {
        // Set manager level and update inspector for each sound instance
        foreach(SoundInstanceEditor soundInstance in soundInstances)
        {
            soundInstance.SetManagerLevel(managerLevelActive, managerLevel);
            soundInstance.UpdateInspectorAndOnRunning();
        }
    }

    public void DrawInspectorGUI()
    {
        EditorGUILayout.LabelField("Manager Properties", EditorStyles.boldLabel);

        string managerLevelString = "Manager Level";

        // if(managerLevelProperty != null) { 
        //     managerLevelString += " (controlled by script)"; 
        //     managerLevel = Convert.ToSingle(managerLevelProperty.GetValue(script));
        // }

        if(managerLevelScriptControl)
        {
            // Allow script control of manager level
            managerLevelString += " (controlled by script)";
            managerLevelScriptActive = EditorGUILayout.Toggle("Enable Script Control", managerLevelScriptActive);
        }

        // Allow manual control of manager level
        EditorGUI.BeginDisabledGroup(managerLevelScriptActive);
        managerLevelActive = EditorGUILayout.Toggle("Enable Manager Level", managerLevelActive);
        EditorGUI.EndDisabledGroup();

        EditorGUI.BeginDisabledGroup(managerLevelActive == false || managerLevelScriptActive);
        managerLevel = EditorGUILayout.Slider(managerLevelString, managerLevel, 0, 1);
        EditorGUI.EndDisabledGroup();
        
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Editor Instances", EditorStyles.boldLabel);

        GUILayout.BeginVertical();
        EditorGUI.indentLevel++;

        // Get all sound instance tags
        List<SoundInstanceTag> soundInstanceTags = SoundInstanceTagManager.Instance.GetSoundInstanceTags();
        for (int i = 0; i < soundInstanceTags.Count; i++)
        {
            SoundInstanceTag soundInstanceTag = soundInstanceTags[i];
            
            // Filter sound instances by tag
            SoundInstanceEditor[] soundInstancesFiltered = soundInstances.Where(soundInstance => soundInstance.SoundInstanceTag == soundInstanceTag).ToArray();
            if (soundInstancesFiltered.Length == 0) continue;
            
            // Draw foldout for each sound instance tag
            soundInstanceTagFoldouts[i] = EditorGUILayout.Foldout(soundInstanceTagFoldouts[i], soundInstanceTag.name + " (" + soundInstancesFiltered.Length + ")");
            if (soundInstanceTagFoldouts[i])
            {
                EditorGUI.indentLevel++;

                foreach (SoundInstanceEditor soundInstance in soundInstancesFiltered)
                {
                    bool drawInspector = false;

                    GUILayout.BeginHorizontal();

                    if (soundInstance.SoundInstanceEditorObject == null)
                    {
                        EditorGUILayout.Foldout(false, "Editor has not been initialized yet!");
                    }
                    else
                    {
                        soundInstance.ShowInManager = EditorGUILayout.Foldout(soundInstance.ShowInManager, soundInstance.SoundInstanceEditorObject.InstanceName);

                        if (soundInstance.ShowInManager)
                        {
                            drawInspector = true;
                        }
                    }

                    if (GUILayout.Button("Go to Editor!", GUILayout.ExpandWidth(false)))
                    {
                        // Select the sound instance object in the Hierarchy
                        Selection.activeObject = soundInstance.gameObject;
                    }

                    GUILayout.EndHorizontal();

                    if(drawInspector)
                    {
                        // Draw the inspector GUI for the sound instance
                        GUILayout.BeginVertical(EditorStyles.helpBox);
                        soundInstance.DrawInspectorGUI();
                        GUILayout.EndVertical();
                    }
                    
                }

                EditorGUI.indentLevel--;
            }
        }
        
        EditorGUI.indentLevel--;
        GUILayout.EndVertical();
    }

    // Set manager level
    public void SetManagerLevel(GameObject gameObject, bool active, float level)
    {   
        this.managerLevelScriptControl = true;
        if (managerLevelScriptActive)
        {
            this.managerLevelActive = active;
            this.managerLevel = level;
        }
    }

    // Update sound instances by comparing with existing instances
    public void UpdateSoundInstances()
    {
        SoundInstanceEditor[] newSoundInstances = UnityEngine.Object.FindObjectsOfType<SoundInstanceEditor>();

        // Compare each sound instance with the ones in the manager
        // If they differ, update the array and create new foldouts
        if (!newSoundInstances.SequenceEqual(soundInstances))
        {
            soundInstances = newSoundInstances;
            soundInstanceTagFoldouts = new bool[newSoundInstances.Length];
        }
    }

    public void InitSoundInstances()
    {
        // Set manager level and update inspector for each sound instance
        foreach(SoundInstanceEditor soundInstance in soundInstances)
        {
            soundInstance.InitEditorObject();
            soundInstance.LoadEditorPrefs();
        }
    }
}

[CustomEditor(typeof(SoundInstanceManager))]
public class SoundInstanceManagerEditor : UnityEditor.Editor
{
    SoundInstanceManager SoundInstanceManager;
    
    private void OnEnable()
    {
        SoundInstanceManager = (SoundInstanceManager)target;
        
        SoundInstanceManager.UpdateSoundInstances();
        SoundInstanceManager.InitSoundInstances();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        if (!Application.isPlaying)
        {
            // Update sound instances during editing mode
            SoundInstanceManager.UpdateSoundInstances();
            SoundInstanceManager.UpdateSoundInstanceMethods();
        }

        // Draw the manager's inspector GUI
        SoundInstanceManager.DrawInspectorGUI();

        serializedObject.ApplyModifiedProperties();
    }
}
