using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using FMODUnity;
using UnityEditor;
using UnityEngine;

public enum SoundInstanceEditorType
{
    Unity,
    Fmod
}

public class SoundInstanceEditor : MonoBehaviour
{
    // Editor Type
    [SerializeField] private SoundInstanceEditorType editorType;
    public SoundInstanceEditorType EditorType
    {
        get { return editorType; }
        set
        {
            if(value != editorType)
            {
                editorType = value;
                SetupEditorObject();
            }
        }
    }

    // Sound Instance Editor Object
    private SoundInstanceEditorObject soundInstanceEditorObject;
    public SoundInstanceEditorObject SoundInstanceEditorObject => soundInstanceEditorObject;

    // Tag
    private SoundInstanceTag soundInstanceTag;
    public SoundInstanceTag SoundInstanceTag => soundInstanceTag;
    private int soundInstanceTagIndex;

    // Unity
    // Audio Clip Reference
    [SerializeField] private AudioClip audioClipReference;
    public AudioClip AudioClipReference 
    {
        get { return audioClipReference; }
        set 
        {
            if(value != audioClipReference)
            {
                audioClipReference = value;
                if(editorType != SoundInstanceEditorType.Unity) { return; }
                if(audioClipReference != null)
                {
                    SetupEditorObject();
                }
                else
                {
                    ResetSoundInstanceEditor();
                }
            }
            audioClipReference = value;
        }
    }

    public AudioSource AudioSourceReference { get; set; }

    // FMOD Event reference
    [SerializeField] private EventReference fmodEventReference;
    private Guid previousGuid;
    public EventReference FmodEventReference
    {
        get { return fmodEventReference; }
        set 
        {
            if(value.Guid != previousGuid)
            {
                fmodEventReference = value;
                previousGuid = value.Guid;
                if(EditorType != SoundInstanceEditorType.Fmod) { return; }
                SetupEditorObject();
            }
            // fmodEventReference = value;
            // previousGuid = value.Guid;
        }
    }

    // Manager Level
    private float managerLevel;
    private bool managerLevelActive;
    public bool ShowInManager { get; set; }

    // Editor Level
    private float editorLevel;
    private bool editorLevelActive;
    private PropertyInfo editorLevelProperty;

    // Reflection external script
    [SerializeField] private MonoBehaviour reflectionScript;
    public Type reflectionScriptType { get; private set; }
    public PropertyInfo[] ReflectionScriptProperties { get; set; }
    private bool reflectionScriptActive;

    // Properties
    private bool addProperty;
    private bool addPreset;
    private string presetName;

    // Start is called before the first frame update
    void Start()
    {
        LoadReflectionScript();
        InitializeEditorObject();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateInspectorAndOnRunning();
    }

    void Awake()
    {
        // Get a list of available SoundInstanceTags from the SoundInstanceTagManager
        List<SoundInstanceTag> tags = SoundInstanceTagManager.Instance.GetSoundInstanceTags();
        if(tags.Count != 0) { soundInstanceTag = tags[0]; }
    }

    // Public
    public void SetManagerLevel(bool active, float value)
    {
        this.managerLevelActive = active;
        this.managerLevel = value;
    }

    public void SetEditorLevel(bool active, float value)
    {
        this.editorLevelActive = active;
        this.managerLevel = value;
    }

    // Inspector GUI Methods
    public void DrawInspectorGUIDefaultInfo()
    {
        EditorGUILayout.LabelField("Editor Info", EditorStyles.boldLabel);
        DrawInspectorGUIEditorType();
        DrawInspectorGUITags();
    }

    private void DrawInspectorGUIEditorType()
    {
        EditorType = (SoundInstanceEditorType) EditorGUILayout.EnumPopup("Editor Type", editorType);
    }

