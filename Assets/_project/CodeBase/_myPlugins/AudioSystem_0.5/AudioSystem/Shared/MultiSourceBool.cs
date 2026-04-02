using System;
using System.Collections.Generic;
using UniRx;
// NOTE: класс плохо подходит под Save/Load систему, подумать в сторону дополнительной поддержки 
/// <summary>
/// Мульти-флаг для управления логическими состояниями из разных источников.<br/>
/// Например: несколько систем могут включать/выключать блок спринта.<br/>
/// Истинное значение зависит от того, есть ли активные владельцы true / false
/// и от выбранной политики доминирования (TrueWins / FalseWins).
/// </summary>
public sealed class MultiSourceBool : IReadOnlyMultiSourceBool, IDisposable
{
    private readonly Dictionary<object, int> _trueOwners = new();
    private readonly Dictionary<object, int> _falseOwners = new();
    private readonly ReactiveProperty<bool> _value;
    private readonly bool _defaultValue;
    private readonly MultiBoolDominance _dominance;

    public MultiSourceBool(bool defaultValue = false, MultiBoolDominance dominance = MultiBoolDominance.TrueWins)
    {
        _defaultValue = defaultValue;
        _dominance = dominance;
        _value = new ReactiveProperty<bool>(_defaultValue);
    }

    /// <summary>
    /// Текущее вычисленное значение  
    /// </summary>
    public bool Current => _value.Value;

    /// <summary>
    /// Текущее значение в обертке IReadOnlyReactiveProperty
    /// </summary>
    public IReadOnlyReactiveProperty<bool> Value => _value;

    /// <summary>
    /// Устанавливает состояние от конкретного владельца.<br/>
    /// Если state = true, владелец попадает в список trueOwners,
    /// при этом автоматически убирается из falseOwners.
    /// </summary>
    public void Set(object owner, bool state)
    {
        if (owner == null)
            return;

        Inc(state ? _trueOwners : _falseOwners, owner);
        ClearOwnerDict(state ? _falseOwners : _trueOwners, owner);
        Recalc();
    }

    /// <summary>
    /// Установить true от владельца 
    /// </summary>
    public void SetTrue(object owner) => Set(owner, true);

    /// <summary>
    /// Установить false от владельца
    /// </summary>
    public void SetFalse(object owner) => Set(owner, false);

    /// <summary>
    /// Полностью убрать владельца (если он мог держать и true, и false).
    /// </summary>
    public void Release(object owner)
    {
        if (owner == null)
            return;

        Dec(_trueOwners, owner);
        Dec(_falseOwners, owner);
        Recalc();
    }

    /// <summary>
    /// Убрать только true-владельца (если владелец держал true)
    /// </summary>
    public void ReleaseTrue(object owner)
    {
        if (owner == null)
            return;

        Dec(_trueOwners, owner);
        Recalc();
    }

    /// <summary>
    /// Убрать только false-владельца (если владелец держал false) 
    /// </summary>
    public void ReleaseFalse(object owner)
    {
        if (owner == null)
            return;

        Dec(_falseOwners, owner);
        Recalc();
    }

    /// <summary>
    /// Проверить, активен ли владелец как источник true
    /// </summary>
    public bool IsOwnerTrue(object owner) => owner != null && _trueOwners.ContainsKey(owner);

    /// <summary>
    /// Проверить, активен ли владелец как источник false
    /// </summary>
    public bool IsOwnerFalse(object owner) => owner != null && _falseOwners.ContainsKey(owner);

    /// <summary>
    /// Полностью очистить всех владельцев и вернуть значение к дефолтному.
    /// </summary>
    public void ClearAll()
    {
        if (_trueOwners.Count == 0 && _falseOwners.Count == 0)
            return;

        _trueOwners.Clear();
        _falseOwners.Clear();
        _value.Value = _defaultValue;
    }

    private static void Inc(Dictionary<object, int> dict, object owner)
    {
        var c = dict.GetValueOrDefault(owner, 0);
        dict[owner] = c + 1;
    }

    private static void Dec(Dictionary<object, int> dict, object owner)
    {
        if (!dict.TryGetValue(owner, out var c))
            return;

        if (--c <= 0)
            dict.Remove(owner);
        else
            dict[owner] = c;
    }

    private static void ClearOwnerDict(Dictionary<object, int> dict, object owner)
    {
        dict.Remove(owner);
    }

    private void Recalc()
    {
        var hasT = _trueOwners.Count > 0;
        var hasF = _falseOwners.Count > 0;

        if (hasT && hasF)
        {
            _value.Value = _dominance == MultiBoolDominance.TrueWins;
            return;
        }

        if (hasT)
        {
            _value.Value = true;
            return;
        }

        if (hasF)
        {
            _value.Value = false;
            return;
        }

        _value.Value = _defaultValue;
    }

    public void Dispose()
    {
        ClearAll();
        _value?.Dispose();
    }
}
