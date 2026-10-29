using System;
using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace PetProject.SaveStorage.Tests
{
    public sealed class JsonSlotStoreTests
    {
        [Serializable] public sealed class Payload { public int Coins; public string Name; }
        private string _root, _profile;
        private JsonSlotStore<Payload> _store;
        [SetUp] public void SetUp()
        {
            _root=Path.Combine(Path.GetTempPath(),"PetProject-SaveTests",Guid.NewGuid().ToString("N"));
            _profile=Guid.NewGuid().ToString("N");
            _store=new JsonSlotStore<Payload>(_root,p=>JsonUtility.ToJson(p),JsonUtility.FromJson<Payload>,p=>p.Coins>=0&&!string.IsNullOrEmpty(p.Name));
        }
        [TearDown] public void TearDown(){if(Directory.Exists(_root))Directory.Delete(_root,true);}
        private void Save(int coins=10)=>_store.Write("slot-1",_profile,new Payload{Coins=coins,Name="Герой"});

        [Test] public void EmptyStoreListsThreeEmptySlots()=>Assert.That(_store.List().TrueForAll(x=>x.State==SaveSlotState.Empty));
        [Test] public void RoundTripIsDetachedAndRetainsProfileIdentity()
        {
            Save();var first=_store.Read("slot-1");first.Data.Coins=999;
            var second=_store.Read("slot-1");Assert.That(second.Data.Coins,Is.EqualTo(10));Assert.That(second.Envelope.ProfileId,Is.EqualTo(_profile));
        }
        [Test] public void CorruptPrimaryRecoversLastGoodBackup()
        {
            Save(10);Save(20);File.WriteAllText(_store.FilePath("slot-1"),"{broken");
            var recovered=_store.Read("slot-1");Assert.That(recovered.State,Is.EqualTo(SaveSlotState.Recovered));Assert.That(recovered.Data.Coins,Is.EqualTo(10));
            Save(30);File.WriteAllText(_store.FilePath("slot-1"),"bad");
            Assert.That(_store.Read("slot-1").Data.Coins,Is.EqualTo(10),"corrupt primary must not replace good backup");
        }
        [Test] public void MissingPrimaryCanRecoverBackup()
        { Save();Save(20);File.Delete(_store.FilePath("slot-1"));Assert.That(_store.Read("slot-1").State,Is.EqualTo(SaveSlotState.Recovered)); }
        [Test] public void CorruptPrimaryAndBackupCannotLoad()
        { Save();Save();File.WriteAllText(_store.FilePath("slot-1"),"bad");File.WriteAllText(_store.FilePath("slot-1")+".bak","bad");Assert.That(_store.Read("slot-1").State,Is.EqualTo(SaveSlotState.Corrupt)); }
        [Test] public void FutureSchemaDoesNotSilentlyLoadOlderBackup()
        {
            Save();Save(20);var path=_store.FilePath("slot-1");var envelope=JsonUtility.FromJson<SaveEnvelope>(File.ReadAllText(path));
            envelope.SchemaVersion=999;File.WriteAllText(path,JsonUtility.ToJson(envelope));
            Assert.That(_store.Read("slot-1").State,Is.EqualTo(SaveSlotState.Unsupported));
        }
        [Test] public void PayloadTamperingIsDetected()
        {
            Save();var path=_store.FilePath("slot-1");var envelope=JsonUtility.FromJson<SaveEnvelope>(File.ReadAllText(path));
            envelope.PayloadJson=JsonUtility.ToJson(new Payload{Coins=999,Name="Герой"});File.WriteAllText(path,JsonUtility.ToJson(envelope));
            Assert.That(_store.Read("slot-1").State,Is.EqualTo(SaveSlotState.Corrupt));
        }
        [Test] public void AutosaveCannotOverwriteFutureSchemaWithoutExplicitReplacement()
        {
            Save();var path=_store.FilePath("slot-1");var envelope=JsonUtility.FromJson<SaveEnvelope>(File.ReadAllText(path));
            envelope.SchemaVersion=999;var future=JsonUtility.ToJson(envelope);File.WriteAllText(path,future);
            Assert.Throws<SaveStorageException>(()=>Save(20));Assert.That(File.ReadAllText(path),Is.EqualTo(future));
            _store.Write("slot-1",_profile,new Payload{Coins=20,Name="Герой"},true);
            Assert.That(_store.Read("slot-1").Data.Coins,Is.EqualTo(20));
        }
        [Test] public void FailedTemporaryWriteLeavesCommittedFileUntouched()
        {
            Save();var path=_store.FilePath("slot-1");var before=File.ReadAllText(path);Directory.CreateDirectory(path+".tmp");
            Assert.Throws<SaveStorageException>(()=>Save(200));Assert.That(File.ReadAllText(path),Is.EqualTo(before));
        }
        [Test] public void SlotIdCannotEscapeRoot()=>Assert.Throws<ArgumentException>(()=>_store.Read("../outside"));
        [Test] public void InvalidDataCannotReplaceGoodSave()
        { Save();Assert.Throws<SaveStorageException>(()=>Save(-1));Assert.That(_store.Read("slot-1").Data.Coins,Is.EqualTo(10)); }
        [Test] public void ArchiveRemovesSlotAndKeepsRecoverableFilesAndLegacyCopy()
        {
            Save();Save(20);_store.PreserveLegacy("original");_store.Archive("slot-1");
            Assert.That(_store.Read("slot-1").State,Is.EqualTo(SaveSlotState.Empty));
            Assert.That(Directory.GetFiles(Path.Combine(_root,"Deleted"),"*.json*",SearchOption.AllDirectories).Length,Is.EqualTo(2));
            Assert.That(File.ReadAllText(Path.Combine(_root,"legacy-playerprefs.json")),Is.EqualTo("original"));
        }
        [Test] public void LegacySnapshotIsImmutable()
        { _store.PreserveLegacy("original");_store.PreserveLegacy("changed");Assert.That(File.ReadAllText(Path.Combine(_root,"legacy-playerprefs.json")),Is.EqualTo("original")); }
        [Test] public void OversizedWriteCannotReplaceReadableSave()
        {
            Save();Assert.Throws<SaveStorageException>(()=>_store.Write("slot-1",_profile,new Payload{Coins=10,Name=new string('x',8*1024*1024)}));
            Assert.That(_store.Read("slot-1").Data.Name,Is.EqualTo("Герой"));
        }
    }
}