    public void DrawInspectorGUI()
    {
        if(soundInstanceEditorObject != null)
        {

            EditorGUILayout.LabelField("Editor Properties", EditorStyles.boldLabel);
            
            DrawInspectorGUIEditorLevel();

            DrawInspectorGUIPresets();

            EditorGUILayout.Space(10);

            EditorGUILayout.LabelField("Sound Properties", EditorStyles.boldLabel);

            EditorGUI.indentLevel++;

            DrawInspectorGUISoundProperties();
            
            EditorGUI.indentLevel--;

            DrawInspectorGUIAddingAudioProperties();

            DrawInspectorGUISaveAudioPropertiesAsExistingPreset();

            DrawInspectorGUISaveAudioPropertiesAsNewPreset();
        }
    }

    private void DrawInspectorGUISaveAudioPropertiesAsNewPreset()
    {
        if (editorType == SoundInstanceEditorType.Unity)
        {
            if (addPreset)
            {
                EditorGUILayout.BeginHorizontal();

                // Display a text field for entering the new preset name
                Rect curveRect = EditorGUILayout.GetControlRect();
                presetName = EditorGUI.TextField(curveRect, presetName);

                // If the "Add new preset" button is clicked
                if (GUILayout.Button("Add new preset", GUILayout.Width(100)))
                {
                    string directoryPath = "Assets" + "/" + "Scenes" + "/" + "Resources" + "/" + "Audio Property Presets";

                    // Check if the directory exists
                    if (!Directory.Exists(directoryPath))
                    {
                        // If the directory doesn't exist, create it
                        Directory.CreateDirectory(directoryPath);
                    }

                    // Check if the preset name is not empty
                    if (!string.IsNullOrEmpty(presetName))
                    {
                        string assetPath = directoryPath + "/" + presetName + ".asset";

                        // Create a new SoundInstanceEditorAudioPropertyPreset instance
                        SoundInstanceEditorAudioPropertyPreset newPreset = ScriptableObject.CreateInstance<SoundInstanceEditorAudioPropertyPreset>();
                        newPreset.propertiesArray = soundInstanceEditorObject.AudioProperties.ToArray();

                        // Create and save the asset
                        AssetDatabase.CreateAsset(newPreset, assetPath);
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();

                        // Load and set the newly created preset
                        soundInstanceEditorObject.LoadPropertyPresets();
                        soundInstanceEditorObject.SelectedPropertyPresetIndex = System.Array.FindIndex(soundInstanceEditorObject.PropertyPresets, p => p.name == presetName);
                        soundInstanceEditorObject.SetAudioPropertiesFromPreset();

                        // Hide the current display after adding a new preset
                        addPreset = false;
                    }
                }

                EditorGUILayout.EndHorizontal();
            }
            else
            {
                // Display a button to show the menu for saving the configuration as a new preset
                addPreset = GUILayout.Button("Save configuration as new preset");
            }
        }
    }

    private void DrawInspectorGUISaveAudioPropertiesAsExistingPreset()
    {
        if (editorType == SoundInstanceEditorType.Unity)
        {
            // Check if the current audio properties differ from the selected preset
            if (!soundInstanceEditorObject.ComparePresetWithAudioProperties())
            {
                if (GUILayout.Button("Save changes to preset"))
                {
                    // Get the current selected preset
                    SoundInstanceEditorAudioPropertyPreset currentPreset = soundInstanceEditorObject.PropertyPresets[soundInstanceEditorObject.SelectedPropertyPresetIndex];

                    // Convert the list of audio properties to an array
                    SoundInstanceEditorAudioProperty[] audioPropertiesArray = soundInstanceEditorObject.AudioProperties.ToArray();

                    // Update the properties array in the current preset and apply changes
                    currentPreset.UpdatePropertiesArray(audioPropertiesArray);
                    soundInstanceEditorObject.SetAudioPropertiesFromPreset();
                }
            }
        }
    }

