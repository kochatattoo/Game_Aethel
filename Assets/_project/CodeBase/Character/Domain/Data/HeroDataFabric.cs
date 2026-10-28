using System;
using System.Collections.Generic;
using UniRx;
namespace Code.Hero.Data
{
    /// <summary>Независимый черновик создания героя; каталог не изменяется.</summary>
    public sealed class HeroDataFabric : IDisposable
    {
        private readonly IHeroDataProvider _provider;
        private readonly Dictionary<string,int> _allocations=new();
        private readonly ReadOnlyReactiveProperty<HeroClass> _selectedClass;
        public ReactiveDictionary<int,HeroClass> Classes { get; }=new();
        public ReactiveProperty<int> SelectedId { get; }=new(-1);
        public IntReactiveProperty BonusPoints { get; }=new(10);
        public IReadOnlyReactiveProperty<HeroClass> SelectedClass=>_selectedClass;
        public bool CanConfirm => SelectedClass.Value != null && BonusPoints.Value==0;
        public HeroDataFabric(IHeroDataProvider provider)
        {
            _provider=provider;
            _selectedClass=SelectedId.Select(id=>Classes.TryGetValue(id,out var hero)?hero:null).ToReadOnlyReactiveProperty();
        }
        public void Initialize() { foreach(var p in _provider.HeroClassesReadonly) Classes.Add(p.Key,p.Value.Clone()); }
        public void SelectClass(int id)
        {
            if(SelectedId.Value==id || !Classes.ContainsKey(id)) return;
            var old=SelectedClass.Value;
            if(old!=null) foreach(var p in _allocations) old.Attributes.AddBaseValue(p.Key,-p.Value);
            _allocations.Clear(); BonusPoints.Value=10; SelectedId.Value=id;
        }
        public int Allocated(string key)=>_allocations.TryGetValue(key,out var v)?v:0;
        public bool TryAllocate(string key,int delta)
        {
            if(SelectedClass.Value==null || !SelectedClass.Value.Attributes.AllAttributes.ContainsKey(key) || (delta!=1 && delta!=-1)) return false;
            if(delta>0 && BonusPoints.Value<=0 || delta<0 && Allocated(key)<=0) return false;
            _allocations[key]=Allocated(key)+delta; SelectedClass.Value.Attributes.AddBaseValue(key,delta); BonusPoints.Value-=delta;return true;
        }
        public HeroData CreateHeroData(CharacterBalance balance=null)
        {
            if(!CanConfirm) throw new InvalidOperationException("Распределите все очки и выберите класс");
            return new HeroData(SelectedClass.Value.Clone(),balance);
        }
        public void Dispose()
        {
            _selectedClass.Dispose();foreach(var hero in Classes.Values) hero.Dispose();Classes.Dispose();SelectedId.Dispose();BonusPoints.Dispose();
        }
    }
}

