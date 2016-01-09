using System;
using System.Drawing;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;

namespace EloBuddy_Test
{
    public class Clone_Movement
    {
        private static readonly String[] Modes = {"Orbwalk to Enemy", "Orbwalk to Self", "Follow Enemy", "Follow Self"};
        private static float LastAttack;
        private static Menu Menu;
        private static CheckBox Enabled;
        private static Slider ModeSlider;
        private static Slider Range;
        private static GameObject Clone;
        private static String ChampName;

        public Clone_Movement()
        {
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
        }

        private static void Loading_OnLoadingComplete(EventArgs args)
        {
            if (Player.Instance.Hero != Champion.Shaco && Player.Instance.Hero != Champion.Leblanc && Player.Instance.Hero != Champion.Yorick && Player.Instance.Hero != Champion.Mordekaiser)
            {
                return;
            }

            ChampName = Player.Instance.ChampionName;
            Chat.Print("Clone Movement by SamuraiGilgamesh Loaded For " + ChampName, Color.Green);
            CreateMenu();

            Game.OnUpdate += Game_OnUpdate;
            GameObject.OnCreate += GameObject_OnCreate;
            GameObject.OnDelete += GameObject_OnDelete;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            
        }

        private static void CreateMenu()
        {
            Menu = MainMenu.AddMenu("Clone Movement", "CloneMov");

            Enabled = Menu.Add("Clone", new CheckBox("Clone Movement Enabled"));
            Menu.AddGroupLabel("You can select a target with left click");

            ModeSlider = Menu.Add("Mode", new Slider("Movement Mode", 0, 0, 3));
            ModeSlider.DisplayName = Modes[ModeSlider.CurrentValue];
            ModeSlider.OnValueChange += 
            delegate (ValueBase<int> sender, ValueBase<int>.ValueChangeArgs Args)
            {
                sender.DisplayName = Modes[Args.NewValue]; //Updates the name of the mode
            };
           
            Range = Menu.Add("Range", new Slider("Get targets within {0} range", 2000, 1, 10000));
        }

        private static void Obj_AI_Base_OnProcessSpellCast(GameObject sender, GameObjectProcessSpellCastEventArgs args)
        {
            if(sender == Clone && EloBuddy.SDK.Constants.AutoAttacks.IsAutoAttack(args))
            {
                LastAttack = Environment.TickCount - (Game.Ping / 2);
            }
        }

        private static void GameObject_OnCreate(GameObject sender, EventArgs args)
        {
            if (sender.Name == Player.Instance.Name)
            {
                Clone = sender;
            }
        }

        private static void GameObject_OnDelete(GameObject sender, EventArgs args)
        {
            if (sender.Name == Player.Instance.Name)
            {
                Clone = null;
            }
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            if (Clone == null || !Enabled.CurrentValue)
            {
                return;
            }

            switch (ModeSlider.CurrentValue)
            {
                case 0: OrbwalkEnemy();
                    break;
                case 1: OrbwalkSelf();
                    break;
                case 2: FollowEnemy();
                    break;
                case 3: FollowSelf();
                    break;
            }
        }

        private static void FollowEnemy()
        {
            var target = TargetSelector.SelectedTarget ?? TargetSelector.GetTarget(Range.CurrentValue, DamageType.Magical);

            if (target == null)
            {
                FollowSelf();
            }
            else
            {
                FollowEnemy();
            }
        }

        private static void FollowSelf()
        {
            Player.IssueOrder(GameObjectOrder.MovePet, ObjectManager.Player.Position);
        }

        private static void OrbwalkSelf()
        {
            var target = TargetSelector.SelectedTarget ?? TargetSelector.GetTarget(Range.CurrentValue, DamageType.Magical);

            if (target == null)
            {
                FollowSelf();
            }
            else
            {
                if(Clone.Distance(target) <= ObjectManager.Player.AttackRange && CanAttack())
                {
                    Player.IssueOrder(GameObjectOrder.AutoAttackPet, target);
                }
            }
        }

        private static void OrbwalkEnemy()
        {
            var target = TargetSelector.SelectedTarget ?? TargetSelector.GetTarget(Range.CurrentValue, DamageType.Magical);

            if (target == null)
            {
                FollowSelf();
            }
            else
            {
                if (Clone.Distance(target) <= ObjectManager.Player.AttackRange && CanAttack())
                {
                    Player.IssueOrder(GameObjectOrder.AutoAttackPet, target);
                }
                else
                {
                    FollowEnemy();
                }
            }
        }

        private static bool CanAttack()
        {
            return (Environment.TickCount + (Game.Ping / 2) > LastAttack);
        }
    }
}