    private void DrawInspectorGUIAddingAudioProperties()
    {
        if (editorType == SoundInstanceEditorType.Unity)
        {
            if (addProperty)
            {
                EditorGUILayout.BeginHorizontal();

                // Display the names of all available property templates in a popup
                string[] propertyNames = System.Array.ConvertAll(soundInstanceEditorObject.PropertyTemplates, obj => obj.propertyData.propertyName);
                
                // If the "Go back" button is pressed, hide the current display
                addProperty = !GUILayout.Button("Go back", GUILayout.Width(100));

                // Display a popup to select the index of the selected property template
                soundInstanceEditorObject.SelectedPropertyTemplateIndex = EditorGUILayout.Popup(soundInstanceEditorObject.SelectedPropertyTemplateIndex, propertyNames);

                // If the "Add" button is clicked
                if (GUILayout.Button("Add", GUILayout.Width(100)))
                {
                    // Add the selected property template to the preset
                    soundInstanceEditorObject.AddNewAudioProperty();

                    // Hide the current display after adding a new template
                    addProperty = false;
                }

                EditorGUILayout.EndHorizontal();
            }
            else
            {
                // Display a button to show the menu for adding a new property template to the preset
                addProperty = GUILayout.Button("Add new property");
            }
        }
    }

    private void DrawInspectorGUISoundProperties()
    {   
        // Check if the list of audio properties has been initialized
        if(soundInstanceEditorObject.AudioProperties != null)
        {
            // Iterate over all audio properties
            for (int i = 0; i < soundInstanceEditorObject.AudioProperties.Count; i++)
            {
                // Take the current property from the list
                SoundInstanceEditorAudioProperty property = soundInstanceEditorObject.AudioProperties[i];

                // Generate the foldout name for the current property
                string foldoutName =  char.ToUpper(property.propertyName[0]) + property.propertyName.Substring(1);
                if(property.propertyControlType != SoundInstanceEditorAudioPropertyControlType.None) { foldoutName += "(" + property.propertyControlType.ToString() + ")"; }

                GUILayout.BeginVertical(EditorStyles.helpBox);
                GUILayout.BeginHorizontal();
                property.showProperty = EditorGUILayout.Foldout(property.showProperty, foldoutName);

                 // Display a button to remove the current property (if applicable)
                if(property.propertyType == SoundInstanceEditorAudioPropertyType.UnityAudioProperty)
                {
                    if (GUILayout.Button("Remove Property", GUILayout.ExpandWidth(false)))
                    {
                        soundInstanceEditorObject.RemoveAudioProperty(i);
                    }
                }
                GUILayout.EndHorizontal();

                // If the property can be displayed
                if(property.showProperty){
                    property.propertyEvaluationType = (SoundInstanceEditorAudioPropertyEvaluationType) EditorGUILayout.EnumPopup("Property Type", property.propertyEvaluationType);

                    // Switch through the property evaluation types
                    // The property evaluation types determine the type of control for input value evaluation
                    switch(property.propertyEvaluationType)
                    {
                        // Handle the curve-based evaluation type
                        case SoundInstanceEditorAudioPropertyEvaluationType.Curve:
                            // Display the animation curve in the inspector
                            Rect curveRect = EditorGUILayout.GetControlRect();
                            property.curve = EditorGUI.CurveField(curveRect, "Curve", property.curve);

                            // Display the input slider, allowing user control
                            // Disable if an external control type has been set (NONE indicates no external control)
                            EditorGUI.BeginDisabledGroup(property.propertyControlType != SoundInstanceEditorAudioPropertyControlType.None);
                            property.inputValue = EditorGUILayout.Slider("Input Value", property.inputValue, 0, 1);
                            EditorGUI.EndDisabledGroup();

                            // Display the output value on a slider for visualization
                            GUI.enabled = false;
                            property.outputValue = EditorGUILayout.Slider("Output Value: ", property.outputValue, property.minValue, property.maxValue);
                            GUI.enabled = true;

                            break;
                        
                        // Handle the labeled evaluation type
                        // For example, given two labels "a" and "b", a input value between 0.0-0.5 will evaluate "a" and 0.5-1.0 will evaluate "b"
                        case SoundInstanceEditorAudioPropertyEvaluationType.Labeled:

                            // Display the input slider, allowing user control
                            // Disable if an external control type has been set (NONE indicates no external control)
                            EditorGUI.BeginDisabledGroup(property.propertyControlType != SoundInstanceEditorAudioPropertyControlType.None);
                            property.inputValue = EditorGUILayout.Slider("Input Value", property.inputValue, 0, 1);
                            EditorGUI.EndDisabledGroup();

                            // Display the output value on a slider and as a separate label for visualization
                            GUI.enabled = false;
                            property.outputValue = EditorGUILayout.IntSlider("Output Value", (int) property.outputValue, (int) property.minValue, (int) property.maxValue);
                            
                            // Display the corresponding string label for the selected slider value
                            if(property.labels.Length == 0)
                            {
                                EditorGUILayout.LabelField("Property has no labels");
                            } 
                            else
                            {
                                EditorGUILayout.LabelField("Selected Label: ", property.labels[(int) property.outputValue]);
                            }
                            GUI.enabled = true;

                            break;
                        // the evaluation type level, evaluates normalized value to a boolean value, or rather a float value of 0.0f or 1.0f
                        // if the input value is between the levels vector2 x and y values, the output will be 1.0f, else 0.0f
                        case SoundInstanceEditorAudioPropertyEvaluationType.Level:

                            // Display a min-max slider to control the levels
                            GUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField("Level");
                            GUILayout.Label(property.level.x.ToString("F2"), GUILayout.Width(30));
                            EditorGUILayout.MinMaxSlider(ref property.level.x, ref property.level.y, 0.0f, 1.0f);
                            GUILayout.Label(property.level.y.ToString("F2"), GUILayout.Width(30));
                            GUILayout.EndHorizontal();
                            EditorGUILayout.Space();
                            
                            // Display the input slider, allowing user control
                            // Disable if an external control type has been set (NONE indicates no external control)
                            EditorGUI.BeginDisabledGroup(property.propertyControlType != SoundInstanceEditorAudioPropertyControlType.None);
                            property.inputValue = EditorGUILayout.Slider("Input Value", property.inputValue, 0, 1);
                            EditorGUI.EndDisabledGroup();

                            // Display the output value as a toggle for visualization
                            GUI.enabled = false;
                            bool b = property.outputValue != 0.0f;
                            property.outputValue = EditorGUILayout.Toggle("Active: ", b) ? 1.0f : 0.0f;
                            GUI.enabled = true;

                            break;

                        // the evaluation type linear, will linearly evaluate a input value between the set
                        // min and max values. for example given a min and max value of -1f and 1f
                        // a input value of 0.5f would equal a output value of 0.0f
                        case SoundInstanceEditorAudioPropertyEvaluationType.Linear:
                            // Display float fields for user-controlled min and max values
                            property.minValue = EditorGUILayout.FloatField("Min Value", property.minValue);
                            property.maxValue = EditorGUILayout.FloatField("Max Value", property.maxValue);

                            // Display the input slider, allowing user control
                            // Disable if an external control type has been set (NONE indicates no external control)
                            EditorGUI.BeginDisabledGroup(property.propertyControlType != SoundInstanceEditorAudioPropertyControlType.None);
                            property.inputValue = EditorGUILayout.Slider("Input Value", property.inputValue, 0, 1);
                            EditorGUI.EndDisabledGroup();

                            // Display the output value on a slider for visualization
                            GUI.enabled = false;
                            property.outputValue = EditorGUILayout.Slider("Output Value: ", property.outputValue, property.minValue, property.maxValue);
                            GUI.enabled = true;

                            break;
                    }
                }

                GUILayout.EndVertical();
            }
        }
    }

