using Core.Contracts.AI;
using Gameplay.AI.Enemies;
using Gameplay.AI.Factories;
using UnityEngine;

namespace Gameplay.AI.Bosses.Ogre
{
    public class OgrePhase1 : IBossPhase
    {
        public float HPThreshold => 0.66f;

        public void EnterPhase(OgreBossEnemy boss)
        {
        }

        public void ExecutePhase(OgreBossEnemy boss)
        {
        }

        public void ExitPhase(OgreBossEnemy boss)
        {
        }
    }

    public class OgrePhase2 : IBossPhase
    {
        public float HPThreshold => 0.33f;
        private float _lastSpawnTime;
        private readonly float _spawnInterval = 15f;

        public void EnterPhase(OgreBossEnemy boss)
        {
            SpawnMinions(boss);
            _lastSpawnTime = Time.time;
        }

        public void ExecutePhase(OgreBossEnemy boss)
        {
            if (Time.time - _lastSpawnTime >= _spawnInterval)
            {
                SpawnMinions(boss);
                _lastSpawnTime = Time.time;
            }
        }

        public void ExitPhase(OgreBossEnemy boss)
        {
        }

        private void SpawnMinions(OgreBossEnemy boss)
        {
            boss.SpawnMinions(2);
        }
    }

    public class OgrePhase3 : IBossPhase
    {
        public float HPThreshold => 0f;
        private float _lastSpawnTime;
        private readonly float _spawnInterval = 10f;

        public void EnterPhase(OgreBossEnemy boss)
        {
            boss.SpawnMinions(3);
            _lastSpawnTime = Time.time;
        }

        public void ExecutePhase(OgreBossEnemy boss)
        {
            if (Time.time - _lastSpawnTime >= _spawnInterval)
            {
                boss.SpawnMinions(2);
                _lastSpawnTime = Time.time;
            }
        }

        public void ExitPhase(OgreBossEnemy boss)
        {
        }
    }
}
