using System;
using System.Collections.Generic;
using Urho;
using Urho.Holographics;

namespace Mutant
{
	public class MutantApp : HoloApplication
	{
		Node mutantNode;
		Vector3 monsterPositionBeforeManipulations;
		AnimationController animation;

		public MutantApp(string pak) : base(pak) { }

		protected override void Start()
		{
			base.Start();

			EnableGestureManipulation = true;

			mutantNode = Scene.CreateChild();
			SetMutantPosition(new Vector3(0, 0, 0.7f));
			var mutantModelNode = mutantNode.CreateChild();
			mutantModelNode.SetScale(0.1f);
			var mutant = mutantModelNode.CreateComponent<AnimatedModel>();

			mutant.Model = ResourceCache.GetModel("Mutant.mdl");
			mutant.SetMaterial(ResourceCache.GetMaterial("Materials/mutant_M.xml"));
			animation = mutantModelNode.CreateComponent<AnimationController>();

			RegisterCortanaCommands(new Dictionary<string, Action>
				{
					//play animations using Cortana
					{"idle", () => PlayAnimation("Mutant_Idle1.ani")},
					{"kill", () => PlayAnimation("Mutant_Death.ani")},
					{"hip hop", () => PlayAnimation("Mutant_HipHop1.ani")},
					{"jump", () => PlayAnimation("Mutant_Jump.ani")},
					{"jump attack", () => PlayAnimation("Mutant_JumpAttack.ani")},
					{"kick", () => PlayAnimation("Mutant_Kick.ani")},
					{"punch", () => PlayAnimation("Mutant_Punch.ani")},
					{"run", () => PlayAnimation("Mutant_Run.ani")},
					{"swipe", () => PlayAnimation("Mutant_Swipe.ani")},
					{"walk", () => PlayAnimation("Mutant_Walk.ani")},

					{"bigger", () => mutantModelNode.ScaleNode(1.2f)},
					{"smaller", () => mutantModelNode.ScaleNode(0.8f)},
					{"increase the brightness", () => IncreaseBrightness(1.2f)},
					{"decrease the brightness", () => IncreaseBrightness(0.2f)},
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
			animation.StopAll(0.2f);
			animation.Play(file, 0, looped, 0.2f);
		}

		// Handle spatial input gestures:

		public override void OnGestureManipulationStarted()
		{
			monsterPositionBeforeManipulations = mutantNode.Position;
		}

		public override void OnGestureManipulationUpdated(Vector3 relativeHandPosition)
		{
			SetMutantPosition(relativeHandPosition + monsterPositionBeforeManipulations);
		}

		void SetMutantPosition(Vector3 pos)
		{
			mutantNode.Position = pos;

			//for optical stabilization:
			FocusWorldPoint = mutantNode.WorldPosition;
		}
	}
}