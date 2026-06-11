using UnityEngine;
using Simcity.Core;

namespace Simcity.World
{
    /// <summary>Dev-only conveniences for testing. F9 deletes the saved character so
    /// the creator shows again next time you enter Play. (Removed before shipping.)</summary>
    public class DevTools : MonoBehaviour
    {
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F9))
                SaveSystem.Delete();
        }
    }
}
