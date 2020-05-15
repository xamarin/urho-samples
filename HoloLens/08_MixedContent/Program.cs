using System;
using System.Collections.Generic;
using Windows.UI.ViewManagement;
using Urho;
using Urho.Actions;
using Urho.HoloLens;

namespace MixedContent
{
	internal class Program
	{
	    [MTAThread]
	    static void Main()
	    {
            // Start XAML app instead of urho app
            // Main is called because we defined DISABLE_XAML_GENERATED_MAIN in the DefineConstants node of the project file.   
            // See http://blog.infernored.com/mixing-hololens-2d-and-3d-xaml-views-in-holographicspace for details

            global::Windows.UI.Xaml.Application.Start((p) => new MixedContent.App());
	    }
	}


	public class MutantApp : HoloApplication
	{
		Node mutantNode;
		Vector3 monsterPositionBeforeManipulations;
		AnimationController animation;

		const string IdleAni = "Mutant_Idle1.ani";
		const string KillAni = "Mutant_Death.ani";
		const string HipHopAni = "Mutant_HipHop1.ani";
		const string JumpAni = "Mutant_Jump.ani";
		const string JumpAttack = "Mutant_JumpAttack.ani";
		const string KickAni = "Mutant_Kick.ani";
		const string PunchAni = "Mutant_Punch.ani";
		const string RunAni = "Mutant_Run.ani";
		const string SwipeAni = "Mutant_Swipe.ani";
		const string WalkAni = "Mutant_Walk.ani";

		public MutantApp(ApplicationOptions opts) : base(opts) { }

		protected override void Start()
		{
			base.Start();

			EnableGestureManipulation = true;

			Zone.AmbientColor = new Color(0.8f, 0.8f, 0.8f);
			DirectionalLight.Brightness = 1;

			mutantNode = Scene.CreateChild();
			mutantNode.Position = new Vector3(0, 0, 2f);
			mutantNode.SetScale(0.2f);
			var mutant = mutantNode.CreateComponent<AnimatedModel>();

			mutant.Model = ResourceCache.GetModel("Models/Mutant.mdl");
			mutant.SetMaterial(ResourceCache.GetMaterial("Materials/mutant_M.xml"));
			animation = mutantNode.CreateComponent<AnimationController>();
			PlayAnimation(IdleAni);

			RegisterCortanaCommands(new Dictionary<string, Action>
				{
					//play animations using Cortana
					{"idle", () => PlayAnimation(IdleAni)},
					{"die", () => PlayAnimation(KillAni)},
					{"dance", () => PlayAnimation(HipHopAni)},
					{"jump", () => PlayAnimation(JumpAni)},
					{"jump attack", () => PlayAnimation(JumpAttack)},
					{"kick", () => PlayAnimation(KickAni)},
					{"punch", () => PlayAnimation(PunchAni)},
					{"run", () => PlayAnimation(RunAni)},
					{"swipe", () => PlayAnimation(SwipeAni)},
					{"walk", () => PlayAnimation(WalkAni)},

					{"bigger", () => mutantNode.ScaleNode(1.2f)},
					{"smaller", () => mutantNode.ScaleNode(0.8f)},
					{"increase the brightness", () => IncreaseBrightness(1.2f)},
					{"decrease the brightness", () => IncreaseBrightness(0.8f)},
					{"look at me", LookAtMe },
					{"turn around", () => mutantNode.RunActions(new RotateBy(1f, 0, 180, 0))},
					{"help", Help },                    
					{"back", ShowXaml }                    
				});
		}

	    private async void ShowXaml()
	    {
            await ApplicationViewSwitcher.SwitchAsync(App.ViewXaml.Id);
        }

	    void LookAtMe()
		{
			mutantNode.Rotation = new Quaternion(0, -LeftCamera.Node.Rotation.ToEulerAngles().Y, 0);
			//or mutantNode.LookAt(...);
		}

		async void Help()
		{
			await TextToSpeech("Available commands are:");
			foreach (var cortanaCommand in CortanaCommands.Keys)
				await TextToSpeech(cortanaCommand);
		}

		void IncreaseBrightness(float byValue)
		{
			//by default, HoloScene has two kinds of lights:
			DirectionalLight.Brightness *= byValue;
		}

		void PlayAnimation(string file, bool looped = true)
		{
			mutantNode.RemoveAllActions();

			if (file == WalkAni)
				mutantNode.RunActions(new RepeatForever(new MoveBy(1f, mutantNode.Rotation * new Vector3(0, 0, -mutantNode.Scale.X))));
			else if (file == RunAni)
				mutantNode.RunActions(new RepeatForever(new MoveBy(1f, mutantNode.Rotation * new Vector3(0, 0, -mutantNode.Scale.X * 2))));

			animation.StopAll(0.2f);
			animation.Play("Animations/" + file, 0, looped, 0.2f);
		}


		protected override void OnUpdate(float timeStep)
		{
			base.OnUpdate(timeStep);

			//for optical stabilization:
			//TODO: PostUpdate?
			FocusWorldPoint = mutantNode.WorldPosition;
		}

		// Handle spatial input gestures:

		public override void OnGestureManipulationStarted()
		{
			monsterPositionBeforeManipulations = mutantNode.Position;
		}

		public override void OnGestureManipulationUpdated(Vector3 relativeHandPosition)
		{
			mutantNode.Position = relativeHandPosition * 2 + monsterPositionBeforeManipulations;
		}
	}
}