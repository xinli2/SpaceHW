using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using VSX.UniversalVehicleCombat;

public class ControlsMenuBuilder : MonoBehaviour
{
    [SerializeField]
    protected List<InputActionAsset> inputAssets = new List<InputActionAsset>();

    [SerializeField]
    protected List<string> controlSchemeKeywords = new List<string>();

    [SerializeField]
    protected List<string> excludedMaps = new List<string>();

    [SerializeField]
    protected ControlsMenuGroupItem controlsMenuGroupItemPrefab;

    [SerializeField]
    protected ControlsMenuItem controlsMenuItemPrefab;

    [SerializeField]
    protected Transform controlsMenuItemsParent;

    // Start is called before the first frame update
    void Start()
    {
        foreach (InputActionAsset inputAsset in inputAssets)
        {
            foreach (InputActionMap map in inputAsset.actionMaps)
            {
                bool excluded = false;
                foreach (string excludedMap in excludedMaps)
                {
                    if (map.name == excludedMap) excluded = true; break;
                }
                if (excluded) continue;

                // Add a group
                ControlsMenuGroupItem groupItemController = (ControlsMenuGroupItem)Instantiate(controlsMenuGroupItemPrefab, controlsMenuItemsParent);
                groupItemController.Set(map.name);

                foreach (InputAction action in map.actions)
                {
                    for (int i = 0; i < action.bindings.Count; ++i)
                    {
                        if (IsRelevantControlScheme(action.bindings[i]))
                        {
                            string control = action.GetBindingDisplayString(i);
                            if (action.bindings[i].isPartOfComposite)
                            {
                                ControlsMenuItem itemController = (ControlsMenuItem)Instantiate(controlsMenuItemPrefab, controlsMenuItemsParent);
                                itemController.Set(action.name + " " + TidyBindingName(action, action.bindings[i].name), control);
                            }
                            else if (!action.bindings[i].isComposite)
                            {
                                ControlsMenuItem itemController = (ControlsMenuItem)Instantiate(controlsMenuItemPrefab, controlsMenuItemsParent);
                                itemController.Set(action.name, control);
                            }
                        }
                    }
                }
            }
        }
    }

    protected virtual bool IsRelevantControlScheme(InputBinding binding)
    {
        if (controlSchemeKeywords.Count == 0) return true;

        foreach(string keyword in controlSchemeKeywords)
        {
            if (binding.groups.Contains(keyword))
            {
                return true;
            }
        }

        return false;
    }

    protected string TidyBindingName(InputAction action, string bindingName)
    {

        bindingName = bindingName.Replace("up", "Up");
        bindingName = bindingName.Replace("down", "Down");
        bindingName = bindingName.Replace("left", "Left");
        bindingName = bindingName.Replace("right", "Right");
        
        if (action.name.Contains("Roll"))
        {
            bindingName = bindingName.Replace("positive", "Left");
            bindingName = bindingName.Replace("negative", "Right");
        }
        else if (action.name.Contains("Throttle"))
        {
            bindingName = bindingName.Replace("positive", "Up");
            bindingName = bindingName.Replace("negative", "Down");
        }
        else if (action.name.Contains("Move"))
        {
            bindingName = bindingName.Replace("Up", "Forward");
            bindingName = bindingName.Replace("Down", "Back");
        }

        return bindingName;
    }
}
