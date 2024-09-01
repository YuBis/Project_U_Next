using Com.LuisPedroFonseca.ProCamera2D;
using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Tilemaps;

public class MapObject : BaseStaticGameObject
{
    [SerializeField]
    Transform m_playerSpawnPoint = null;

    Player m_player = null;

    public void Initialize()
    {
        SpawnPlayer();
    }

    public void SpawnPlayer()
    {
        if (!TableManager.Instance.CharacterTable.TryGetValue(StaticString.PLAYER, out var playerCharacter))
        {
            Universe.LogError("Player character template not found!");
            return;
        }

        var player = CharacterManager.Instance.GetCharacter(TeamType.ALLY, playerCharacter.KEY);
        if(player != null)
        {
            player.OnDeath();
            CharacterManager.Instance.RemoveCharacter(player);
        }

        CharacterManager.Instance.MakeCharacter(playerCharacter.KEY, TeamType.ALLY, _CallBackAddPlayer);
    }

    void _CallBackAddPlayer(GameObject obj)
    {
        if (!obj)
            return;

        m_player = obj.GetComponent<Player>();

        obj.transform.position = m_playerSpawnPoint.position;
        Camera cameraObject = GameMaster.Instance.MAIN_CAMERA;//GameObject.FindGameObjectWithTag(StaticString.MAIN_CAMERA);
        if (cameraObject != null)
        {
            ProCamera2D proCamera2D = cameraObject.GetComponent<ProCamera2D>();
            proCamera2D.AddCameraTarget(obj.transform);
        }
    }
}
