using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CodeBase.CharacterIntegration
{
    public sealed class CharacterMainMenuView : MonoBehaviour
    {
        public Button ContinueButton;
        public Button AttributesButton;
        public TMP_Text Status;
        public void Bind(CharacterMenuService service,bool worldReady,bool canContinue,bool hasCharacter)
        {
            ContinueButton.interactable=worldReady||canContinue;
            ContinueButton.GetComponentInChildren<TMP_Text>().text=worldReady?"Вернуться в игру":"Продолжить";
            ContinueButton.onClick.AddListener(service.Continue);
            AttributesButton.gameObject.SetActive(worldReady);
            AttributesButton.interactable=hasCharacter;
            AttributesButton.onClick.AddListener(service.OpenAttributes);
            Status.text=worldReady?"Пауза":canContinue?"Найдено сохранение":"Начните новую игру";
        }
    }
}
