using Silk.NET.Core.Native;
using Silk.NET.Direct3D.Compilers;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using System.Drawing;
using System.Runtime.CompilerServices;

namespace AbsEngine.Rendering.DirectX11
{
    internal class DirectX11Graphics : IGraphics
    {
        private readonly DXGI dxgi = null!;
        private readonly D3D11 d3d11 = null!;
        private readonly D3DCompiler compiler = null!;

        ComPtr<IDXGIFactory2> factory = default;
        ComPtr<IDXGISwapChain1> swapchain = default;
        ComPtr<ID3D11Device> device = default;
        ComPtr<ID3D11DeviceContext> deviceContext = default;

        public unsafe DirectX11Graphics(IWindow window, GraphicsAPIs gfxAPI)
        {
            if (gfxAPI != GraphicsAPIs.D3D11)
            {
                throw new ApplicationException("Invalid graphics API specified for DirectX11Graphics!");
            }

            dxgi = DXGI.GetApi(window);
            d3d11 = D3D11.GetApi(window);
            compiler = D3DCompiler.GetApi();

            SilkMarshal.ThrowHResult
            (
                d3d11.CreateDevice
                (
                    default(ComPtr<IDXGIAdapter>),
                    D3DDriverType.Hardware,
                    Software: default,
                    (uint)CreateDeviceFlag.Debug,
                    null,
                    0,
                    D3D11.SdkVersion,
                    ref device,
                    null,
                    ref deviceContext
                )
            );

            if (OperatingSystem.IsWindows())
            {
                // Log debug messages for this device (given that we've enabled the debug flag). Don't do this in release code!
                device.SetInfoQueueCallback(msg => Console.WriteLine(SilkMarshal.PtrToString((nint)msg.PDescription)));
            }

            var swapChainDesc = new SwapChainDesc1
            {
                BufferCount = 2, // double buffered
                Format = Format.FormatB8G8R8A8Unorm,
                BufferUsage = DXGI.UsageRenderTargetOutput,
                SwapEffect = SwapEffect.FlipDiscard,
                SampleDesc = new SampleDesc(1, 0)
            };

            factory = dxgi.CreateDXGIFactory<IDXGIFactory2>();

            SilkMarshal.ThrowHResult
            (
                factory.CreateSwapChainForHwnd
                (
                    device,
                    window.Native!.DXHandle!.Value,
                    in swapChainDesc,
                    null,
                    ref Unsafe.NullRef<IDXGIOutput>(),
                    ref swapchain
                )
            );
        }

        public GraphicsAPIs GraphicsAPIs => GraphicsAPIs.D3D11;

        public IShaderTranspiler ShaderTranspiler => throw new NotImplementedException();

        public unsafe void ClearScreen(Color colour)
        {
            // Obtain the framebuffer for the swapchain's backbuffer.
            using var framebuffer = swapchain.GetBuffer<ID3D11Texture2D>(0);

            // Create a view over the render target.
            ComPtr<ID3D11RenderTargetView> renderTargetView = default;
            SilkMarshal.ThrowHResult(device.CreateRenderTargetView(framebuffer, null, ref renderTargetView));

            var col = new Span<float>(new[] { colour.R / 255.0f, colour.G / 255.0f, colour.B / 255.0f, colour.A / 255.0f });

            deviceContext.ClearRenderTargetView(renderTargetView, col);
        }

        public void UpdateViewport(Vector2D<int> viewport)
        {
            // Implement updating the viewport here
            // Example: 
            var vp = new Viewport(0, 0, viewport.X, viewport.Y, 0, 1);
            deviceContext.RSSetViewports(1, in vp);
        }

        public void SetActiveDepthTest(bool enabled)
        {
            // Implement setting active depth test here
            // Example:
            //ComPtr<ID3D11DepthStencilState> depthStencilState = default;
            //var desc = new DepthStencilDesc(
            //    depthEnable: enabled,
            //    depthWriteMask: DepthWriteMask.All,
            //    depthFunc: ComparisonFunc.Less,
            //    stencilEnable: false,
            //    frontFace: new DepthStencilopDesc(
            //);
            //SilkMarshal.ThrowHResult
            //(
            //    device.CreateDepthStencilState(in desc, ref depthStencilState)
            //);
            //deviceContext.OMSetDepthStencilState(depthStencilState, 0);
        }

        public void DrawElements(uint length)
        {
            deviceContext.DrawIndexed(length, 0, 0);
        }

        public void Dispose()
        {
            dxgi?.Dispose();
            d3d11?.Dispose();
            compiler?.Dispose();
            factory.Dispose();
            swapchain.Dispose();
            device.Dispose();
            deviceContext.Dispose();
        }

        public void DrawArrays(uint length)
        {
            throw new NotImplementedException();
        }

        public void Swap()
        {
            throw new NotImplementedException();
        }
    }
}