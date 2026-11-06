using UnityEngine;

namespace DialogueSystemExtensions.Components.Makers
{
    public class DialogueMaker: MonoBehaviour, IDialogueMaker
    {
       public Transform Transform => this.gameObject.transform;
    }
}
