using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public class SoundInstanceManager : MonoBehaviour
{
    [SerializeField]
    private SoundInstanceEditor[] soundInstancesUnity;

    [SerializeField]
    private SoundInstanceEditor[] soundInstancesFmod;

    private bool showInstanceEditorsUnity;
    private bool showInstanceEditorsFmod;

    [SerializeField]
    private MonoBehaviour script;
    private Type scriptType;
    private float managerLevel;
    private bool managerLevelActive;
    private bool managerLevelScriptActive;
    private PropertyInfo managerLevelProperty;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Application.isPlaying)
        {
            UpdateSoundInstances();
            UpdateSoundInstanceMethods();
        }
    }

    public void UpdateSoundInstanceMethods()
    {
        for(int i = 0; i < soundInstancesUnity.Length; i++)
        {
            SoundInstanceEditor soundInstance = soundInstancesUnity[i];
            soundInstance.UpdateManagerLevel(managerLevelActive, managerLevel);
            soundInstance.UpdateMethods();
        }

        for(int i = 0; i < soundInstancesFmod.Length; i++)
        {
            SoundInstanceEditor soundInstance = soundInstancesFmod[i];
            soundInstance.UpdateManagerLevel(managerLevelActive, managerLevel);
            soundInstance.UpdateMethods();
        }
    }

    public void DrawInspectorGUI()
    {
        string managerLevelString = "Manager Level";
        if(managerLevelProperty != null) { 
            managerLevelString += " (controlled by script)"; 
            managerLevel = Convert.ToSingle(managerLevelProperty.GetValue(script));
        }

        managerLevelScriptActive = EditorGUILayout.Toggle("Enable Script Control", managerLevelScriptActive);

        EditorGUI.BeginDisabledGroup(managerLevelProperty != null || managerLevelScriptActive);
        managerLevelActive = EditorGUILayout.Toggle("Enable Manager Level", managerLevelActive);
        EditorGUI.EndDisabledGroup();

        EditorGUI.BeginDisabledGroup(managerLevelActive == false || managerLevelProperty != null || managerLevelScriptActive);
        managerLevel = EditorGUILayout.Slider(managerLevelString, managerLevel, 0, 1);
        EditorGUI.EndDisabledGroup();
        

        GUILayout.BeginVertical(GUI.skin.box);
        EditorGUI.indentLevel++;
        
        // Unity Sound Instances
        showInstanceEditorsUnity = EditorGUILayout.Foldout(showInstanceEditorsUnity, "Unity Sound Instances: (" + soundInstancesUnity.Length + ")");
        if (showInstanceEditorsUnity) {
            EditorGUI.indentLevel++;
            for(int i = 0; i < soundInstancesUnity.Length; i++)
            {
                SoundInstanceEditor soundInstance = soundInstancesUnity[i];

                GUILayout.BeginHorizontal();

                if(soundInstance.SoundInstanceEditorObject != null)
                {
                    soundInstance.showInManager = EditorGUILayout.Foldout(soundInstance.showInManager, soundInstance.SoundInstanceEditorObject.InstanceName);
                } else {
                    EditorGUILayout.LabelField("Editor has no audio source!");
                }

                if (GUILayout.Button("Go to Editor!")){
                    Selection.activeObject = soundInstance.gameObject;
                }

                GUILayout.EndHorizontal();

                if(soundInstance.showInManager && soundInstance.SoundInstanceEditorObject != null)
                {
                    soundInstance.DrawInspectorGUI();
                }
                
            }
            EditorGUI.indentLevel--;
        }
        
        // FMOD Sound Instances
        showInstanceEditorsFmod = EditorGUILayout.Foldout(showInstanceEditorsFmod, "Fmod Sound Instances: (" + soundInstancesFmod.Length + ")");
        if (showInstanceEditorsFmod) {
            EditorGUI.indentLevel++;
            for(int i = 0; i < soundInstancesFmod.Length; i++)
            {
                SoundInstanceEditor soundInstance = soundInstancesFmod[i];
                GUILayout.BeginHorizontal();

                if(soundInstance.SoundInstanceEditorObject != null)
                {
                    soundInstance.showInManager = EditorGUILayout.Foldout(soundInstance.showInManager, soundInstance.SoundInstanceEditorObject.InstanceName);
                } else {
                    EditorGUILayout.LabelField("Editor has not active audio source set!");
                }

                if (GUILayout.Button("Go to Editor!")){
                    Selection.activeObject = soundInstance.gameObject;
                }

                GUILayout.EndHorizontal();

                if(soundInstance.showInManager && soundInstance.SoundInstanceEditorObject != null)
                {
                    soundInstance.DrawInspectorGUI();
                }
            }
            EditorGUI.indentLevel--;
        }
        
        EditorGUI.indentLevel--;
        GUILayout.EndVertical();
    }

    public void SetManagerLevel(bool active, float level)
    {
        if (managerLevelScriptActive) {
            this.managerLevelActive = active;
            this.managerLevel = level;
        }
    }

    public void UpdateSoundInstances()
    {
        SoundInstanceEditor[] soundInstances = UnityEngine.Object.FindObjectsOfType<SoundInstanceEditor>();
        soundInstancesUnity = soundInstances.Where(obj => obj.editorType == SoundInstanceEditorType.Unity).ToArray();
        soundInstancesFmod = soundInstances.Where(obj => obj.editorType == SoundInstanceEditorType.Fmod).ToArray();
    }
}

[CustomEditor(typeof(SoundInstanceManager))]
public class SoundInstanceManagerEditor : UnityEditor.Editor
{
    SoundInstanceManager SoundInstanceManager;
    
    private void OnEnable()
    {
        SoundInstanceManager = (SoundInstanceManager)target;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        if (!Application.isPlaying)
        {
            SoundInstanceManager.UpdateSoundInstances();
            SoundInstanceManager.UpdateSoundInstanceMethods();
        }

        SoundInstanceManager.DrawInspectorGUI();

        serializedObject.ApplyModifiedProperties();
    }
}
