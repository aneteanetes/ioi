using System.Collections.Generic;
using System.Linq;
using Geranium.Reflection;

namespace ioi.Game
{
    public class GameEntitySquad
    {
        private List<GameEntity> _members = new List<GameEntity>();
        
        public IEnumerable<GameEntity> Members => _members;
        
        public GameEntity Current { get; private set; }
        
        public GameEntity Leader => FirstAlive();
        
        public GameEntitySquad(GameEntity leader)
        {
            _members.Add(leader);
            Current = leader;
        }

        public void Add(GameEntity entity)
        {
            entity.Squad = this;
            _members.Add(entity);
        }

        public void Remove(GameEntity entity)
        {
            _members.Remove(entity);
        }

        public void CombatTurn(GameEntity target)
        {
            var i = 0;
            while (i < _members.Count)
            {
                /// если враги убили всех персонажей, то в дальнейшем избиении смысла нет.
                if (Global.IsGameOver)
                    break;
                
                /// если бой закончился пока мобы ходят
                if (Global.CombatSystem.IsEndOfBattle)
                    break;
                
                _members[i].Func("combatturn", target);
                i++;
            }
        }
        
        public bool IsEmpty() => _members.IsEmpty();

        public bool IsAnybodyAlive() => _members.Count(m => !m.IsUnconscious) != 0;
        
        public void MoveNext()
        {
            var currentIdx = Index(Current);

            currentIdx++;
            if (currentIdx == _members.Count) //end
            {
                currentIdx = 0;
            }

            Current = _members[currentIdx];
        }

        public void MoveNextAlive(int? index=null)
        {
            var currentIdx = index ?? Index(Current);

            currentIdx++;
            if (currentIdx == _members.Count) //end
            {
                currentIdx = 0;
            }

            var next = _members[currentIdx];
            if (!next.IsUnconscious)
            {
                Current = next;
            }
            else
            {
                MoveNextAlive(currentIdx);
            }
        }

        public bool TrySetMember(int idx, out GameEntity entity)
        {
            entity = _members.ElementAtOrDefault(idx);
            if (entity==default)
                return false;

            if(entity.IsUnconscious)
            {
                return false;
            }

            Current = entity;
            return true;
        }

        public GameEntity FirstAlive()
        {
            return _members.FirstOrDefault(x => !x.IsUnconscious);
        }

        private int Index(GameEntity entity)
        {
            return _members.IndexOf(entity);
        }

        //internal GameEntity NextAlive(GameEntity entity)
        //{
        //    var currentIdx = _members.IndexOf(entity);
        //    var next = GetNext(currentIdx);

        //    if (next == null && entity.IsUnconscious)
        //        throw new Exception("All party members is dead, but game requested next party member!");

        //    return next;
        //}

        //public GameEntity GetNext(int currentIdx, int? originalIdx = null)
        //{
        //    if (originalIdx.HasValue && currentIdx >= originalIdx)
        //    {
        //        // we did full loop
        //        var first = _members.ElementAtOrDefault(currentIdx);
        //        if (first.IsUnconscious)
        //            return null;

        //        return first;
        //    }

        //    if (originalIdx == null)
        //        originalIdx = currentIdx;

        //    currentIdx++;

        //    if (currentIdx == _members.Count)
        //        currentIdx = 0;

        //    var next = _members.ElementAtOrDefault(currentIdx);
        //    if (next == null || next.IsUnconscious)
        //        return GetNext(currentIdx, originalIdx);

        //    return next;
        //}

        internal void Destroy()
        {
            foreach (var member in _members)
            {
                member.Squad = null;
            }
            _members.Clear();
        }
    }
}