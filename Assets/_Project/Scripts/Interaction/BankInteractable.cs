using UnityEngine;
using Simcity.Economy;
using Simcity.Economy.RealMoney;
using Simcity.UI;

namespace Simcity.Interaction
{
    /// <summary>
    /// The Bank — your link to the (web) money portal. Press E to open the buy-Coins /
    /// cash-out screen. Per GAME_DESIGN §6.5 the real buy/cash-out flows live on the web;
    /// in-app this stands in for that portal so the loop is testable end to end.
    /// </summary>
    public class BankInteractable : Interactable
    {
        public override string Prompt => "Open Bank / Web Portal";

        public override void Interact(GameObject interactor)
        {
            var svc = interactor.GetComponent<RealMoneyService>();
            var wallet = interactor.GetComponent<Wallet>();
            if (svc == null || wallet == null)
            {
                Debug.Log("[Bank] This character has no money service.");
                return;
            }
            RealMoneyPortalScreen.Open(svc, wallet);
        }
    }
}
