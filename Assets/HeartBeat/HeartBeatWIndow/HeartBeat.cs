using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class HeartBeat : MonoBehaviour
{
    public static event Action<DateTime, int> OnNotifyHeartBeat;

    //error
    private string _lastError;
    readonly Dictionary<string, Dictionary<string, string>> _devices = new Dictionary<string, Dictionary<string, string>>();
    private const string HeartBeatDefault = "Heart Beat currently not tracked";
    private string _heartBeatValue = HeartBeatDefault;

    //device scan
    private const string DeviceScanInactive = "Scan Devices!";
    private const string DeviceScanOngoing = "Scanning Devices ...";
    private string _currentDeviceScanText = DeviceScanInactive;
    private bool _isScanningDevices;
    private int _selectedDevice;
    private List<string> _deviceOptions = new();
    private List<string> _deviceNames = new();
    
    //service scan
    private const string ServiceScanInactive = "Scan Services!";
    private const string ServiceScanOngoing = "Scanning Services ...";
    private string _currentServiceScanText = ServiceScanInactive;
    private bool _isScanningServices;
    private int _selectedService;
    private List<string> _serviceOptions = new();
    
    //characteristics scan
    private const string CharacteristicsScanInactive = "Scan Characteristics!";
    private const string CharacteristicsScanOngoing = "Scanning Characteristics ...";
    private string _currentCharacteristicsScanText = CharacteristicsScanInactive;
    private bool _isScanningCharacteristics;
    private int _selectedCharacteristics;
    private List<string> _characteristicsOptions = new();

    //subscribe
    private bool _isSubscribed;

    private void Update()
    {
        UpdateBluetooth();
    }

    private void StartStopDeviceScan()
    {
        if (!_isScanningDevices)
        {
            _deviceOptions = new List<string>();
            _deviceNames = new List<string>();
            
            BleApi.StartDeviceScan();
            _isScanningDevices = true;
            _currentDeviceScanText = DeviceScanInactive;
        }
        else
        {
            // stop scan
            _isScanningDevices = false;
            BleApi.StopDeviceScan();
            _currentDeviceScanText = DeviceScanOngoing;
        }
    }

    private void StartCharacteristicScan()
    {
        if (!_isScanningCharacteristics)
        {
            // start new scan
            _characteristicsOptions = new List<string>();
            BleApi.ScanCharacteristics(_deviceOptions[_selectedDevice], _serviceOptions[_selectedService]);
            _isScanningCharacteristics = true;
            _currentCharacteristicsScanText = CharacteristicsScanOngoing;
        }
    }

    private void ResetBluetooth()
    {
        BleApi.Quit();
        
        _isScanningDevices = false;
        _currentDeviceScanText = DeviceScanInactive;
        _selectedDevice = 0;
        _deviceNames = new();
        _deviceOptions = new();

        _isScanningServices = false;
        _currentServiceScanText = ServiceScanInactive;
        _selectedService = 0;
        _serviceOptions = new();

        _isScanningCharacteristics = false;
        _currentCharacteristicsScanText = CharacteristicsScanInactive;
        _selectedCharacteristics = 0;
        _characteristicsOptions = new();

        _heartBeatValue = HeartBeatDefault;
    }

    public void UpdateBluetooth()
    {
        BleApi.ScanStatus status;
        if (_isScanningDevices)
        {
            BleApi.DeviceUpdate res = new BleApi.DeviceUpdate();
            do
            {
                status = BleApi.PollDevice(ref res, false);
                if (status == BleApi.ScanStatus.AVAILABLE)
                {
                    if (!_devices.ContainsKey(res.id))
                        _devices[res.id] = new Dictionary<string, string>() {
                            { "name", "" },
                            { "isConnectable", "False" }
                        };
                    if (res.nameUpdated)
                        _devices[res.id]["name"] = res.name;
                    if (res.isConnectableUpdated)
                        _devices[res.id]["isConnectable"] = res.isConnectable.ToString();
                    // consider only devices which have a name and which are connectable
                    if (_devices[res.id]["name"] != "" && _devices[res.id]["isConnectable"] == "True")
                    {
                        _deviceNames.Add(_devices[res.id]["name"]);
                        _deviceOptions.Add(res.id);
                    }
                }
                else if (status == BleApi.ScanStatus.FINISHED)
                {
                    _isScanningDevices = false;
                    _currentDeviceScanText = DeviceScanInactive;
                }
            } while (status == BleApi.ScanStatus.AVAILABLE);
        }
        if (_isScanningServices)
        {
            BleApi.Service res = new BleApi.Service();
            do
            {
                status = BleApi.PollService(out res, false);
                if (status == BleApi.ScanStatus.AVAILABLE)
                {
                    _serviceOptions.Add(res.uuid);
                }
                else if (status == BleApi.ScanStatus.FINISHED)
                {
                    _isScanningServices = false;
                    _currentServiceScanText = ServiceScanInactive;
                }
            } while (status == BleApi.ScanStatus.AVAILABLE);
        }
        if (_isScanningCharacteristics)
        {
            BleApi.Characteristic res = new BleApi.Characteristic();
            do
            {
                status = BleApi.PollCharacteristic(out res, false);
                if (status == BleApi.ScanStatus.AVAILABLE)
                {
                    _characteristicsOptions.Add(res.uuid);
                }
                else if (status == BleApi.ScanStatus.FINISHED)
                {
                    _isScanningCharacteristics = false;
                    _currentCharacteristicsScanText = CharacteristicsScanInactive;
                }
            } while (status == BleApi.ScanStatus.AVAILABLE);
        }
        if (_isSubscribed)
        {
            BleApi.BLEData res = new BleApi.BLEData();
            BleApi.PollData(out res, false);
            while (BleApi.PollData(out res, false))
            {
                var memoryStream = new MemoryStream(res.buf);
                var binaryReader = new BinaryReader(memoryStream);
                var flags = binaryReader.ReadByte();
                var value = binaryReader.ReadByte();
                _heartBeatValue = "Heart Beat: " + value + " | DateTime: " + DateTime.Now;
                OnNotifyHeartBeat?.Invoke(DateTime.Now, value);
                Debug.Log(flags + " " + value);
            }
        }
        {
            // log potential errors
            BleApi.ErrorMessage res = new BleApi.ErrorMessage();
            BleApi.GetError(out res);
            if (_lastError != res.msg)
            {
                Debug.LogWarning("ErrorMessage: " + res.msg);
                _lastError = res.msg;
            }
        }
    }

    private void Subscribe()
    {
        // no error code available in non-blocking mode
        BleApi.SubscribeCharacteristic(
            _deviceOptions[_selectedDevice], 
            _serviceOptions[_selectedService], 
            _characteristicsOptions[_selectedCharacteristics],
            false
            );
        
        _isSubscribed = true;
    }

    private void StartServiceScan()
    {
        if (!_isScanningServices)
        {
            // start new scan
            _serviceOptions = new List<string>();
            BleApi.ScanServices(_deviceOptions[_selectedDevice]);
            _isScanningServices = true;
            _currentServiceScanText = ServiceScanOngoing;
        }
    }

    public void InspectorGUI()
    {
        GUILayout.Label("Heart Beat: Service ID = 0x180D | Characteristics ID = 0x2a37", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Reset Bluetooth"))
        {
            ResetBluetooth();
        }
        EditorGUILayout.Space();

        if (GUILayout.Button(_currentDeviceScanText))
        {
            StartStopDeviceScan();
        }
        _selectedDevice = EditorGUILayout.Popup("Found Devices", _selectedDevice, _deviceNames.ToArray());
        EditorGUILayout.Space();
        
        if (GUILayout.Button(_currentServiceScanText))
        {
            StartServiceScan();
        }
        _selectedService = EditorGUILayout.Popup("Found Services", _selectedService, _serviceOptions.ToArray());
        EditorGUILayout.Space();
        
        if (GUILayout.Button(_currentCharacteristicsScanText))
        {
            StartCharacteristicScan();
        }
        _selectedCharacteristics = EditorGUILayout.Popup("Found Characteristics", _selectedCharacteristics, _characteristicsOptions.ToArray());
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Subscribe!"))
        {
            Subscribe();
        }
        EditorGUILayout.Space();
        
        GUILayout.Label("ErrorMessage: " + _lastError, EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        GUILayout.Label(_heartBeatValue, EditorStyles.boldLabel);
    }
}

[CustomEditor(typeof(HeartBeat))]
public class HeartBeatEditor: UnityEditor.Editor
{
    private HeartBeat _heartBeat;

    private void OnEnable()
    {
        _heartBeat = (HeartBeat)target;
    }

    public override void OnInspectorGUI()
    {
        _heartBeat.InspectorGUI();

        if(!Application.isPlaying)
        {
            _heartBeat.UpdateBluetooth();                
        }
            
    }
}
