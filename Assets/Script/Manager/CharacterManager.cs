using UnityEngine;
using System.Collections.Generic;
using SimpleJSON;
using System;
using static Spine.Unity.Examples.MixAndMatchSkinsExample;
using System.Linq;

public enum TeamType
{ 
    ALLY,
    ENEMY_1,
}

public enum CharacterState
{
    NORMAL,
    SPAWNING,
    DIE
}

public enum SearchType
{
    SIGHT,
    AREA,
}


public class CharacterManager : BaseManager<CharacterManager>
{
    public Dictionary<TeamType, List<GameCharacterPresenter>> m_dicGameCharacterList = new();

    protected override void _InitManager()
    {
        ClearAllCharacter();
    }

    public void ClearAllCharacter()
    {
        m_dicGameCharacterList.Clear();
    }

    public void MakeCharacter(string characterKey, TeamType teamType, Action<GameObject> callBack = null)
    {
        if (!TableManager.Instance.CharacterTable.TryGetValue(characterKey, out var characterTemplate))
            return;

        ObjectPoolManager.Instance.GetObject(characterTemplate.PREFAB, (go) =>
        {
            _CallbackMakeCharacter(characterKey, go, teamType, callBack);
        });
    }

    void _CallbackMakeCharacter(string characterKey, GameObject character, TeamType teamType, Action<GameObject> finalCallback)
    {
        List<GameCharacterPresenter> list = null;
        if (!m_dicGameCharacterList.TryGetValue(teamType, out list))
            list = new();

        var gameCharacterView = character.GetComponent<GameCharacterView>();
        GameCharacterModel model = new GameCharacterModel(characterKey, teamType);
        GameCharacterPresenter presenter = new GameCharacterPresenter(model, gameCharacterView);

        gameCharacterView.Initialize(presenter);
        list.Add(presenter);

        m_dicGameCharacterList[teamType] = list;

        finalCallback?.Invoke(character);
    }

    public GameCharacterPresenter GetCharacter(TeamType teamType, string characterKey)
    {
        if (!m_dicGameCharacterList.TryGetValue(teamType, out var list))
            return null;

        return list.FirstOrDefault(c => c.GetTemplateKey() == characterKey);
    }

    public void RemoveCharacter(GameCharacterPresenter gameCharacter)
    {
        if (gameCharacter.GetState() != CharacterState.DIE)
            gameCharacter.OnDeath();

        if (m_dicGameCharacterList.TryGetValue(gameCharacter.GetTeamType(), out var list))
        {
            list.Remove(gameCharacter);
        }

        ObjectPoolManager.Instance.ReleaseObject(gameCharacter.GetGameObject());
    }

    public GameCharacterPresenter GetEnemyInSight(GameCharacterPresenter myCharacter)
    {
        if (myCharacter == null || myCharacter.GetState() == CharacterState.DIE)
            return null;

        var teamType = myCharacter.GetTeamType();
        var myPos = myCharacter.GetPosition();
        var sight = myCharacter.GetStat(StatType.SIGHT);

        GameCharacterPresenter nearCharacter = null;
        double lastDist = double.MaxValue;

        var enumerator = m_dicGameCharacterList.GetEnumerator();
        while (enumerator.MoveNext())
        {
            if (enumerator.Current.Key == teamType)
                continue;

            var characterList = enumerator.Current.Value;
            foreach(var targetCharacter in characterList)
            {
                if (targetCharacter == null)
                    continue;

                if (targetCharacter.GetState() != CharacterState.NORMAL)
                    continue;

                var targetPos = targetCharacter.GetPosition();

                var r = sight * targetCharacter.GetScale().x / 2;
                var range = Vector3.Distance(myPos, targetPos);

                if (range > r)
                    continue;

                if (range < lastDist)
                {
                    lastDist = range;
                    nearCharacter = targetCharacter;
                }
            }
            
        }

        return nearCharacter;
    }

    public List<GameCharacterPresenter> GetEnemiesInRange(GameCharacterPresenter myCharacter, Rect rangeRect, int target = 1)
    {
        if (myCharacter == null || myCharacter.GetState() == CharacterState.DIE)
            return null;

        var teamType = myCharacter.GetTeamType();
        var myPos = myCharacter.GetPosition();

        Dictionary<float, GameCharacterPresenter> dicEnemies = new();

        rangeRect.position += new Vector2(myCharacter.GetPosition().x, myCharacter.GetPosition().y);

        bool flip = myCharacter.GetRotation().y != 0;
        if(flip)
        {
            rangeRect.x -= rangeRect.width;
        }

        var enumerator = m_dicGameCharacterList.GetEnumerator();
        while (enumerator.MoveNext())
        {
            if (enumerator.Current.Key == teamType)
                continue;

            var characterList = enumerator.Current.Value;
            foreach (var targetCharacter in characterList)
            {
                if (targetCharacter == null)
                    continue;

                if (targetCharacter.GetState() != CharacterState.NORMAL)
                    continue;

                var targetPos = targetCharacter.GetPosition();

                if (!rangeRect.Contains(targetPos))
                    continue;

                var range = Vector3.Distance(myPos, targetPos);

                dicEnemies.Add(range, targetCharacter);
            }

        }

        var targetEnemies = dicEnemies.OrderBy(t => t.Key).Select(t => t.Value).ToList();
        if (targetEnemies.Count <= target)
            return targetEnemies;

        return targetEnemies.GetRange(0, target);
    }
}