    private void DrawInspectorGUIPresets()
{
    // Presets are currently only present for Unity sound
    if (editorType == SoundInstanceEditorType.Unity)
    {
        // Display a popup or dropdown list for selecting presets

        // Convert the array of PropertyPresets to an array of preset names
        string[] audioSourcePropertiesPresetsStrings = System.Array.ConvertAll(soundInstanceEditorObject.PropertyPresets, obj => obj.name);

        // Display the dropdown for selecting a preset, showing the currently selected preset
        soundInstanceEditorObject.SelectedPropertyPresetIndex = EditorGUILayout.Popup("Presets", soundInstanceEditorObject.SelectedPropertyPresetIndex, audioSourcePropertiesPresetsStrings);

        // Check if the selected preset index has changed
        if (soundInstanceEditorObject.SelectedPropertyPresetIndex != soundInstanceEditorObject.PreviousPropertyPresetIndex)
        {
            // Update the previous preset index and apply the selected preset's audio properties
            soundInstanceEditorObject.PreviousPropertyPresetIndex = soundInstanceEditorObject.SelectedPropertyPresetIndex;
            soundInstanceEditorObject.SetAudioPropertiesFromPreset();
        }
    }
}

    private void DrawInspectorGUITags()
    {
        // Get a list of available SoundInstanceTags from the SoundInstanceTagManager
        List<SoundInstanceTag> tags = SoundInstanceTagManager.Instance.GetSoundInstanceTags();

        // Convert the list of tags to an array of strings
        string[] tagsStrings = System.Array.ConvertAll(tags.ToArray(), obj => obj.name);

        // Check if there are any tags available
        if (tagsStrings.Length == 0)
        {
            // Display a label indicating that no tags are present
            EditorGUILayout.Popup("Tag", 0, new string[] { "No tags present" });

            // Reset tag-related variables and return
            soundInstanceTagIndex = -1;
            soundInstanceTag = null;
            return;
        }
        else
        {
            // If tags are present, display the tag selection popup

            // Ensure that the soundInstanceTagIndex is within bounds
            if (soundInstanceTagIndex == -1) { soundInstanceTagIndex = 0; }

            // Display the dropdown for selecting a tag, showing the current tag selection
            soundInstanceTagIndex = EditorGUILayout.Popup("Tag", soundInstanceTagIndex, tagsStrings);

            // Set the selected SoundInstanceTag based on the index
            soundInstanceTag = tags[soundInstanceTagIndex];
        }
    }

