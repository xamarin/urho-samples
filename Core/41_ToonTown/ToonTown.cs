//
// Copyright (c) 2014-2015, THUNDERBEAST GAMES LLC All rights reserved
// Copyright (c) 2015 Xamarin Inc
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), 
// to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, 
// and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;

namespace Urho.Samples
{
	public class ToonTown : Sample
	{
		Scene scene;
		RoboMan character;

		const float CameraMinDist = 1.0f;
		const float CameraInitialDist = 5.0f;
		const float CameraMaxDist = 20.0f;
		const float YawSensitivity = 0.1f;

		public const int CtrlForward = 1;
		public const int CtrlBack = 2;
		public const int CtrlLeft = 4;
		public const int CtrlRight = 8;
		public const int CtrlJump = 16;

		public ToonTown(Context ctx) : base(ctx) { }
		
		public override void Start()
		{
			base.Start();
			CreateScene();
			CreateCharacter();
			SubscribeToEvents();
		}

		void CreateScene()
		{
			var cache = ResourceCache;
			scene = new Scene(Context);

			// Simply open a scene designed in the Atomic Game Engine:
			// https://habrastorage.org/files/3b5/fe9/5ba/3b5fe95ba61044dcbae77233f09748ad.png
			scene.LoadXmlFromCache(cache, "Scenes/ToonTown.scene");

			// Set up a viewport
			CameraNode = new Node(Context);
			Camera camera = CameraNode.CreateComponent<Camera>();
			camera.FarClip = 300.0f;
			Renderer.SetViewport(0, new Viewport(Context, scene, camera, null));

			// Load music:
			var musicFile = cache.GetSound("Music/StoryTime.ogg");
			musicFile.SetLooped(true);
			var musicNode = scene.CreateChild("MusicNode");
			var musicSource = musicNode.CreateComponent<SoundSource>();
			musicSource.Gain = 0.5f;
			musicSource.SetSoundType("Music");
			musicSource.Play(musicFile);
		}

		void SubscribeToEvents()
		{
			Engine.SubscribeToPostUpdate(HandlePostUpdate);
			scene.GetComponent<PhysicsWorld>().SubscribeToPhysicsPreStep(HandlePhysicsPreStep);
		}

		void HandlePhysicsPreStep(PhysicsPreStepEventArgs args)
		{
			character?.FixedUpdate(args.TimeStep);
		}

		protected override void OnUpdate(float timeStep)
		{
			Input input = Input;

			if (character != null)
			{
				// Clear previous controls
				character.Controls.Set(CtrlForward | CtrlBack | CtrlLeft | CtrlRight | CtrlJump, false);

				// Update controls using keys
				UI ui = UI;
				if (ui.FocusElement == null)
				{
					character.Controls.Set(CtrlForward, input.GetKeyDown(Key.W));
					character.Controls.Set(CtrlBack, input.GetKeyDown(Key.S));
					character.Controls.Set(CtrlLeft, input.GetKeyDown(Key.A));
					character.Controls.Set(CtrlRight, input.GetKeyDown(Key.D));
					character.Controls.Set(CtrlJump, input.GetKeyDown(Key.Space));

					// Add character yaw & pitch from the mouse motion or touch input
					if (TouchEnabled)
					{
						for (uint i = 0; i < input.NumTouches; ++i)
						{
							TouchState state = input.GetTouch(i);
							if (state.TouchedElement() != null)    // Touch on empty space
							{
								Camera camera = CameraNode.GetComponent<Camera>();
								if (camera == null)
									return;

								var graphics = Graphics;
								character.Controls.Yaw += TouchSensitivity * camera.Fov / graphics.Height * state.Delta.X;
								character.Controls.Pitch += TouchSensitivity * camera.Fov / graphics.Height * state.Delta.Y;
							}
						}
					}
					else
					{
						character.Controls.Yaw += (float)input.MouseMove.X * YawSensitivity;
						character.Controls.Pitch += (float)input.MouseMove.Y * YawSensitivity;
					}
					// Limit pitch
					character.Controls.Pitch = MathHelper.Clamp(character.Controls.Pitch, -80.0f, 80.0f);
				}

				// Set rotation already here so that it's updated every rendering frame instead of every physics frame
				if (character != null)
					character.Node.Rotation = Quaternion.FromAxisAngle(Vector3.UnitY, character.Controls.Yaw);
			}
		}

