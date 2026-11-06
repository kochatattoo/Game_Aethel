using System;
using System.Collections.Generic;
using CodeBase.Data;


namespace CodeBase.Infrastructure.Services.SaveLoad
{
    public interface ISaveSlotService
    {
        string ActiveSlotId { get; }
        string Notice { get; }
        bool HasPendingProfile { get; }
        void InitializeStorage(Func<PlayerProgress, bool> validator, string safeLevel);
        IReadOnlyList<SaveSlotInfo> GetSlots();
        void PrepareNewProfile(string slotId);
        void CancelPendingProfile();
        void SelectSlot(string slotId);
        void SaveToSlot(string slotId);
        void DeleteSlot(string slotId);
    }
}
