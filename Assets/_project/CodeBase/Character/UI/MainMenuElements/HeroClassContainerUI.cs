using Code.Hero.Data;
using CodeBase.CharacterIntegration;
using UnityEngine;
using UniRx;
namespace Code.UI.MainMenuElements
{
    public class HeroClassContainerUI : MonoBehaviour
    {
        [SerializeField] private HeroClassUI _itemPrefab;
        [SerializeField] private Transform _container;
        public void Construct(HeroDataFabric draft,CharacterAssets assets)
        {
            foreach(Transform child in _container)Destroy(child.gameObject);
            foreach(var pair in draft.Classes)
            {
                var id=pair.Key;var ui=Instantiate(_itemPrefab,_container);
                ui.SetData(pair.Value,id,assets,()=>draft.SelectClass(id));
                draft.SelectedId.Subscribe(selected=>ui.SetSelected(selected==id)).AddTo(ui);
            }
        }
    }
}

