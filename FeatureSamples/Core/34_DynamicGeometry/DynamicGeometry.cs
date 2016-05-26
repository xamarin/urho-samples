//
// Copyright (c) 2008-2015 the Urho3D project.
// Copyright (c) 2015 Xamarin Inc
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Urho.IO;

namespace Urho.Samples
{
	public class DynamicGeometry : Sample
	{
		Scene scene;
		float time;
		bool animate = true;
		uint[] vertexDuplicates;
		readonly List<Vector3> originalVertices = new List<Vector3>();
		readonly List<VertexBuffer> animatingBuffers = new List<VertexBuffer>();

		public DynamicGeometry(ApplicationOptions options = null) : base(options) { }

		protected override void Start()
		{
			base.Start();
			CreateScene();
			SimpleCreateInstructionsWithWasd("\nSpace to toggle animation");
			SetupViewport();
		}

		protected override void OnUpdate(float timeStep)
		{
			base.OnUpdate(timeStep);
			SimpleMoveCamera3D(timeStep);
			if (Input.GetKeyPress(Key.Space))
				animate = !animate;

			if (animate)
				AnimateObjects(timeStep);
		}

		void AnimateObjects(float timeStep)
		{
			time += timeStep * 5.0f;

			// Repeat for each of the cloned vertex buffers
			for (int i = 0; i < animatingBuffers.Count; ++i)
			{
				float startPhase = time + i * 30.0f;
				VertexBuffer buffer = animatingBuffers[i];

				IntPtr vertexRawData = buffer.Lock(0, buffer.VertexCount, false);
				if (vertexRawData != IntPtr.Zero)
				{
					uint numVertices = buffer.VertexCount;
					uint vertexSize = buffer.VertexSize;
					// Copy the original vertex positions
					for (int j = 0; j < numVertices; ++j)
					{
						float phase = startPhase + vertexDuplicates[j] * 10.0f;
						var src = originalVertices[j];

						unsafe
						{
							//TODO: avoid unsafe
							Vector3* dest = (Vector3*)IntPtr.Add(vertexRawData, j * (int)vertexSize);

							dest->X = src.X * (1.0f + 0.1f * (float)Math.Sin(phase));
							dest->Y = src.Y * (1.0f + 0.1f * (float)Math.Sin(phase + 60.0f));
							dest->Z = src.Z * (1.0f + 0.1f * (float)Math.Sin(phase + 120.0f));
						}
					}
					buffer.Unlock();
				}
			}
		}

		void SetupViewport()
		{
			var renderer = Renderer;
			renderer.SetViewport(0, new Viewport(Context, scene, CameraNode.GetComponent<Camera>(), null));
		}

