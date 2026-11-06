using System;
using System.Collections.Generic;
using PetProject.Tactics;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CodeBase.TacticsSandbox
{
    /// <summary>Owns prototype meshes/materials. Presentation never changes encounter state.</summary>
    internal sealed class TacticsSandboxView : IDisposable
    {
        private readonly Transform _root;
        private readonly GridBoard _board;
        private readonly Shader _shader;
        private readonly Dictionary<string, GameObject> _actors = new(StringComparer.Ordinal);
        private readonly List<Material> _materials = new();
        private readonly Material _ally, _enemy;
        private bool _disposed;
        public int ActorCount => _actors.Count;

        public TacticsSandboxView(Transform parent, GridBoard board)
        {
            _shader = Shader.Find("Universal Render Pipeline/Lit");
            if (_shader == null) throw new InvalidOperationException("URP Lit shader is missing");
            _board = board;
            _root = new GameObject("Tactical presentation").transform;
            _root.SetParent(parent, false);
            var floor = CreateMaterial(new Color(.22f, .28f, .34f));
            var wall = CreateMaterial(new Color(.08f, .11f, .15f));
            var difficult = CreateMaterial(new Color(.64f, .46f, .2f));
            _ally = CreateMaterial(new Color(.15f, .8f, .64f));
            _enemy = CreateMaterial(new Color(.92f, .25f, .3f));
            for (var y = 0; y < board.Height; ++y)
                for (var x = 0; x < board.Width; ++x)
                {
                    var tile = board.GetCell(new GridPosition(x, y));
                    var instance = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    instance.name = $"Cell {x},{y} cost={tile.MoveCost}";
                    instance.transform.SetParent(_root, false);
                    var height = tile.Walkable ? .12f : 1f;
                    instance.transform.localScale = new Vector3(.94f, height, .94f);
                    instance.transform.localPosition = new Vector3(x, tile.Height * .4f + height / 2, y);
                    instance.GetComponent<Renderer>().sharedMaterial = !tile.Walkable ? wall : tile.MoveCost > 1 ? difficult : floor;
                }
        }

        public void Sync(EncounterSnapshot snapshot)
        {
            foreach (var actor in snapshot.Actors)
            {
                if (!_actors.TryGetValue(actor.Id, out var instance))
                {
                    instance = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                    instance.transform.SetParent(_root, false);
                    instance.GetComponent<Renderer>().sharedMaterial = actor.FactionId == "allies" ? _ally : _enemy;
                    _actors.Add(actor.Id, instance);
                }
                instance.name = $"{actor.Id}: HP {actor.Health}/{actor.Definition.MaxHealth}, AP {actor.ActionPoints}";
                instance.SetActive(actor.IsAlive);
                instance.transform.localPosition = PositionFor(actor.Position);
                var radius = actor.Id == snapshot.CurrentActorId ? .65f : .55f;
                instance.transform.localScale = new Vector3(radius, .75f, radius);
            }
        }

        public Vector3 GetActorPosition(string id) => _actors[id].transform.position;
        public bool IsActorActive(string id) => _actors[id].activeSelf;
        private Vector3 PositionFor(GridPosition position) => new(position.X, _board.GetCell(position).Height * .4f + .87f, position.Y);

        private Material CreateMaterial(Color color)
        {
            var material = new Material(_shader) { color = color, name = "Tactics prototype" };
            _materials.Add(material);
            return material;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            foreach (var material in _materials) if (material != null) DestroyOwned(material);
            _materials.Clear();
            if (_root != null) DestroyOwned(_root.gameObject);
            _actors.Clear();
        }

        private static void DestroyOwned(Object instance)
        {
            if (Application.isPlaying) Object.Destroy(instance);
            else Object.DestroyImmediate(instance);
        }
    }
}
