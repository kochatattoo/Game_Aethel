using System.Collections;
using UnityEngine;

namespace CodeBase.Enemies
{
    public class Aggro : MonoBehaviour
    {
        public TriggerObserver TriggerObserver;
        public Follow Follow;

        public float Cooldown;
        private Coroutine _aggroCoroutine;
        private bool _hasAggroTarget;

        private void Start()
        {
            TriggerObserver.TriggerEnter += TriggerEnter;
            TriggerObserver.TriggerExit += TriggerExit;

            SwitchFollowOff();
        }
        private void OnDisable()
        {
            TriggerObserver.TriggerExit -= TriggerExit;
            TriggerObserver.TriggerEnter -= TriggerEnter;
        }

        private void TriggerEnter(Collider obj)
        {
            if (_hasAggroTarget == false)
            {
                _hasAggroTarget = true;
                StopAgroCoroutine();

                SwitchFollowOn();
            }
        }

        private void TriggerExit(Collider obj)
        {
            if (_hasAggroTarget == true)
            {
                _hasAggroTarget = false;
                _aggroCoroutine = StartCoroutine(SwitchFollowOffAfterCooldown());
            }
        }

        private void StopAgroCoroutine()
        {
            if (_aggroCoroutine != null)
            {
                StopCoroutine(_aggroCoroutine);
                _aggroCoroutine = null;
            }
        }

        private void SwitchFollowOff() =>
            Follow.enabled = false;

        private void SwitchFollowOn() =>
            Follow.enabled = true;

        private IEnumerator SwitchFollowOffAfterCooldown()
        {
            yield return new WaitForSeconds(Cooldown);

            SwitchFollowOff();
        }
    }
}
