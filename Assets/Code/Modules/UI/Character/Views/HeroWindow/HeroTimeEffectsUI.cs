using Code.Hero.Data;
using Code.Hero.Items;
using Code.UI.Elements;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace Code.UI.HeroWindow
{
    public class HeroTimeEffectsUI : MonoBehaviour
    {
        [SerializeField] private BuffIconUI _prefab;
        [SerializeField] private Transform _container;

        private readonly Dictionary<ActiveBuff, BuffIconUI> _activeIcons = new();
        private CompositeDisposable _disposables = new();

        public void Construct(HeroData heroData, CompositeDisposable windowLifetime)
        {
            _disposables.Dispose();
            _disposables = new CompositeDisposable();
            foreach(var icon in _activeIcons.Values) if(icon != null) Destroy(icon.gameObject);
            _activeIcons.Clear();

            // Слушаем появление новых эффектов
            heroData.Buffs.ActiveBuffs
                .ObserveAdd()
                .Subscribe(addEvent => CreateIcon(addEvent.Value))
                .AddTo(_disposables);

            // Слушаем удаление эффектов (когда таймер в сервисе вышел)
            heroData.Buffs.ActiveBuffs
                .ObserveRemove()
                .Subscribe(removeEvent => RemoveIcon(removeEvent.Value))
                .AddTo(_disposables);

            // Отрисовываем то, что уже висит на герое при открытии окна
            foreach (var buff in heroData.Buffs.ActiveBuffs)
                CreateIcon(buff);

            _disposables.AddTo(windowLifetime);
        }

        private void CreateIcon(ActiveBuff buff)
        {
            if (_activeIcons.ContainsKey(buff)) return;

            var icon = Instantiate(_prefab, _container);
            icon.Construct(buff);
            _activeIcons.Add(buff, icon);
        }

        private void RemoveIcon(ActiveBuff buff)
        {
            if (_activeIcons.TryGetValue(buff, out var icon))
            {
                Destroy(icon.gameObject);
                _activeIcons.Remove(buff);
            }
        }

        private void OnDestroy() => _disposables.Dispose();
    }
}

