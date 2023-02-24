using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TenCrowns.AppCore;
using TenCrowns.GameCore;
using TenCrowns.GameCore.Text;
using static TenCrowns.GameCore.Text.TextExtensions;
using Constants = TenCrowns.GameCore.Constants;
using Enum = System.Enum;
using TenCrowns.ClientCore;
using Mohawk.SystemCore;
using Mohawk.UIInterfaces;
using UnityEngine;
using UnityEngine.UI;
using static TenCrowns.ClientCore.ClientUI;


namespace BetterAI
{
    public class BetterAIGameFactory : GameFactory
    {
        public BetterAIGameFactory() : base()
        {
            return;
        }
        public override Infos CreateInfos(ModSettings pModSettings)
        {
            return new BetterAIInfos(pModSettings);
        }
        public override InfoGlobals CreateInfoGlobals()
        {
            return new BetterAIInfoGlobals();
        }
        public override HelpText CreateHelpText(Infos pInfos, TextManager textManager)
        {
            return new BetterAIHelpText(pInfos, textManager);
        }
        public override City CreateCity()
        {
            return new BetterAICity();
        }
        public override Player CreatePlayer()
        {
            return new BetterAIPlayer();
        }
        public override Player.PlayerAI CreatePlayerAI()
        {
            return new BetterAIPlayer.BetterAIPlayerAI();
        }
        public override Player.PlayerAI.UnitRoleManager CreateUnitRoleManager(Game pGame)
        {
            return new BetterAIPlayer.BetterAIPlayerAI.BetterAIUnitRoleManager(pGame);
        }
        public override Unit CreateUnit()
        {
            return new BetterAIUnit();
        }
        public override Tile CreateTile()
        {
            return new BetterAITile();
        }
        public override Character CreateCharacter()
        {
            return new BetterAICharacter();
        }
        public override ClientUI CreateClientUI(ClientManager pClientManager)
        {
            return new BetterAIClientUI(pClientManager);
        }
        public override ClientRenderer CreateClientRenderer(ClientManager pClientManager)
        {
            return new BetterAIClientRenderer(pClientManager);
        }
        public override ClientManager CreateClientManager(ModSettings modSettings, Game gameClient, GameInterfaces gameInterfaces, IClientNetwork network)
        {
            return new BetterAIClientManager(modSettings, gameClient, gameInterfaces, network);
        }
    }
}