   private void DrawInspectorGUIEditorLevel()
    {
        GUILayout.BeginHorizontal();

        // Set the base label for the editor level control
        string editorLevelString = "Editor Level";

        // Check if manager level is active or if the editor level is controlled by a script property
        // Modify the label string to provide additional information about the source of control
        if (editorLevelProperty != null && !managerLevelActive)
        {
            editorLevelString += " (controlled by script)";
        }
        else if (managerLevelActive)
        {
            editorLevelString += " (controlled by manager level)";
        }

        // Disable the slider if the editor level is inactive, controlled by script, or manager level is active
        EditorGUI.BeginDisabledGroup(editorLevelActive == false || editorLevelProperty != null || managerLevelActive == true);

        // Display the slider for editor level control
        if (managerLevelActive)
        {
            // If manager level is active, the editor level value will be replaced by the manager level value
            editorLevel = EditorGUILayout.Slider(editorLevelString, managerLevel, 0, 1);
        }
        else
        {
            // Display the slider for regular editor level control
            editorLevel = EditorGUILayout.Slider(editorLevelString, editorLevel, 0, 1);
        }
        EditorGUI.EndDisabledGroup();

        // Display a toggle to enable/disable the editor level control
        editorLevelActive = EditorGUILayout.Toggle(editorLevelActive);

        GUILayout.EndHorizontal();
    }

    // Private

    private void ResetSoundInstanceEditor()
    {
        if(soundInstanceEditorObject != null)
        {
            soundInstanceEditorObject.DisableAudioInstance();
            soundInstanceEditorObject = null;
        }
        addProperty = false;
    }

    private void SetAudioInstance()
    {
        if(soundInstanceEditorObject != null)
        {
            soundInstanceEditorObject.SetAudioInstance();
        }
    }

    private void LoadReflectionScript(){
        reflectionScriptType = reflectionScript ? reflectionScript.GetType() : null;
    }

    // Private

    private void LoadPropertyTemplates()
    {
        if(soundInstanceEditorObject == null) return;
        soundInstanceEditorObject.LoadPropertyTemplates();
    }

    private void LoadPropertyPresets()
    {
        if(soundInstanceEditorObject == null) return;
        soundInstanceEditorObject.LoadPropertyPresets();
    }

