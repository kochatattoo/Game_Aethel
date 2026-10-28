using Code.Hero.Data;
using TMPro;
using UniRx;
using UnityEngine;

namespace Code.UI.MainMenuElements
{
    public class HeroClassDescriptionUI : MonoBehaviour
    { 
        // При обновлении выбранного класса - обновляем описание поля из HeroClass.Description
        // Пусть отслеживает через поток

        [SerializeField] private TextMeshProUGUI _descriptionField;

        public void Construct(HeroDataFabric heroData)
        {
            heroData.SelectedClass
                .Subscribe(hc => _descriptionField.text = hc?.Description ?? "")
                .AddTo(this);
        }
    }
}
