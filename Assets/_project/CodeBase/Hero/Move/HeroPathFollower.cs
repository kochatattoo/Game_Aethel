using CodeBase.Data;
using CodeBase.Infrastructure.Services.PersistentProgress;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

namespace CodeBase.Hero
{
    [RequireComponent(typeof(NavMeshAgent), typeof(HeroAnimator))]
    public class HeroPathFollower : Move, ISavedProgress
    {
        [SerializeField] private float _maxHeightDifference = 2f;
        [SerializeField] private float _stopingDistance = 0.1f;
        [SerializeField] private float _moveSpeed = 5f;
        [SerializeField] private HeroAnimator heroAnimator;
        [SerializeField] private NavMeshAgent _agent;

        public bool IsMove { get; private set; }
        public float MaxHeightDifference { get => _maxHeightDifference; private set => _maxHeightDifference = value; }

        public void Construct()
        {
            if(_agent == null) _agent = GetComponent<NavMeshAgent>();
            _agent.speed = _moveSpeed;
            _agent.stoppingDistance = _stopingDistance;
        }

        private void Update()
        {
            if (!_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance)
                IsMove = false;
            else
                IsMove = true;

            PlayMove();
        }

        public void MoveTo(Vector3 worldPoint)
        {
           // Debug.Log("Установка дистанции для агента" + worldPoint.x + " " + worldPoint.y + " " + worldPoint.z);
            _agent.isStopped = false;
            _agent.SetDestination(worldPoint);
            IsMove = true;
        }

        public void Stop()
        {
            _agent.ResetPath();
            _agent.isStopped = true;
            IsMove = false;
        }

        public void UpdateProgress(PlayerProgress progress)
        {
            progress.WorldData.PositionOnLevel = new PositionOnLevel(CurrentLevel(), transform.position.AsVectorData());
        }

        public void LoadProgress(PlayerProgress progress)
        {
            if (CurrentLevel() == progress.WorldData.PositionOnLevel.Level)
            {
                Vector3Data savedPosition = progress.WorldData.PositionOnLevel.Position;

                if (savedPosition != null)
                {
                    Warp(savedPosition);
                }
            }
        }

        private void PlayMove()
        {
            if (IsMove)
                heroAnimator.Move();
            else
                heroAnimator.StopMoving();
        }

        private void Warp(Vector3Data to)
        {
            _agent.enabled = false;
            transform.position = to.AsUnityVector().AddY(_agent.height / 2);
            _agent.enabled = true;
        }

        private static string CurrentLevel() => SceneManager.GetActiveScene().name;
    }
}