using Code.Hero.Attributes;
using Code.Hero.Data;
using Code.Hero.Items;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI.Elements
{
    public class ConsumableItemUI : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _descriptionText;
        [SerializeField] private TextMeshProUGUI _statsText;
        [SerializeField] private Button _useButton;
        [SerializeField] private TextMeshProUGUI _buttonText;

        private readonly CompositeDisposable _disposables = new();

        public void Construct(ConsumableItem item, HeroData heroData)
        {
            _disposables.Clear();

            _icon.sprite = item.Icon;
            _nameText.text = item.ItemName;
            _descriptionText.text = item.Description;
            _buttonText.text = "Использовать";

            _statsText.text = FormatModifiersWithDuration(item);

            _useButton.onClick.AsObservable()
                .Subscribe(_ =>
                {
                    // Применяем эффект через сервис баффов
                    heroData.Buffs.ApplyBuff(heroData.Hero, item);

                    // Тут можно добавить логику уничтожения предмета из инвентаря
                    // Destroy(gameObject); 
                })
                .AddTo(_disposables);
        }

        private string FormatModifiersWithDuration(ConsumableItem item)
        {
            var sb = new System.Text.StringBuilder();

            foreach (var mod in item.Modifiers)
            {
                string sign = mod.Amount >= 0 ? "+" : "";
                string type = mod.Type == ModifierType.Percantage ? "%" : "";
                string attrName = mod.Attribute.ToFriendlyName();

                sb.AppendLine($"{sign}{mod.Amount}{type} {attrName}");
            }

            sb.AppendLine($"<color=#5CC9FF>Длительность: {item.Duration} сек.</color>");

            return sb.ToString().TrimEnd();
        }

        private void OnDestroy() => _disposables.Dispose();
    }
}
