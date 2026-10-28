using Code.Hero.Attributes;
using Code.Hero.Data;
using Code.Hero.Items;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI.Elements
{
    public class EquipmentItemUI : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _dexcriptionText;
        [SerializeField] private TextMeshProUGUI _statsText;
        [SerializeField] private Button _actionButton;
        [SerializeField] private TextMeshProUGUI _buttonText;

        private readonly CompositeDisposable _disposables = new();

        public void Construct(EquipmentItem item, HeroData heroData)
        {
            _disposables.Clear();

            _icon.sprite = item.Icon;
            _nameText.text = item.ItemName;
            _dexcriptionText.text = item.Description;
            _statsText.text = FormatModifiers(item);

            heroData.Equipment.Slots
                .ObserveReplace() // Когда один предмет заменил другой в слоте
                .AsUnitObservable()
                .Merge(heroData.Equipment.Slots.ObserveAdd().AsUnitObservable())
                .Merge(heroData.Equipment.Slots.ObserveRemove().AsUnitObservable())
                .StartWith(Unit.Default)
                .Subscribe(_ =>
                {
                    bool isEquipped = heroData.Equipment.IsEquipped(item);
                    _buttonText.text = isEquipped ? "Снять" : "Надеть";
                })
                .AddTo(_disposables);

            _actionButton.onClick.AsObservable()
                .Subscribe(_ =>
                {
                    if (heroData.Equipment.IsEquipped(item))
                        heroData.Equipment.Unequip(heroData.Hero, item);
                    else
                        heroData.Equipment.Equip(heroData.Hero, item);
                })
                .AddTo(_disposables);
        }

        private void OnDestroy() => _disposables.Dispose();

        private string FormatModifiers(EquipmentItem item)
        {
            var sb = new System.Text.StringBuilder();

            foreach (var mod in item.Modifiers)
            {
                string sign = mod.Amount >= 0 ? "+" : "";
                string color = mod.Amount >= 0 ? "green" : "red";
                string type = mod.Type == ModifierType.Percantage ? "%" : "";

                // Получаем красивое имя атрибута (например, STR -> Сила)
                string attrName = mod.Attribute.ToFriendlyName();

                sb.AppendLine($"<color={color}>{sign}{mod.Amount}{type}</color> {attrName}"); 
            }

            return sb.ToString().TrimEnd();
        }

    }
}