    public void UpdateInspectorAndOnRunning()
    {
        CheckReflectionScriptActive();
        UpdateAudioPropertyValues();
    }

    public void UpdateInspectorOnly()
    {
        LoadPropertyTemplates();
        LoadPropertyPresets();
    }

    private void CheckReflectionScriptActive()
    {
        if(reflectionScript != null)
        {
            if(reflectionScript.gameObject.activeInHierarchy != reflectionScriptActive)
            {
                reflectionScriptActive = reflectionScript.gameObject.activeInHierarchy;
                if(soundInstanceEditorObject != null) { 
                    soundInstanceEditorObject.SetAudioProperties();
                }
            }
        }
    }

    private void UpdateAudioPropertyValues()
    {
        // Check if the sound instance editor object and its audio properties are available
        if(soundInstanceEditorObject == null) return;
        if(soundInstanceEditorObject.AudioProperties == null) return;
        
        for (int index = 0; index < soundInstanceEditorObject.AudioProperties.Count; index++)
        {
            // Get the current audio property and its corresponding reflection property
            SoundInstanceEditorAudioProperty property = soundInstanceEditorObject.AudioProperties[index];
            PropertyInfo reflectionAudioProperty = ReflectionScriptProperties[index];

            // Get the reflection script property (if available)
            PropertyInfo reflectionScriptProperty = ReflectionScriptProperties != null ? ReflectionScriptProperties[index] : null;
            
            // Initialize input value
            float inputValue = property.inputValue;
            property.propertyControlType = SoundInstanceEditorAudioPropertyControlType.None;

            // Update input value based on different sources (manager level, editor level or script property)
            // The hierarchy is as follows: script property > manager level > editor level > audio property value
            if(editorLevelActive) {
                inputValue = editorLevel;
                property.propertyControlType = SoundInstanceEditorAudioPropertyControlType.Editor;
            }
            if(managerLevelActive) { 
                inputValue = managerLevel;
                property.propertyControlType = SoundInstanceEditorAudioPropertyControlType.Manager; 
            }
            if(reflectionScriptProperty != null) { 
                inputValue = Convert.ToSingle(reflectionScriptProperty.GetValue(reflectionScript));
                property.propertyControlType = SoundInstanceEditorAudioPropertyControlType.Script;
            }

            // Evaluate property based on its evaluation type
            switch (property.propertyEvaluationType)
            {
                case SoundInstanceEditorAudioPropertyEvaluationType.Curve:
                    // Update min and max values based on curve keys
                    property.inputValue = inputValue;
                    property.outputValue = property.curve.Evaluate(inputValue);
                    
                    // Update min and max values based on curve keys
                    if(property.curve.length != 0)
                    {
                        property.minValue = property.curve.keys.Min(key => key.value);
                        property.maxValue = property.curve.keys.Max(key => key.value);
                    } else {
                        property.minValue = 0;
                        property.maxValue = 0;
                    }
                    break;
                case SoundInstanceEditorAudioPropertyEvaluationType.Linear:
                    // Update input value and calculate output value linearly
                    property.inputValue = inputValue;
                    property.outputValue = property.minValue + (property.maxValue - property.minValue) * inputValue;
                    property.minValue = property.defaultMinValue;
                    property.maxValue = property.defaultMaxValue;
                    break;
                case SoundInstanceEditorAudioPropertyEvaluationType.Level:
                    property.inputValue = inputValue;
                    property.outputValue = inputValue >= property.level.x && inputValue <= property.level.y ? 1 : 0;
                    property.minValue = property.defaultMinValue;
                    property.maxValue = property.defaultMaxValue;
                    break;
                case SoundInstanceEditorAudioPropertyEvaluationType.Labeled:
                    property.inputValue = inputValue;
                    property.outputValue = Mathf.RoundToInt(Mathf.Lerp(property.minValue, property.maxValue, inputValue));
                    property.minValue = property.defaultMinValue;
                    property.maxValue = property.defaultMaxValue;
                    break;
            }

            // Set the updated audio property value in the sound instance editor object
            soundInstanceEditorObject.SetAudioPropertyValue(property, index, property.outputValue);
        }
    }

