using Com.LuisPedroFonseca.ProCamera2D;
using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class MobSpawner : BaseObject
{
    [SerializeField]
    Transform m_spawnPoint = null;

    [SerializeField]
    TeamType m_teamType = TeamType.ENEMY_1;

    [SerializeField]
    string m_characterKey = null;

    GameCharacterPresenter m_targetCharacter = null;

    readonly float m_spawnCheckTime = 10.0f;

    bool m_bSpawning = false;

    private void OnEnable()
    {
        m_bSpawning = false;
        _SpawnChecker().Forget();
    }

    async UniTaskVoid _SpawnChecker()
    {
        while(true)
        {
            if (!_AvailableMakeMob())
            {
                await UniTask.WaitForSeconds(m_spawnCheckTime);
                continue;
            }

            m_targetCharacter = null;

            _MakeMob();

            await UniTask.WaitForSeconds(m_spawnCheckTime);
        }
    }

    bool _AvailableMakeMob() =>
        m_bSpawning == false &&
        (m_targetCharacter == null || m_targetCharacter.GetState() == CharacterState.DIE);

    void _MakeMob()
    {
        if (!TableManager.Instance.CharacterTable.TryGetValue(m_characterKey, out var characterData))
        {
            Universe.LogError(m_characterKey + " : Character template not found!");
            return;
        }

        m_bSpawning = true;
        CharacterManager.Instance.MakeCharacter(characterData.KEY, m_teamType, _CallBackSpawnMob);
    }

    void _CallBackSpawnMob(GameObject obj)
    {
        m_bSpawning = false;
        if (!obj)
            return;

        m_targetCharacter = obj.GetComponent<GameCharacterView>()?.Presenter;
        if (m_targetCharacter == null)
            return;

        m_targetCharacter.SetPosition(Vector2.zero);
        m_targetCharacter.SetParent(m_spawnPoint);
    }
}
