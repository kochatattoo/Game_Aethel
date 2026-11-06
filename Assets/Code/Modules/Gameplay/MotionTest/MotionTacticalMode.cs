using System;
using System.Collections.Generic;
using PetProject.Tactics;
using UnityEngine;

namespace CodeBase.MotionTest
{
    internal sealed class MotionTacticalMode
    {
        private readonly MotionHero _hero;
        private readonly SurfaceGridMap _map;
        private readonly MotionTestConfig _config;
        private readonly WorldActionBudgets _worldActions;
        private readonly Func<int> _budget;
        private readonly Action<string> _setNotice;
        private readonly Action<string> _fail;
        private readonly Action _invalidatePreview;
        private readonly Action _refreshView;
        private IReadOnlyList<GridPosition> _route;
        private int _routeIndex;
        private bool _alignment;
        private bool _exitAfterMove;
        private GridPosition _alignedCell;
        private int _command;
        private float _alignmentElapsed;

        public MotionTestSession Session { get; private set; }
        public bool Active { get; private set; }
        public bool Alignment => _alignment;
        public bool Busy => _alignment || _route != null;
        public bool CanSwitch => !_alignment && !_exitAfterMove;
        public int AcceptedMoves { get; private set; }

        public MotionTacticalMode(MotionHero hero, SurfaceGridMap map, MotionTestConfig config,
            WorldActionBudgets worldActions, Func<int> budget, Action<string> setNotice, Action<string> fail,
            Action invalidatePreview, Action refreshView)
        {
            _hero = hero;
            _map = map;
            _config = config;
            _worldActions = worldActions;
            _budget = budget;
            _setNotice = setNotice;
            _fail = fail;
            _invalidatePreview = invalidatePreview;
            _refreshView = refreshView;
        }

        public void Tick()
        {
            UpdateAlignment();
            if (_route != null) AdvanceRoute();
            if (!Busy && _exitAfterMove)
            {
                _exitAfterMove = false;
                SetActive(false);
            }
            if (Active && !Busy && Session != null && Session.Actions == 0)
                NewTurn("ОД потрачены — новый ход начат автоматически");
        }

        public bool SetActive(bool requested)
        {
            if (requested == Active && !_alignment) return true;
            if (_route != null)
            {
                if (!requested)
                {
                    _exitAfterMove = true;
                    _setNotice("Выход после завершения пути");
                }
                return false;
            }
            if (_alignment)
            {
                _setNotice("Подождите выравнивание по клетке");
                return false;
            }
            if (!requested) return Exit();

            _hero.Stop();
            if (!_map.NearestReachable(_hero.Instance.transform.position, out _alignedCell))
            {
                _setNotice("Рядом нет безопасного центра клетки");
                return false;
            }
            if (!_hero.BeginAlignment(_map.World(_alignedCell)))
            {
                _setNotice("Не удалось задать путь к центру клетки");
                return false;
            }
            _alignmentElapsed = 0;
            _alignment = true;
            _setNotice("Выравнивание по клетке");
            return true;
        }

        public MotionMoveResult Move(GridPosition destination)
        {
            var result = Session.Move("motion-" + ++_command, destination, Session.Revision);
            if (!result.Accepted) return result;

            _route = result.Path.Cells;
            _routeIndex = 1;
            ++AcceptedMoves;
            _hero.Animator.Move();
            return result;
        }

        public bool NewTurn()
        {
            if (!Active || Busy || Session.Actions == 0) return false;
            NewTurn("Новый тестовый ход");
            return true;
        }

        public void PrepareForEncounter()
        {
            SaveActions();
            ClearState();
            _hero.PrepareForEncounter();
        }

        public void FinishEncounterAlignment() => _hero.EnterTactical();

        public bool RestoreExploration()
        {
            ClearState();
            return _hero.RestoreExploration(true);
        }

        public void Reset()
        {
            ClearState();
            AcceptedMoves = 0;
            _hero.Reset();
        }

        private void UpdateAlignment()
        {
            if (!_alignment) return;
            _alignmentElapsed += Time.deltaTime;
            if (_alignmentElapsed > 5 || _hero.AlignmentFailed)
            {
                _hero.Stop();
                _alignment = false;
                _setNotice("Не удалось безопасно выровнять героя; обычный режим сохранён");
            }
            if (!_alignment || !_hero.AlignmentCompleted) return;

            _hero.CompleteAlignment(_map.World(_alignedCell));
            var budget = Budget;
            Session = new MotionTestSession(_map.Board, _alignedCell, budget, _config.MaxHeightStep,
                _worldActions.Get(MotionTestController.HeroActorId, budget));
            Active = true;
            _alignment = false;
            _setNotice("Тактическое перемещение");
            _invalidatePreview();
        }

        private void AdvanceRoute()
        {
            var destination = _map.World(_route[_routeIndex]);
            if (!GridPathMovement.MoveStep(_hero.Instance.transform, _map, destination, _config.Speed))
            {
                _route = null;
                _fail("Исполняемый путь потерял опору; выполните Reset");
                return;
            }
            if (!GridPathMovement.HasArrived(_hero.Instance.transform, destination)) return;
            if (++_routeIndex < _route.Count) return;

            _route = null;
            _hero.Animator.StopMoving();
            _setNotice("Путь завершён");
            if (Session.Actions == 0) NewTurn("ОД потрачены — новый ход начат автоматически");
            _invalidatePreview();
            _refreshView();
        }

        private bool Exit()
        {
            SaveActions();
            ClearState();
            if (!_hero.RestoreExploration(false))
            {
                _fail("Не удалось вернуть героя на NavMesh");
                return false;
            }
            _invalidatePreview();
            _setNotice("Обычное движение");
            return true;
        }

        private void NewTurn(string message)
        {
            Session.NewTurn();
            _setNotice(message);
            _invalidatePreview();
            _refreshView();
        }

        private void SaveActions()
        {
            if (Session != null) _worldActions.Set(MotionTestController.HeroActorId, Session.Actions, Budget);
        }

        private void ClearState()
        {
            _route = null;
            _alignment = false;
            _exitAfterMove = false;
            Active = false;
            Session = null;
        }

        private int Budget => Math.Max(1, _budget());
    }
}
