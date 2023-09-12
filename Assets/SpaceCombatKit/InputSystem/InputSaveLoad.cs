using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


[System.Serializable]
public class InputBindingSerialized
{
    public string id;
    public string overridePath;

    public InputBindingSerialized(string id, string overridePath)
    {
        this.id = id;
        this.overridePath = overridePath;
    }
}

[System.Serializable]
public class InputBindingsSerialized
{
    public List<InputBindingSerialized> bindings = new List<InputBindingSerialized>();
}

public class InputSaveLoad : MonoBehaviour
{

    public InputActionAsset inputActionAsset;

    public string playerPrefsKey = "SCKInputBindingOverrides";


    void SaveBindings()
    {
        Debug.Log("Saving bindings");
        InputBindingsSerialized bindingOverrides = new InputBindingsSerialized();

        foreach(InputActionMap map in inputActionAsset.actionMaps)
        {
            foreach(InputBinding binding in map.bindings)
            {
                if (!string.IsNullOrEmpty(binding.overridePath))
                {
                    bindingOverrides.bindings.Add(new InputBindingSerialized(binding.id.ToString(), binding.overridePath));
                }
            }
        }

        PlayerPrefs.SetString(playerPrefsKey, JsonUtility.ToJson(bindingOverrides));
        PlayerPrefs.Save();

    }

    void LoadBindings()
    {
        if (PlayerPrefs.HasKey(playerPrefsKey))
        {
            Debug.Log("Loading bindings - found key");
            InputBindingsSerialized bindingOverrides = JsonUtility.FromJson(PlayerPrefs.GetString(playerPrefsKey), typeof(InputBindingsSerialized)) as InputBindingsSerialized;

            Dictionary<System.Guid, string> overridesDictionary = new Dictionary<System.Guid, string>();
            foreach (InputBindingSerialized bindingOverride in bindingOverrides.bindings)
            {
                Debug.Log("Loaded binding: " + bindingOverride.id + "  " + bindingOverride.overridePath);
                overridesDictionary.Add(new System.Guid(bindingOverride.id), bindingOverride.overridePath);
            }

            foreach (InputActionMap map in inputActionAsset.actionMaps)
            {
                InputActionReference action = new InputActionReference();
                action.Set(map.actions[0]);

                for (int i = 0; i < map.bindings.Count; ++i)
                {
                    if (overridesDictionary.ContainsKey(map.bindings[i].id))
                    {
                        //map.ApplyBindingOverride(i, new InputBinding { overridePath = overridesDictionary[map.bindings[i].id] });
                    }
                }
            }
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Comma))
        {
            SaveBindings();
        }

        if (Input.GetKeyDown(KeyCode.Period))
        {
            LoadBindings();
        }
    }
}
