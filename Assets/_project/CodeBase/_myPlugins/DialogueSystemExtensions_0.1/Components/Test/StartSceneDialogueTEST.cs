using PixelCrushers.DialogueSystem;
using UnityEngine;

namespace DialogueSystemExtensions.Components.Test
{
    public class StartSceneDialogueTEST: MonoBehaviour
    {
        private void Start()
        {
            DialogueManager.instance.StartConversation("DialogueUI_TEST_Scene");
        }
    }
}