    private void SetupEditorObject()
    {
        ResetSoundInstanceEditor();
        switch (editorType)
        {
            case SoundInstanceEditorType.Unity:
                // Create a Unity-based sound instance editor object if an audio clip is available
                if(audioClipReference != null)
                {
                    soundInstanceEditorObject = new SoundInstanceEditorObjectUnity(this);
                }
                break;
            case SoundInstanceEditorType.Fmod:
                // Create an FMOD-based sound instance editor object if an FMOD event is available
                if(!fmodEventReference.Guid.IsNull){
                    soundInstanceEditorObject = new SoundInstanceEditorObjectFmod(this);
                }
                break;
        }
    }

    private void InitializeEditorObject()
    {
        ResetSoundInstanceEditor();
        switch (editorType)
        {
            case SoundInstanceEditorType.Unity:
                if(audioClipReference != null && soundInstanceEditorObject == null)
                {
                    soundInstanceEditorObject = new SoundInstanceEditorObjectUnity(this);
                }
                break;
            case SoundInstanceEditorType.Fmod:
                if(!fmodEventReference.Guid.IsNull && soundInstanceEditorObject == null){
                    soundInstanceEditorObject = new SoundInstanceEditorObjectFmod(this);
                }
                break;
        }
    }

    [CustomEditor(typeof(SoundInstanceEditor))]
    public class SoundInstanceEditorInspector : UnityEditor.Editor
    {
        SoundInstanceEditor SoundInstanceEditor;

        // Serialized properties for FMOD event, Audio Clip, and Reflection Script
        private SerializedProperty eventReferenceProperty;
        private SerializedProperty audioClipProperty;
        private SerializedProperty reflectionScriptProperty;
        
        private void OnEnable()
        {
            SoundInstanceEditor = (SoundInstanceEditor)target;

            // Initialize serialized properties
            eventReferenceProperty = serializedObject.FindProperty("fmodEventReference");
            audioClipProperty = serializedObject.FindProperty("audioClipReference");
            reflectionScriptProperty = serializedObject.FindProperty("reflectionScript");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Update inspector based on play mode status
            if (!Application.isPlaying)
            {
                SoundInstanceEditor.UpdateInspectorAndOnRunning();
                SoundInstanceEditor.UpdateInspectorOnly();
            }

            // Draw default Sound Instance information in the Inspector
            SoundInstanceEditor.DrawInspectorGUIDefaultInfo();

            EditorGUILayout.Space(10);

            // Draw serialized properties (FMOD Event or Unity Audio Clip)
            DrawSerializedProperties();

            EditorGUILayout.Space(10);

            SoundInstanceEditor.DrawInspectorGUI();

            serializedObject.ApplyModifiedProperties();
        }

        
        private void DrawSerializedProperties()
        {   
            EditorGUILayout.LabelField("Sound Instance Properties", EditorStyles.boldLabel);

            // Check the editor type and draw corresponding property fields
            if(SoundInstanceEditor.editorType == SoundInstanceEditorType.Fmod)
            { 
                // Draw FMOD event reference property field
                EditorGUILayout.PropertyField(eventReferenceProperty);

                // Get the selected FMOD event reference and update SoundInstanceEditor
                EventReference eventReference = (EventReference) eventReferenceProperty.GetEventReference();
                SoundInstanceEditor.FmodEventReference = eventReference;
            }
            else if(SoundInstanceEditor.editorType == SoundInstanceEditorType.Unity)
            {   
                // Draw Unity Audio Clip property field
                EditorGUILayout.PropertyField(audioClipProperty);

                // Update SoundInstanceEditor with the selected Unity Audio Clip
                SoundInstanceEditor.AudioClipReference = (AudioClip) audioClipProperty.objectReferenceValue;
            }

            // Display reflection script property field if not in play mode
            if(!Application.isPlaying)
            {
                EditorGUILayout.PropertyField(reflectionScriptProperty);
            }
        }
    }
}