		void HandlePostUpdate(PostUpdateEventArgs args)
		{
			if (character == null)
				return;

			Node characterNode = character.Node;

			// Get camera lookat dir from character yaw + pitch
			Quaternion rot = characterNode.Rotation;
			Quaternion dir = rot * Quaternion.FromAxisAngle(Vector3.UnitX, character.Controls.Pitch);

			// Turn head to camera pitch, but limit to avoid unnatural animation
			Node headNode = characterNode.GetChild("Bip01_Head", true);
			float limitPitch = MathHelper.Clamp(character.Controls.Pitch, -45.0f, 45.0f);
			Quaternion headDir = rot * Quaternion.FromAxisAngle(new Vector3(1.0f, 0.0f, 0.0f), limitPitch);
			// This could be expanded to look at an arbitrary target, now just look at a point in front
			Vector3 headWorldTarget = headNode.WorldPosition + headDir * new Vector3(0.0f, 0.0f, 1.0f);
			headNode.LookAt(headWorldTarget, new Vector3(0.0f, 1.0f, 0.0f), TransformSpace.World);
			// Correct head orientation because LookAt assumes Z = forward, but the bone has been authored differently (Y = forward)
			headNode.Rotate(new Quaternion(0.0f, 90.0f, 90.0f), TransformSpace.Local);

			// Third person camera: position behind the character
			Vector3 aimPoint = characterNode.Position + rot * new Vector3(0.0f, 1.7f, 0.0f);

			// Collide camera ray with static physics objects (layer bitmask 2) to ensure we see the character properly
			Vector3 rayDir = dir * new Vector3(0f, 0f, -1f);
			float rayDistance = CameraInitialDist;

			PhysicsRaycastResult result = new PhysicsRaycastResult();
			scene.GetComponent<PhysicsWorld>().RaycastSingle(ref result, new Ray(aimPoint, rayDir), rayDistance, 2);
			if (result.Body != null)
				rayDistance = Math.Min(rayDistance, result.Distance);
			rayDistance = MathHelper.Clamp(rayDistance, CameraMinDist, CameraMaxDist);

			CameraNode.Position = aimPoint + rayDir * rayDistance;
			CameraNode.Rotation = dir;
		}
		
		void CreateCharacter()
		{
			var cache = ResourceCache;

			Node objectNode = scene.CreateChild("Jack");
			objectNode.Position = new Vector3(-58.68f, 39.21f, -7.08f);

			// Create the rendering component + animation controller
			AnimatedModel obj = objectNode.CreateComponent<AnimatedModel>();
			obj.Model = cache.GetModel("Models/Jack.mdl");
			var material = cache.GetMaterial("Materials/BlueGrid.xml");
			obj.SetMaterial(material);
			obj.CastShadows = true;
			objectNode.CreateComponent<AnimationController>();

			// Set the head bone for manual control
			//obj.Skeleton.GetBoneSafe("Bip01_Head").Animated = false;

			// Create rigidbody, and set non-zero mass so that the body becomes dynamic
			RigidBody body = objectNode.CreateComponent<RigidBody>();
			body.CollisionLayer = 1;
			body.Mass = 1.0f;

			// Set zero angular factor so that physics doesn't turn the character on its own.
			// Instead we will control the character yaw manually
			body.SetAngularFactor(Vector3.Zero);

			// Set the rigidbody to signal collision also when in rest, so that we get ground collisions properly
			body.CollisionEventMode = CollisionEventMode.Always;

			// Set a capsule shape for collision
			CollisionShape shape = objectNode.CreateComponent<CollisionShape>();
			shape.SetCapsule(0.7f, 1.8f, new Vector3(0.0f, 0.9f, 0.0f), Quaternion.Identity);

			// Create the character logic component, which takes care of steering the rigidbody
			// Remember it so that we can set the controls. Use a WeakPtr because the scene hierarchy already owns it
			// and keeps it alive as long as it's not removed from the hierarchy
			character = new RoboMan(Context);
			objectNode.AddComponent(character);
			character.Start();
		}
	}
}
