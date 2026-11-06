using DialogueSystemExtensions.Components.Makers;
using PixelCrushers.DialogueSystem;
using UnityEngine;

namespace DialogueSystemExtensions.Components.Sensors
{
    public class PlayerTriggerSensor : TriggerSensor
    {
        [SerializeField]
        private DialogueSystemTrigger _trigger;
        protected override void OnEnter(IDialogueMaker maker)
        {
            _trigger.conversationActor = maker.Transform;
        }

        protected override void OnExit(IDialogueMaker maker)
        {
            _trigger.conversationActor = null;
        }
    }
}
