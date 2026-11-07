using System;
using System.IO;
using ChaosFramework.Collections;
using ChaosFramework.Components;
using ChaosFramework.Graphics.Imaging;
using ChaosFramework.Graphics.Imaging.Formats;
using ChaosFramework.Graphics.OpenGl;
using ChaosFramework.Graphics.OpenGl.Text;
using ChaosFramework.Math;
using OpenTK.Graphics.OpenGL;
using static ChaosFramework.Math.Clamping;

namespace LD60.World
{
    public abstract class WorldScene
        : DeferredRenderingScene
    {
        public const float GRAVITY = -9.81f;

        public TransparencyRenderer transparency;
        public Transparents.ParticleFire fire;

        public readonly LinkedList<Text> hudTexts, worldTexts, materialTexts;
        LinkedList<Tuple<DrawLayers, string, LinkedList<Text>>> textLists = [];

        public ChaosFramework.Physics.PhysicsWorld physics { get; private set; }

        protected bool takeScreenshot = false;

        readonly Framebuffer intermediateFbo;
        public Texture postprocessingTexture => intermediateFbo.textures[0];

        public WorldScene(Game game, int width, int height)
            : base(game, width, height)
        {
            textLists.Add(new Tuple<DrawLayers, string, LinkedList<Text>>(DrawLayers.World, "World", worldTexts = []));
            textLists.Add(new Tuple<DrawLayers, string, LinkedList<Text>>(DrawLayers.Material, "Material", materialTexts = []));
            textLists.Add(new Tuple<DrawLayers, string, LinkedList<Text>>(DrawLayers.HUD, "HUD", hudTexts = []));

            transparency = new TransparencyRenderer(
                game.graphics,
                Max(1, (int)(deferredShader.width * Confix.instance.transparencyShaderScale)),
                Max(1, (int)(deferredShader.height * Confix.instance.transparencyShaderScale)),
                deferredShader.depthBuffer,
                (int)Confix.instance.numTransparencyLayers);
            transparency.solidDepthBias = 0.000025f;
            transparency.transparents.Add(fire = AddComponent<Transparents.ParticleFire>());

            CreatePhysics();

            intermediateFbo = new Framebuffer(deferredShader.width, deferredShader.height, [
                new Texture(
                    game.graphics.dispatcher,
                    new Texture.Parameters(
                       deferredShader.width,
                       deferredShader.height,
                       pixelType: PixelType.Float,
                       pixelFormat: PixelFormat.Rgba,
                       internalFormat: PixelInternalFormat.Rgba32f
                       )
                    )
                ],
                deferredShader.depthBuffer);
        }

        public void CreatePhysics()
        {
            physics?.Dispose();
            physics = AddComponent<ChaosFramework.Physics.PhysicsWorld>(CreateParameters.Create((int)UpdateLayers.Collision));
            physics.forces.Add(new ChaosFramework.Physics.Forces.LinearForce(0, GRAVITY, 0));
        }

        public override void SetUpdateCalls()
        {
            base.SetUpdateCalls();

            if (takeScreenshot)
                TakeScreenshot();
        }

        protected override Framebuffer GetIntermediateFbo()
            => intermediateFbo;

        void TakeScreenshot()
        {
            string picFolder = System.Environment.GetFolderPath(
                System.Environment.SpecialFolder.MyPictures,
                System.Environment.SpecialFolderOption.None
                );
            if (Directory.Exists(picFolder))
            {
                DirectoryInfo outFolder = new(Path.GetFullPath(picFolder) + "/ChaosTechnology/LD60/");
                FileInfo outFile;
                if (outFolder.Exists)
                    for (uint i = 0; (outFile = new FileInfo($"{outFolder.FullName}/{i}.png")).Exists; ++i);
                else
                {
                    outFolder.Create();
                    outFile = new FileInfo($"{outFolder.FullName}/0.png");
                }

                // allocate the file preemptively for name resolution sake
                Stream targetStr = outFile.Create();

                Rgba8Image img = game.graphics.TakeScreenshot(Draw);
                using (targetStr)
                    // TODO: do this in a background thread
                    Png.Save(img, targetStr);
            }
        }

        public override void SetDrawCalls()
        {
            foreach (Tuple<DrawLayers, string, LinkedList<Text>> textList in textLists)
                textList.Item3.Clear();

            base.SetDrawCalls();
            drawLayers[(int)DrawLayers.Transparents].Add(() => transparency.Render(deferredShader, intermediateFbo));

            foreach (Tuple<DrawLayers, string, LinkedList<Text>> textList in textLists)
                drawLayers[(int)textList.Item1].Add(() =>
                {
                    view.SetValues(game.graphics.shaders.managedText, Matrix.IDENTITY);
                    game.textRenderer.DrawTexts(game.textBuffer, game.graphics.shaders.managedText, textList.Item2, textList.Item3);
                });

        }

        protected override void DoDispose()
        {
            base.DoDispose();
            transparency.Dispose();
        }
    }
}
