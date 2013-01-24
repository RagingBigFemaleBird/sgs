using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.Triggers;
using Sanguosha.Core.Cards;
using Sanguosha.Core.UI;
using Sanguosha.Core.Games;

namespace Sanguosha.Core.Skills
{
    public abstract class SaveLifeSkill : AutoVerifiedActiveSkill
    {
        public bool OwnerOnly { get; set; }
        public SaveLifeSkill()
        {
            OwnerOnly = true;
        }

        protected override bool VerifyCard(Players.Player source, Card card)
        {
            return true;
        }

        protected override bool VerifyPlayer(Players.Player source, Players.Player player)
        {
            return true;
        }

        protected abstract bool? SaveLifeVerify(Players.Player source, List<Card> cards, List<Players.Player> players);

        protected override bool? AdditionalVerify(Players.Player source, List<Card> cards, List<Players.Player> players)
        {
            if (Game.CurrentGame.IsDying.Count == 0) return false;
            if (OwnerOnly && Game.CurrentGame.IsDying.Last() != Owner) return false;
            return SaveLifeVerify(source, cards, players);
        }

        public override bool Commit(GameEventArgs arg)
        {
            throw new NotImplementedException();
        }
    }
}
