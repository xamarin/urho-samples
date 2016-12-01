namespace Urho.Samples
{
	public class PBRMaterials : Sample
	{
		Scene scene;

		public PBRMaterials(ApplicationOptions options = null) : base(options) { }

		protected override void Start()
		{
			Application.UnhandledException += Application_UnhandledException;

			base.Start();

			// Create the scene content
			CreateScene();

			// Setup the viewport for displaying the scene
			SetupViewport();
		}

		void Application_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			e.Handled = true;
		}

		void CreateScene()
		{
			scene = new Scene();

			// Load scene content prepared in the editor (XML format). GetFile() returns an open file from the resource system
			// which scene.LoadXML() will read
			scene.LoadXml(FileSystem.ProgramDir + "Data/Scenes/PBRExample.xml");

			// Create the camera (not included in the scene file)
			CameraNode = scene.CreateChild("Camera");
			CameraNode.CreateComponent<Camera>();

			// Set an initial position for the camera scene node above the plane
			CameraNode.Position = new Vector3(0.0f, 4.0f, 0.0f);
		}

		protected override void OnUpdate(float timeStep)
		{
			SimpleMoveCamera3D(timeStep);
		}

		void SetupViewport()
		{
			Viewport viewport = new Viewport(scene, CameraNode.GetComponent<Camera>(), null);
			Renderer.SetViewport(0, viewport);

			var effectRenderPath = viewport.RenderPath.Clone();
			effectRenderPath.Append(ResourceCache.GetXmlFile("PostProcess/BloomHDR.xml"));
			effectRenderPath.Append(ResourceCache.GetXmlFile("PostProcess/FXAA2.xml"));
			effectRenderPath.Append(ResourceCache.GetXmlFile("PostProcess/GammaCorrection.xml"));

			viewport.RenderPath = effectRenderPath;
		}
	}
}
