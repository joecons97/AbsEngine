using AbsEngine.IO;
using AbsEngine.Rendering;
using AbsGameProject.Models;
using Silk.NET.Maths;
using System.Drawing;
using static System.Net.Mime.MediaTypeNames;

namespace AbsGameProject.Textures
{
    public static class TextureAtlas
    {
        static byte[]? _finalTex = null;

        static Vector4D<byte>?[] _tex;

        static Vector2D<int> _carat = new Vector2D<int>();

        public static Texture? AtlasTexture { get; set; }

        public static Dictionary<string, Rectangle<int>> BlockLocations = new Dictionary<string, Rectangle<int>>();

        public static int Size { get; private set; }
        public static int Offset { get; private set; }

        public static void Initialise(int size = 1024, int offset = 2)
        {
            Size = size;
            Offset = offset;

            _tex = new Vector4D<byte>?[size * size * 4];

            AtlasTexture = new Texture();
            AtlasTexture.SetMaxMips(3);
            AtlasTexture.WrapMode = TextureWrapMode.Repeat;
            AtlasTexture.MinFilter = TextureMinFilter.NearestMipmapNearest;
            AtlasTexture.MagFilter = TextureMagFilter.Nearest;
            AtlasTexture.PixelFormat = Silk.NET.OpenGL.PixelFormat.Rgba;
        }

        public static void InsertVoxelModel(VoxelModel model)
        {
            if (model == null) return;

            foreach (var texture in model.Textures)
            {
                if (string.IsNullOrEmpty(texture.Value))
                    throw new ArgumentNullException(nameof(texture));

                if (File.Exists(texture.Value) == false)
                    throw new FileNotFoundException(texture.Value);

                if (BlockLocations.ContainsKey(texture.Key))
                    continue;

                var imgResult = TextureLoader.LoadImageResult(texture.Value);

                if (_carat.Y + imgResult.Height > Size)
                    _carat.Y = 0;

                for (int x = 0; x < Size; x++)
                {
                    if (_tex[x * Size + _carat.Y] == null)
                    {
                        _carat.X = x + Offset;
                        break;
                    }
                }

                for (int x = 0; x < imgResult.Width * 4; x += 4)
                {
                    for (int y = 0; y < imgResult.Height * 4; y += 4)
                    {
                        int pixelIndex = x * imgResult.Width + y;
                        int texIndex = (_carat.X + x / 4) * Size + (_carat.Y + y / 4);
                        var pixel = new Vector4D<byte>(
                            imgResult.Data[pixelIndex],
                            imgResult.Data[pixelIndex + 1],
                            imgResult.Data[pixelIndex + 2],
                            imgResult.Data[pixelIndex + 3]);

                        _tex[texIndex] = pixel;
                    }
                }

                BlockLocations.Add(texture.Key, new Rectangle<int>(_carat.X, _carat.Y, imgResult.Width, imgResult.Height));

                _carat += new Vector2D<int>(0, imgResult.Height + Offset);
            }
        }

        public static void Build()
        {
            _finalTex = _tex.Select(pixel => new byte[] {
                pixel.GetValueOrDefault().X,
                pixel.GetValueOrDefault().Y,
                pixel.GetValueOrDefault().Z,
                pixel.GetValueOrDefault().W
            }).SelectMany(x => x).ToArray();

            AtlasTexture.SetPixels(_finalTex, Size, Size);
            AtlasTexture.Update();
        }
    }
}