		void CreateScene()
		{
			var cache = ResourceCache;

			scene = new Scene();

			// Create the Octree component to the scene so that drawable objects can be rendered. Use default volume
			// (-1000, -1000, -1000) to (1000, 1000, 1000)
			scene.CreateComponent<Octree>();

			// Create a Zone for ambient light & fog control
			Node zoneNode = scene.CreateChild("Zone");
			Zone zone = zoneNode.CreateComponent<Zone>();
			zone.SetBoundingBox(new BoundingBox(-1000.0f, 1000.0f));
			zone.FogColor = new Color(0.2f, 0.2f, 0.2f);
			zone.FogStart = 200.0f;
			zone.FogEnd = 300.0f;

			// Create a directional light
			Node lightNode = scene.CreateChild("DirectionalLight");
			lightNode.SetDirection(new Vector3(-0.6f, -1.0f, -0.8f)); // The direction vector does not need to be normalized
			Light light = lightNode.CreateComponent<Light>();
			light.LightType = LightType.Directional;
			light.Color = new Color(0.4f, 1.0f, 0.4f);
			light.SpecularIntensity = (1.5f);

			// Get the original model and its unmodified vertices, which are used as source data for the animation
			Model originalModel = cache.GetModel("Models/Box.mdl");
			if (originalModel == null)
			{
				Log.Write(LogLevel.Error, "Model not found, cannot initialize example scene");
				return;
			}
			// Get the vertex buffer from the first geometry's first LOD level
			VertexBuffer buffer = originalModel.GetGeometry(0, 0).GetVertexBuffer(0);
			IntPtr vertexRawData = buffer.Lock(0, buffer.VertexCount, false);

			if (vertexRawData != IntPtr.Zero)
			{
				uint numVertices = buffer.VertexCount;
				uint vertexSize = buffer.VertexSize;
				// Copy the original vertex positions
				for (int i = 0; i < numVertices; ++i)
				{
					var src = (Vector3)Marshal.PtrToStructure(IntPtr.Add(vertexRawData, i * (int)vertexSize), typeof(Vector3));
					originalVertices.Add(src);
				}
				buffer.Unlock();

				// Detect duplicate vertices to allow seamless animation
				vertexDuplicates = new uint[originalVertices.Count];
				for (int i = 0; i < originalVertices.Count; ++i)
				{
					vertexDuplicates[i] = (uint)i; // Assume not a duplicate
					for (int j = 0; j < i; ++j)
					{
						if (originalVertices[i].Equals(originalVertices[j]))
						{
							vertexDuplicates[i] = (uint)j;
							break;
						}
					}
				}
			}
			else
			{
				Log.Write(LogLevel.Error, "Failed to lock the model vertex buffer to get original vertices");
				return;
			}

			// Create StaticModels in the scene. Clone the model for each so that we can modify the vertex data individually
			for (int y = -1; y <= 1; ++y)
			{
				for (int x = -1; x <= 1; ++x)
				{
					Node node = scene.CreateChild("Object");
					node.Position = (new Vector3(x * 2.0f, 0.0f, y * 2.0f));
					StaticModel sm = node.CreateComponent<StaticModel>();
					Model cloneModel = originalModel.Clone();
					sm.Model = (cloneModel);
					// Store the cloned vertex buffer that we will modify when animating
					animatingBuffers.Add(cloneModel.GetGeometry(0, 0).GetVertexBuffer(0));
				}
			}

			// Finally create one model (pyramid shape) and a StaticModel to display it from scratch
			// Note: there are duplicated vertices to enable face normals. We will calculate normals programmatically
			{
				const uint numVertices = 18;
				float[] vertexData =
				{
					// Position             Normal
					0.0f, 0.5f, 0.0f,       0.0f, 0.0f, 0.0f,
					0.5f, -0.5f, 0.5f,      0.0f, 0.0f, 0.0f,
					0.5f, -0.5f, -0.5f,     0.0f, 0.0f, 0.0f,

					0.0f, 0.5f, 0.0f,       0.0f, 0.0f, 0.0f,
					-0.5f, -0.5f, 0.5f,     0.0f, 0.0f, 0.0f,
					0.5f, -0.5f, 0.5f,      0.0f, 0.0f, 0.0f,

					0.0f, 0.5f, 0.0f,       0.0f, 0.0f, 0.0f,
					-0.5f, -0.5f, -0.5f,    0.0f, 0.0f, 0.0f,
					-0.5f, -0.5f, 0.5f,     0.0f, 0.0f, 0.0f,

					0.0f, 0.5f, 0.0f,       0.0f, 0.0f, 0.0f,
					0.5f, -0.5f, -0.5f,     0.0f, 0.0f, 0.0f,
					-0.5f, -0.5f, -0.5f,    0.0f, 0.0f, 0.0f,

					0.5f, -0.5f, -0.5f,     0.0f, 0.0f, 0.0f,
					0.5f, -0.5f, 0.5f,      0.0f, 0.0f, 0.0f,
					-0.5f, -0.5f, 0.5f,     0.0f, 0.0f, 0.0f,

					0.5f, -0.5f, -0.5f,     0.0f, 0.0f, 0.0f,
					-0.5f, -0.5f, 0.5f,     0.0f, 0.0f, 0.0f,
					-0.5f, -0.5f, -0.5f,    0.0f, 0.0f, 0.0f
				};

				short[] indexData =
				{
					0, 1, 2,
					3, 4, 5,
					6, 7, 8,
					9, 10, 11,
					12, 13, 14,
					15, 16, 17
				};

				Model fromScratchModel = new Model();
				VertexBuffer vb = new VertexBuffer(Context, false);
				IndexBuffer ib = new IndexBuffer(Context, false);
				Geometry geom = new Geometry();

				// Shadowed buffer needed for raycasts to work, and so that data can be automatically restored on device loss
				vb.Shadowed = true;
				vb.SetSize(numVertices, ElementMask.Position | ElementMask.Normal, false);
				vb.SetData(vertexData);

				ib.Shadowed = true;
				ib.SetSize(numVertices, false, false);
				ib.SetData(indexData);

				geom.SetVertexBuffer(0, vb);
				geom.IndexBuffer = ib;
				geom.SetDrawRange(PrimitiveType.TriangleList, 0, numVertices, true);

				fromScratchModel.NumGeometries = 1;
				fromScratchModel.SetGeometry(0, 0, geom);
				fromScratchModel.BoundingBox = new BoundingBox(new Vector3(-0.5f, -0.5f, -0.5f), new Vector3(0.5f, 0.5f, 0.5f));

				Node node = scene.CreateChild("FromScratchObject");
				node.Position = (new Vector3(0.0f, 3.0f, 0.0f));
				StaticModel sm = node.CreateComponent<StaticModel>();
				sm.Model = fromScratchModel;
			}

			// Create the camera
			CameraNode = new Node();
			CameraNode.Position = (new Vector3(0.0f, 2.0f, -20.0f));
			Camera camera = CameraNode.CreateComponent<Camera>();
			camera.FarClip = 300.0f;
		}
	}
}
