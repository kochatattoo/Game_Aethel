using CodeBase.Data;
using CodeBase.Infrastructure.Services;
using CodeBase.Infrastructure.Services.PersistentProgress;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

namespace CodeBase.Hero
{
    public class HeroPathFollower : Move, ISavedProgress, IMoveable
    {
        public float MoveSpeed = 5f;
        public HeroAnimator heroAnimator;

        [SerializeField] private NavMeshAgent _agent;
        private PathFollower _pathFollower;

        public bool IsMove { get; private set; }

        public void Construct(IInputService inputService)
        {
            _pathFollower = new PathFollower(inputService);
            _pathFollower.OnNewDestination += SetDestination;

            if(_agent == null) _agent = GetComponent<NavMeshAgent>();
            _agent.speed = MoveSpeed;
        }

        void Update()
        {
            if (_pathFollower != null)
            {
                _pathFollower.Update();          // Обрабатывает клики мышки

                // NavMeshAgent уже управляет движением, но нам нужно знать, когда он остановился
                if (!_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance)
                    IsMove = false;
                else
                    IsMove = true;

                PlayMove();
            }
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

        private void SetDestination(Vector3 dest)
        {
            Debug.Log("Установка дистанции для агента" + dest.x + " " + dest.y + " " + dest.z);
            _agent.SetDestination(dest);
            IsMove = true;
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