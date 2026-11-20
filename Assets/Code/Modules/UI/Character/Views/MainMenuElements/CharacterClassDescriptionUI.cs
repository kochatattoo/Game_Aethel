using Code.Character.Data;
using TMPro;
using UniRx;
using UnityEngine;

namespace Code.UI.MainMenuElements
{
    public class CharacterClassDescriptionUI : MonoBehaviour
    { 
        // При обновлении выбранного класса - обновляем описание поля из CharacterClass.Description
        // Пусть отслеживает через поток

        [SerializeField] private TextMeshProUGUI _descriptionField;

        public void Construct(CharacterDraft heroData)
        {
            heroData.SelectedClass
                .Subscribe(hc => _descriptionField.text = hc?.Description ?? "")
                .AddTo(this);
        }
    }
}
