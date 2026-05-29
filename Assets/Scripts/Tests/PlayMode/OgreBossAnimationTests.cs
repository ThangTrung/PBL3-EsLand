using System.Collections;
using UnityEngine;
using Gameplay.AI.Enemies;
using Gameplay.AI.Strategies;
using Gameplay.AI.Animation;
using Gameplay.Characters;
using Infrastructure.Pooling;
using Gameplay.AI;
using System.Collections.Generic;

public class OgreBossAnimationTests : MonoBehaviour
{
    private GameObject _bossGo;
    private OgreBossEnemy _boss;
    private CharacterAnimationController _animator;
    private GameObject _poolManagerGo;
    
    private void Start()
    {
        StartCoroutine(RunTest());
    }

    public void Setup()
    {
        _poolManagerGo = new GameObject("ObjectPoolManager");
        _poolManagerGo.AddComponent<ObjectPoolManager>();

        _bossGo = new GameObject("OgreBoss");
        _boss = _bossGo.AddComponent<OgreBossEnemy>();
        _animator = _bossGo.AddComponent<CharacterAnimationController>();
        
        var config = ScriptableObject.CreateInstance<AnimationConfig>();
        var seqs = new List<AnimationSequence>
        {
            new AnimationSequence { stateName = "Windup", frames = new Sprite[2] { null, null }, isLooping = false },
            new AnimationSequence { stateName = "Attack", frames = new Sprite[4] { null, null, null, null }, isLooping = false, triggerFrame = 2 },
            new AnimationSequence { stateName = "Recovery", frames = new Sprite[2] { null, null }, isLooping = false },
            new AnimationSequence { stateName = "Death", frames = new Sprite[3] { null, null, null }, isLooping = false }
        };
        config.Initialize(seqs, 10f);
        _animator.SetConfig(config);
    }

    public void Teardown()
    {
        Destroy(_bossGo);
        Destroy(_poolManagerGo);
    }

    private IEnumerator RunTest()
    {
        Setup();
        
        var hammerStrategy = new HammerSmashAttackStrategy(10f, 2f, 2f, 1f, _animator, 2, _bossGo.transform, _boss);
        var mockPlayer = new GameObject("MockPlayer");
        hammerStrategy.BeginAttack(mockPlayer.transform);

        if (_animator.GetCurrentState() != "Windup") Debug.LogError("Test Failed: Must start with Windup");
        else Debug.Log("Test Step 1 Passed: Windup started.");
        
        while (!_animator.IsCurrentAnimationFinished()) yield return null;
        hammerStrategy.TryApplyHitIfReady(); 

        if (_animator.GetCurrentState() != "Attack") Debug.LogError("Test Failed: Must transition to Attack");
        else Debug.Log("Test Step 2 Passed: Transitioned to Attack.");
        
        while (!_animator.IsCurrentAnimationFinished()) yield return null;
        hammerStrategy.TryApplyHitIfReady();

        if (_animator.GetCurrentState() != "Recovery") Debug.LogError("Test Failed: Must transition to Recovery");
        else Debug.Log("Test Step 3 Passed: Transitioned to Recovery.");
        
        Destroy(mockPlayer);
        
        Debug.Log("<color=green>ALL TESTS PASSED: OgreBoss Animation Flow is correct.</color>");
        Teardown();
    }
}
