using Code.Character.Data;
using TMPro;
using UniRx;
using UnityEngine;
namespace Code.UI.MainMenuElements
{
    public class CharacterAttributesView : MonoBehaviour
    {
        [SerializeField] private DistributePoints[] _statRows;
        [SerializeField] private TextMeshProUGUI _globalPointsText;
        public void Construct(CharacterParametersUI parameters,CharacterDraft draft,CompositeDisposable lifetime)
        {
            parameters.Construct(draft,lifetime);
            draft.BonusPoints.Subscribe(v=>_globalPointsText.text=$"ОЧКИ: {v}").AddTo(lifetime);
            draft.SelectedClass.Subscribe(hero=>{if(hero!=null)foreach(var row in _statRows)row.Bind(hero,draft,lifetime);}).AddTo(lifetime);
        }
    }
}

