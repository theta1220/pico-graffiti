
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UnityEngine
{
    public class ExclusiveInput
    {
        public static bool IsFocusedOnInputField
        {
            get
            {
                return EventSystem.current?.currentSelectedGameObject?.GetComponent<InputField>() != null;
            }
        }
        //
        // Summary:
        //     Returns true the first frame the user hits any key or mouse button. (Read Only)
        public static bool anyKeyDown
        {
            get
            {
                return !IsFocusedOnInputField && Input.anyKeyDown;
            }
        }
        //
        // Summary:
        //     Is any key or mouse button currently held down? (Read Only)
        public static bool anyKey
        {
            get
            {
                return !IsFocusedOnInputField && Input.anyKey;
            }
        }
        //
        // Summary:
        //     Returns the value of the virtual axis identified by axisName.
        //
        // Parameters:
        //   axisName:
        public static float GetAxis(string axisName)
        {
            return IsFocusedOnInputField ? 0 : Input.GetAxis(axisName);
        }
        //
        // Summary:
        //     Returns the value of the virtual axis identified by axisName with no smoothing
        //     filtering applied.
        //
        // Parameters:
        //   axisName:
        public static float GetAxisRaw(string axisName)
        {
            return IsFocusedOnInputField ? 0 : Input.GetAxisRaw(axisName);
        }
        //
        // Summary:
        //     Returns true while the user holds down the key identified by name.
        //
        // Parameters:
        //   name:
        public static bool GetKey(string name)
        {
            return !IsFocusedOnInputField && Input.GetKey(name);
        }
        //
        // Summary:
        //     Returns true while the user holds down the key identified by the key KeyCode
        //     enum parameter.
        //
        // Parameters:
        //   key:
        public static bool GetKey(KeyCode key)
        {
            return !IsFocusedOnInputField && Input.GetKey(key);
        }
        //
        // Summary:
        //     Returns true during the frame the user starts pressing down the key identified
        //     by name.
        //
        // Parameters:
        //   name:
        public static bool GetKeyDown(string name)
        {
            return !IsFocusedOnInputField && Input.GetKeyDown(name);
        }
        //
        // Summary:
        //     Returns true during the frame the user starts pressing down the key identified
        //     by the key KeyCode enum parameter.
        //
        // Parameters:
        //   key:
        public static bool GetKeyDown(KeyCode key)
        {
            return !IsFocusedOnInputField && Input.GetKeyDown(key);
        }
        //
        // Summary:
        //     Returns true during the frame the user releases the key identified by the key
        //     KeyCode enum parameter.
        //
        // Parameters:
        //   key:
        public static bool GetKeyUp(KeyCode key)
        {
            return !IsFocusedOnInputField && Input.GetKeyUp(key);
        }
        //
        // Summary:
        //     Returns true during the frame the user releases the key identified by name.
        //
        // Parameters:
        //   name:
        public static bool GetKeyUp(string name)
        {
            return !IsFocusedOnInputField && Input.GetKeyUp(name);
        }
    }
}
