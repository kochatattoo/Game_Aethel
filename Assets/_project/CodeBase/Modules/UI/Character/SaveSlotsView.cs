using System;
using System.Collections.Generic;
using Code.Hero.Data;
using CodeBase.Infrastructure.Services.SaveLoad;

using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CodeBase.CharacterIntegration
{
    public sealed class SaveSlotsView : MonoBehaviour
    {
        private ISaveSlotService _slots;
        private bool _playing, _newGame, _busy;
        private Action<string> _create, _load;
        private IReadOnlyDictionary<int,HeroClass> _classes;
        private RectTransform _panel;
        private TMP_Text _status;
        private readonly List<GameObject> _rows = new();
        private GameObject _confirmation;

        public void Construct(ISaveSlotService slots, bool playing, bool newGame,
            Action<string> create, Action<string> load, Action close, IReadOnlyDictionary<int,HeroClass> classes)
        {
            _classes=classes;
            _slots=slots; _playing=playing; _newGame=newGame; _create=create; _load=load;
            var canvas=gameObject.AddComponent<Canvas>();
            canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=200;
            var scaler=gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution=new Vector2(1920,1080);scaler.matchWidthOrHeight=.5f;
            gameObject.AddComponent<GraphicRaycaster>();
            var backdrop=Box(transform,"Backdrop",Vector2.zero,new Vector2(1920,1080),new Color(.025f,.025f,.035f,.97f));
            backdrop.anchorMin=Vector2.zero;backdrop.anchorMax=Vector2.one;backdrop.offsetMin=backdrop.offsetMax=Vector2.zero;
            _panel=Box(transform,"Panel",Vector2.zero,new Vector2(1380,880),new Color(.09f,.08f,.09f,1));
            Label(_panel,"Title",newGame?"Новая игра — выберите слот":"Сохранения",new Vector2(0,370),new Vector2(1280,70),38);
            _status=Label(_panel,"Status","",new Vector2(0,-300),new Vector2(1260,90),24);
            MakeButton(_panel,"Назад",new Vector2(0,-395),new Vector2(280,60),close);
            Render();
        }

        private void Render()
        {
            foreach(var row in _rows){row.SetActive(false);Destroy(row);}_rows.Clear();
            var list=_slots.GetSlots();
            for(var i=0;i<list.Count;i++)
            {
                var slot=list[i];
                var row=Box(_panel,slot.Id,new Vector2(0,210-i*175),new Vector2(1300,155),new Color(.15f,.13f,.14f,1));
                _rows.Add(row.gameObject);
                var description="Слот "+(i+1)+(slot.Id==_slots.ActiveSlotId?" • текущий":"");
                if(slot.CanLoad)
                {
                    var character=slot.Data.Profile.Character;
                    var name=character!=null&&_classes.TryGetValue(character.ClassId,out var definition)?definition.Name:"Старый персонаж";
                    description+="   |   "+name+"   |   "+slot.Data.Level.LevelId;
                    if(DateTimeOffset.TryParse(slot.SavedAtUtc,out var date))description+="\n"+date.ToLocalTime().ToString("dd.MM.yyyy HH:mm:ss");
                }
                description+="\n"+slot.Message;
                var label=Label(row,"Description",description,new Vector2(-280,0),new Vector2(680,135),24);
                label.alignment=TextAlignmentOptions.MidlineLeft;
                if(_newGame)
                {
                    MakeButton(row,slot.State==SaveSlotStatus.Empty?"Создать":"Заменить",new Vector2(410,0),new Vector2(320,64),()=>
                        ConfirmIfOccupied(slot,"Заменить содержимое слота новым героем? Запись произойдёт после создания мира.",()=>_create(slot.Id)));
                }
                else
                {
                    MakeButton(row,"Загрузить",new Vector2(170,0),new Vector2(165,60),()=>Run(()=>_load(slot.Id))).interactable=slot.CanLoad;
                    MakeButton(row,"Сохранить",new Vector2(355,0),new Vector2(165,60),()=>
                        ConfirmIfOccupied(slot,"Перезаписать выбранный слот текущим персонажем?",()=>_slots.SaveToSlot(slot.Id))).interactable=_playing;
                    MakeButton(row,"Удалить",new Vector2(540,0),new Vector2(165,60),()=>
                        Confirm("Удалить выбранный слот? Он исчезнет из списка сохранений.",()=>_slots.DeleteSlot(slot.Id)))
                        .interactable=slot.State!=SaveSlotStatus.Empty && !(_playing&&slot.Id==_slots.ActiveSlotId);
                }
            }
            _status.text=_newGame?"Создание можно отменить: сохранение останется прежним.":
                _playing?"Сохраняется текущий персонаж. Активный слот можно удалить после выхода из игры.":
                "Выберите сохранение для загрузки. Повреждённые и несовместимые данные не загружаются.";
        }

        private void ConfirmIfOccupied(SaveSlotInfo slot,string text,Action action)
        {
            if(slot.State==SaveSlotStatus.Empty)Run(action);else Confirm(text,action);
        }

        private void Confirm(string text,Action action)
        {
            if(_confirmation!=null||_busy)return;
            var overlay=Box(transform,"Confirm",Vector2.zero,new Vector2(1920,1080),new Color(0,0,0,.85f));
            overlay.anchorMin=Vector2.zero;overlay.anchorMax=Vector2.one;overlay.offsetMin=overlay.offsetMax=Vector2.zero;
            _confirmation=overlay.gameObject;
            var dialog=Box(overlay,"Dialog",Vector2.zero,new Vector2(850,350),new Color(.18f,.13f,.13f,1));
            Label(dialog,"Message",text,new Vector2(0,65),new Vector2(760,180),30);
            MakeButton(dialog,"Подтвердить",new Vector2(-190,-110),new Vector2(320,65),()=>{CloseConfirmation();Run(action);});
            MakeButton(dialog,"Отмена",new Vector2(190,-110),new Vector2(320,65),CloseConfirmation);
        }
        private void CloseConfirmation(){if(_confirmation!=null){_confirmation.SetActive(false);Destroy(_confirmation);}_confirmation=null;}
        private void Run(Action operation)
        {
            if(_busy)return;_busy=true;
            try{operation();if(this!=null&&gameObject.activeInHierarchy){Render();_status.text="Операция завершена";}}
            catch(Exception error){Debug.LogWarning("Сохранения: "+error.Message);if(_status!=null)_status.text=error.Message;}
            finally{_busy=false;}
        }
        private static RectTransform Box(Transform parent,string name,Vector2 position,Vector2 size,Color color)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(Image));go.transform.SetParent(parent,false);
            var rect=(RectTransform)go.transform;rect.anchorMin=rect.anchorMax=rect.pivot=new Vector2(.5f,.5f);rect.anchoredPosition=position;rect.sizeDelta=size;
            go.GetComponent<Image>().color=color;return rect;
        }
        private static TMP_Text Label(Transform parent,string name,string caption,Vector2 position,Vector2 size,float fontSize)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(TextMeshProUGUI));go.transform.SetParent(parent,false);
            var rect=(RectTransform)go.transform;rect.anchorMin=rect.anchorMax=new Vector2(.5f,.5f);rect.sizeDelta=size;rect.anchoredPosition=position;
            var label=go.GetComponent<TextMeshProUGUI>();label.text=caption;label.fontSize=fontSize;label.alignment=TextAlignmentOptions.Center;label.color=Color.white;label.raycastTarget=false;
            return label;
        }
        private static Button MakeButton(Transform parent,string caption,Vector2 position,Vector2 size,Action action)
        {
            var rect=Box(parent,caption,position,size,new Color(.35f,.12f,.11f,1));
            var button=rect.gameObject.AddComponent<Button>();button.onClick.AddListener(()=>action());
            Label(rect,"Label",caption,Vector2.zero,size-new Vector2(10,4),24);return button;
        }
    }
}
