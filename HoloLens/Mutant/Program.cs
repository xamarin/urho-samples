using System;
using System.Collections.Generic;
using Windows.ApplicationModel.Core;
using Urho;
using Urho.Actions;
using Urho.Holographics;
using Urho.HoloLens;

namespace Mutant
{
	/// <summary>
	/// Windows Holographic application using UrhoSharp.
	/// </summary>
	internal class Program
	{
		/// <summary>
		/// Defines the entry point of the application.
		/// </summary>
		[MTAThread]
		private static void Main()
		{
			var exclusiveViewApplicationSource = new AppViewSource();
			CoreApplication.Run(exclusiveViewApplicationSource);
		}

		class AppViewSource : IFrameworkViewSource
		{
			public IFrameworkView CreateView() => UrhoAppView.Create<MutantApp>("Data");
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

		public MutantApp(string pak, bool emulator) : base(pak, emulator) { }

		protected override void Start()
		{
			base.Start();

			EnableGestureManipulation = true;

			mutantNode = Scene.CreateChild();
			mutantNode.Position = new Vector3(0, 0, 1f);
			var mutantModelNode = mutantNode.CreateChild();
			mutantModelNode.SetScale(0.1f);
			var mutant = mutantModelNode.CreateComponent<AnimatedModel>();

			mutant.Model = ResourceCache.GetModel("Models/Mutant.mdl");
			mutant.SetMaterial(ResourceCache.GetMaterial("Materials/mutant_M.xml"));
			animation = mutantModelNode.CreateComponent<AnimationController>();
			PlayAnimation(IdleAni);

			RegisterCortanaCommands(new Dictionary<string, Action>
				{
					//play animations using Cortana
					{"idle", () => PlayAnimation(IdleAni)},
					{"kill", () => PlayAnimation(KillAni)},
					{"hip hop", () => PlayAnimation(HipHopAni)},
					{"jump", () => PlayAnimation(JumpAni)},
					{"jump attack", () => PlayAnimation(JumpAttack)},
					{"kick", () => PlayAnimation(KickAni)},
					{"punch", () => PlayAnimation(PunchAni)},
					{"run", () => PlayAnimation(RunAni)},
					{"swipe", () => PlayAnimation(SwipeAni)},
					{"walk", () => PlayAnimation(WalkAni)},

					{"bigger", () => mutantModelNode.ScaleNode(1.2f)},
					{"smaller", () => mutantModelNode.ScaleNode(0.8f)},
					{"increase the brightness", () => IncreaseBrightness(1.2f)},
					{"decrease the brightness", () => IncreaseBrightness(0.8f)},
				});
		}

		void IncreaseBrightness(float byValue)
		{
			//by default, HoloScene has two kinds of lights:
			CameraLight.Brightness *= byValue;
			DirectionalLight.Brightness *= byValue;
		}

		void PlayAnimation(string file, bool looped = true)
		{
			mutantNode.RemoveAllActions();

			if (file == WalkAni)
				mutantNode.RunActions(new RepeatForever(new MoveBy(1f, new Vector3(0, 0, -0.1f))));
			else if (file == RunAni)
				mutantNode.RunActions(new RepeatForever(new MoveBy(1f, new Vector3(0, 0, -0.3f))));

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
			mutantNode.Position = relativeHandPosition + monsterPositionBeforeManipulations;
		}
	}
}