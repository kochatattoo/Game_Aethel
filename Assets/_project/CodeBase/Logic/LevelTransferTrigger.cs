using CodeBase.Infrastructure.Services.Levels;
using UnityEngine;

namespace CodeBase.Logic
{
    public class LevelTransferTrigger : MonoBehaviour
    {
        private const string PlayerTag = "Player";

        public string TransferTo;

        private bool _triggered = false;
        private ILevelTransferService _levelTransfer;

        public void Construct(ILevelTransferService levelTransferService, string levelTo)
        {
            _levelTransfer = levelTransferService;
            TransferTo = levelTo;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_triggered)
                return;

            if (other.CompareTag(PlayerTag))
            {
                _triggered = true;
                _levelTransfer.GoTo(TransferTo);
            }
        }
    }
}
