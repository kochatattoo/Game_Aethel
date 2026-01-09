using CodeBase.Infrastructure.Services.SaveLoad;
using System.Collections;
using UnityEngine;

namespace CodeBase.Logic
{
    public class SaveTrigger: MonoBehaviour
    {
        private const string PlayerTag = "Player";

        private ISaveLoadService _saveLoadService;
        private bool _triggered = false;
        private string _id;

        public string Id { get => _id; set => _id=value; }

        public void Construct(ISaveLoadService saveLoadService)
        {
            _saveLoadService = saveLoadService;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_triggered)
                return;

            if (other.CompareTag(PlayerTag))
            {
                _triggered = true;
                _saveLoadService.SaveProgress();
                //StartCoroutine(ResetTimer());
            }
        }

        private IEnumerator ResetTimer()
        {
            yield return new WaitForSeconds(3);
            _triggered = false;
        }
    }
}
