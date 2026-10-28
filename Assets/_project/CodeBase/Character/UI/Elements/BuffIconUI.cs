using Code.Hero.Items;
using System;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI.Elements
{
    public class BuffIconUI : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private Image _border;
        [SerializeField] private TextMeshProUGUI _timerText;

        private readonly CompositeDisposable _disposables = new();

        public void Construct(ActiveBuff buff)
        {
            _disposables.Clear(); // Очистка при переиспользовании (если есть пул объектов)

            _icon.sprite = buff.SourceItem.Icon;

            // Цвет рамки (проверяем первый модификатор)
            bool isPositive = buff.SourceItem.Modifiers[0].Amount >= 0;
            _border.color = isPositive ? Color.green : Color.red;

            // Поток обновления таймера
            Observable.EveryUpdate()
                .Subscribe(_ =>
                {
                    float elapsed = Time.time - buff.StartTime;
                    float remaining = Math.Max(0, buff.SourceItem.Duration - elapsed);

                    // Обновление текста
                    _timerText.text = $"{remaining:F1}с";

                    // Логика мигания (если осталось меньше 2 секунд)
                    if (remaining < 2f && remaining > 0)
                    {
                        // Синусоида для плавного мигания альфа-каналом или цветом
                        float alpha = 0.5f + Mathf.Sin(Time.time * 15f) * 0.5f;
                        _timerText.color = new Color(1, 1, 1, alpha);
                        _border.color = new Color(_border.color.r, _border.color.g, _border.color.b, alpha);
                    }

                    // Самозавершение потока не требуется, так как 
                    // HeroTimeEffectsUI уничтожит объект, и сработает OnDestroy
                })
                .AddTo(_disposables); // гарантирует, что если бафф удалится раньше времени (например, выпили антидот), расчет мгновенно прекратится.
        }

        private void OnDestroy() => _disposables.Dispose();
    }
}
